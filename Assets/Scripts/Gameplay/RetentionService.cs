using System;
using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    public class RetentionService : MonoBehaviour, IRetentionService
    {
        private static readonly int[] StreakRewards = { 50, 75, 100, 150, 200, 300, 500 };
        private const double CooldownHours = 20.0;
        private const double StreakExpiryHours = 48.0;

        public event Action OnRetentionStateChanged;

        private CollectionManager collectionManager;

        private void Awake()
        {
            ServiceLocator.Register<IRetentionService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IRetentionService>();
        }

        private void Start()
        {
            EnsureManager();
            CheckDailyReset();
        }

        private void EnsureManager()
        {
            if (collectionManager == null)
            {
                collectionManager = CollectionManager.Instance ?? FindFirstObjectByType<CollectionManager>();
            }
        }

        private PlayerCollectionData GetData()
        {
            EnsureManager();
            return collectionManager != null ? collectionManager.Data : null;
        }

        public int CurrentStreak
        {
            get
            {
                var d = GetData();
                return d != null ? Mathf.Clamp(d.dailyStreak, 0, 7) : 0;
            }
        }

        public int NextDailyRewardCoins
        {
            get
            {
                int index = Mathf.Clamp(CurrentStreak, 0, StreakRewards.Length - 1);
                return StreakRewards[index];
            }
        }

        public bool CanClaimDailyReward
        {
            get
            {
                var d = GetData();
                if (d == null) return false;
                if (string.IsNullOrEmpty(d.lastDailyClaimUtc)) return true;

                if (DateTime.TryParse(d.lastDailyClaimUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastClaim))
                {
                    TimeSpan elapsed = DateTime.UtcNow - lastClaim;
                    return elapsed.TotalHours >= CooldownHours;
                }
                return true;
            }
        }

        public TimeSpan TimeUntilNextDaily
        {
            get
            {
                var d = GetData();
                if (d == null || string.IsNullOrEmpty(d.lastDailyClaimUtc)) return TimeSpan.Zero;

                if (DateTime.TryParse(d.lastDailyClaimUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastClaim))
                {
                    TimeSpan elapsed = DateTime.UtcNow - lastClaim;
                    if (elapsed.TotalHours >= CooldownHours) return TimeSpan.Zero;
                    return TimeSpan.FromHours(CooldownHours) - elapsed;
                }
                return TimeSpan.Zero;
            }
        }

        public bool ClaimDailyReward(out int coinsAwarded)
        {
            coinsAwarded = 0;
            if (!CanClaimDailyReward) return false;

            var d = GetData();
            if (d == null) return false;

            // Check if streak broke (>48h)
            if (!string.IsNullOrEmpty(d.lastDailyClaimUtc) &&
                DateTime.TryParse(d.lastDailyClaimUtc, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastClaim))
            {
                if ((DateTime.UtcNow - lastClaim).TotalHours > StreakExpiryHours)
                {
                    d.dailyStreak = 0;
                }
            }

            coinsAwarded = NextDailyRewardCoins;
            d.dailyStreak = (d.dailyStreak % 7) + 1;
            d.lastDailyClaimUtc = DateTime.UtcNow.ToString("O");

            // Award currency
            if (ServiceLocator.TryGet<IEconomyService>(out var eco))
            {
                eco.AwardCoins(coinsAwarded);
            }
            else if (collectionManager != null)
            {
                collectionManager.AddCoins(coinsAwarded);
            }

            collectionManager?.ForceSave();
            OnRetentionStateChanged?.Invoke();
            return true;
        }

        public IReadOnlyList<DailyQuestData> GetActiveQuests()
        {
            CheckDailyReset();
            var d = GetData();
            return d != null ? d.activeQuests : new List<DailyQuestData>();
        }

        public bool ClaimQuestReward(string questId, out int coinsAwarded)
        {
            coinsAwarded = 0;
            var d = GetData();
            if (d == null || d.activeQuests == null) return false;

            DailyQuestData quest = d.activeQuests.Find(q => q.id == questId);
            if (quest == null || !quest.IsComplete || quest.isClaimed) return false;

            quest.isClaimed = true;
            coinsAwarded = quest.rewardCoins;

            if (ServiceLocator.TryGet<IEconomyService>(out var eco))
            {
                eco.AwardCoins(coinsAwarded);
            }
            else if (collectionManager != null)
            {
                collectionManager.AddCoins(coinsAwarded);
            }

            collectionManager?.ForceSave();
            OnRetentionStateChanged?.Invoke();
            return true;
        }

        public void RecordDrop()
        {
            var d = GetData();
            if (d == null || d.activeQuests == null) return;

            bool changed = false;
            for (int i = 0; i < d.activeQuests.Count; i++)
            {
                var q = d.activeQuests[i];
                if (q.id == "quest_drops" && !q.IsComplete)
                {
                    q.currentProgress++;
                    changed = true;
                }
            }

            if (changed)
            {
                collectionManager?.ForceSave();
                OnRetentionStateChanged?.Invoke();
            }
        }

        public void RecordPrizeWon(string machineId, int rarity)
        {
            var d = GetData();
            if (d == null || d.activeQuests == null) return;

            bool changed = false;
            for (int i = 0; i < d.activeQuests.Count; i++)
            {
                var q = d.activeQuests[i];
                if (q.id == "quest_wins" && !q.IsComplete)
                {
                    q.currentProgress++;
                    changed = true;
                }
                else if (q.id == "quest_rarity" && !q.IsComplete && rarity >= 1) // 1=Rare, 2=Secret
                {
                    q.currentProgress++;
                    changed = true;
                }
            }

            if (changed)
            {
                collectionManager?.ForceSave();
                OnRetentionStateChanged?.Invoke();
            }
        }

        private void CheckDailyReset()
        {
            var d = GetData();
            if (d == null) return;

            string todayKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (d.activeQuests == null || d.activeQuests.Count == 0 || d.lastQuestDateUtc != todayKey)
            {
                d.lastQuestDateUtc = todayKey;
                d.activeQuests = new List<DailyQuestData>
                {
                    new DailyQuestData
                    {
                        id = "quest_drops",
                        title = "🎯 Target Practice",
                        description = "Trigger 5 claw drops",
                        targetGoal = 5,
                        currentProgress = 0,
                        rewardCoins = 40,
                        isClaimed = false
                    },
                    new DailyQuestData
                    {
                        id = "quest_wins",
                        title = "🧸 Prize Collector",
                        description = "Win 2 prizes",
                        targetGoal = 2,
                        currentProgress = 0,
                        rewardCoins = 60,
                        isClaimed = false
                    },
                    new DailyQuestData
                    {
                        id = "quest_rarity",
                        title = "✨ Jackpot Hunter",
                        description = "Catch 1 Rare or Secret prize",
                        targetGoal = 1,
                        currentProgress = 0,
                        rewardCoins = 100,
                        isClaimed = false
                    }
                };
                collectionManager?.ForceSave();
                OnRetentionStateChanged?.Invoke();
            }
        }
    }
}
