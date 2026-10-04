using System;
using System.Collections.Generic;

namespace ClawMachine.Core.Services
{
    [Serializable]
    public class DailyQuestData
    {
        public string id;
        public string title;
        public string description;
        public int currentProgress;
        public int targetGoal;
        public int rewardCoins;
        public bool isClaimed;

        public bool IsComplete => currentProgress >= targetGoal;
        public float ProgressNormalized => targetGoal > 0 ? (float)currentProgress / targetGoal : 0f;
    }

    [Serializable]
    public class MilestoneRewardData
    {
        public string id;
        public string title;
        public string hint;
        public int rewardCoins;
        public bool isUnlocked;
        public bool isClaimed;
    }

    public interface IRetentionService
    {
        int CurrentStreak { get; }
        int NextDailyRewardCoins { get; }
        bool CanClaimDailyReward { get; }
        TimeSpan TimeUntilNextDaily { get; }
        bool HasAnyClaimableReward { get; }

        event Action OnRetentionStateChanged;

        bool ClaimDailyReward(out int coinsAwarded);
        IReadOnlyList<DailyQuestData> GetActiveQuests();
        bool ClaimQuestReward(string questId, out int coinsAwarded);
        IReadOnlyList<MilestoneRewardData> GetMilestones();
        bool ClaimMilestoneReward(string milestoneId, out int coinsAwarded);
        void RecordDrop();
        void RecordPrizeWon(string machineId, int rarity);
    }
}
