using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class ProgressModalUI : MonoBehaviour
    {
        [Header("Modal & Toggle")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text coinsText;

        [Header("Tabs")]
        [SerializeField] private Button upgradesTabBtn;
        [SerializeField] private Button rewardsTabBtn;
        [SerializeField] private Image upgradesTabBg;
        [SerializeField] private Image rewardsTabBg;
        [SerializeField] private GameObject upgradesView;
        [SerializeField] private GameObject rewardsView;

        [Header("Upgrades Tab Controls")]
        [SerializeField] private Text setBonusText;
        [SerializeField] private Text trolleyLevelText;
        [SerializeField] private Text trolleyCostText;
        [SerializeField] private Button trolleyBuyBtn;
        [SerializeField] private Text gripLevelText;
        [SerializeField] private Text gripCostText;
        [SerializeField] private Button gripBuyBtn;
        [SerializeField] private Text dropLevelText;
        [SerializeField] private Text dropCostText;
        [SerializeField] private Button dropBuyBtn;

        [Header("Rewards Tab Controls")]
        [SerializeField] private Text streakHeaderSubtitle;
        [SerializeField] private Text[] streakDayCards; // 7 text elements
        [SerializeField] private Button claimDailyBtn;
        [SerializeField] private Text claimDailyBtnText;

        [Header("Quest Cards")]
        [SerializeField] private Text[] questTitleTexts;     // 3 elements
        [SerializeField] private Text[] questDescTexts;      // 3 elements
        [SerializeField] private Text[] questProgressTexts;  // 3 elements
        [SerializeField] private Button[] questClaimBtns;    // 3 elements
        [SerializeField] private Text[] questClaimBtnTexts;  // 3 elements

        private int activeTabIndex = 0; // 0 = Upgrades, 1 = Rewards
        private float timerUpdateInterval = 0.5f;
        private float nextTimerUpdate = 0f;

        private void Awake()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(OpenModal);
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);

            if (upgradesTabBtn != null) upgradesTabBtn.onClick.AddListener(() => SwitchTab(0));
            if (rewardsTabBtn != null) rewardsTabBtn.onClick.AddListener(() => SwitchTab(1));

            if (trolleyBuyBtn != null) trolleyBuyBtn.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.TrolleySpeed));
            if (gripBuyBtn != null) gripBuyBtn.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.GripPower));
            if (dropBuyBtn != null) dropBuyBtn.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.DropPrecision));

            if (claimDailyBtn != null) claimDailyBtn.onClick.AddListener(OnClaimDaily);

            if (questClaimBtns != null)
            {
                for (int i = 0; i < questClaimBtns.Length; i++)
                {
                    int index = i;
                    questClaimBtns[i].onClick.AddListener(() => OnClaimQuest(index));
                }
            }

            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged += HandleCoinsChanged;
                econ.OnUpgradePurchased += HandleUpgradePurchased;
            }
            if (ServiceLocator.TryGet<IRetentionService>(out var ret))
            {
                ret.OnRetentionStateChanged += HandleRetentionChanged;
            }
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged -= HandleCoinsChanged;
                econ.OnUpgradePurchased -= HandleUpgradePurchased;
            }
            if (ServiceLocator.TryGet<IRetentionService>(out var ret))
            {
                ret.OnRetentionStateChanged -= HandleRetentionChanged;
            }
        }

        public void OpenModal()
        {
            if (modalPanel != null) modalPanel.SetActive(true);
            SwitchTab(activeTabIndex);
        }

        public void CloseModal()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        public void SwitchTab(int tabIndex)
        {
            activeTabIndex = tabIndex;

            if (upgradesView != null) upgradesView.SetActive(tabIndex == 0);
            if (rewardsView != null) rewardsView.SetActive(tabIndex == 1);

            // Tab visual highlight
            Color activeColor = new Color(0.24f, 0.42f, 0.70f);
            Color inactiveColor = new Color(0.14f, 0.16f, 0.22f);

            if (upgradesTabBg != null) upgradesTabBg.color = tabIndex == 0 ? activeColor : inactiveColor;
            if (rewardsTabBg != null) rewardsTabBg.color = tabIndex == 1 ? activeColor : inactiveColor;

            RefreshUI();
        }

        private void Update()
        {
            if (modalPanel == null || !modalPanel.activeSelf) return;

            if (Time.time >= nextTimerUpdate)
            {
                nextTimerUpdate = Time.time + timerUpdateInterval;
                if (activeTabIndex == 1)
                {
                    RefreshDailyClaimButton();
                }
            }
        }

        private void RefreshUI()
        {
            RefreshCoins();
            if (activeTabIndex == 0)
            {
                RefreshUpgradesTab();
            }
            else
            {
                RefreshRewardsTab();
            }
        }

        private void RefreshCoins()
        {
            if (coinsText == null) return;
            int coins = 0;
            if (ServiceLocator.TryGet<IEconomyService>(out var econ)) coins = econ.Coins;
            else if (CollectionManager.Instance != null) coins = CollectionManager.Instance.Coins;
            coinsText.text = $"🪙 {coins:N0}";
        }

        // ==================== UPGRADES TAB ====================
        private void RefreshUpgradesTab()
        {
            IEconomyService econ = null;
            ServiceLocator.TryGet<IEconomyService>(out econ);
            if (econ == null && CollectionManager.Instance != null) econ = CollectionManager.Instance;
            if (econ == null) return;

            ICollectionService coll = null;
            ServiceLocator.TryGet<ICollectionService>(out coll);
            var mach = coll != null ? coll.CurrentMachine : (CollectionManager.Instance != null ? CollectionManager.Instance.CurrentMachine : null);

            int currentCoins = econ.Coins;

            // Speed
            int spdLvl = econ.GetUpgradeLevel(UpgradeType.TrolleySpeed, mach);
            int spdCost = econ.GetUpgradeCost(UpgradeType.TrolleySpeed, mach);
            bool spdMax = econ.IsUpgradeMaxed(UpgradeType.TrolleySpeed, mach);
            if (trolleyLevelText != null) trolleyLevelText.text = $"LVL {spdLvl}/5";
            if (trolleyCostText != null) trolleyCostText.text = spdMax ? "MAX" : $"{spdCost} 🪙";
            if (trolleyBuyBtn != null) trolleyBuyBtn.interactable = !spdMax && currentCoins >= spdCost;

            // Grip
            int grpLvl = econ.GetUpgradeLevel(UpgradeType.GripPower, mach);
            int grpCost = econ.GetUpgradeCost(UpgradeType.GripPower, mach);
            bool grpMax = econ.IsUpgradeMaxed(UpgradeType.GripPower, mach);
            if (gripLevelText != null) gripLevelText.text = $"LVL {grpLvl}/5";
            if (gripCostText != null) gripCostText.text = grpMax ? "MAX" : $"{grpCost} 🪙";
            if (gripBuyBtn != null) gripBuyBtn.interactable = !grpMax && currentCoins >= grpCost;

            // Precision
            int prcLvl = econ.GetUpgradeLevel(UpgradeType.DropPrecision, mach);
            int prcCost = econ.GetUpgradeCost(UpgradeType.DropPrecision, mach);
            bool prcMax = econ.IsUpgradeMaxed(UpgradeType.DropPrecision, mach);
            if (dropLevelText != null) dropLevelText.text = $"LVL {prcLvl}/5";
            if (dropCostText != null) dropCostText.text = prcMax ? "MAX" : $"{prcCost} 🪙";
            if (dropBuyBtn != null) dropBuyBtn.interactable = !prcMax && currentCoins >= prcCost;

            // Set Bonus
            if (setBonusText != null)
            {
                bool owns = coll != null && mach != null && coll.IsMachineOwned(mach.machineId);
                setBonusText.text = owns ? "★ SET BONUS ACTIVE: +50% SPEED/GRIP ★" : "SET BONUS: COMPLETE COLLECTION TO UNLOCK +50% BOOST";
                setBonusText.color = owns ? new Color(1f, 0.85f, 0.2f) : new Color(0.6f, 0.6f, 0.65f);
            }
        }

        private void OnBuyUpgrade(UpgradeType type)
        {
            bool success = false;
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                success = econ.TryPurchaseUpgrade(type);
            }
            else if (CollectionManager.Instance != null)
            {
                success = CollectionManager.Instance.TryPurchaseUpgrade(type);
            }

            if (success)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayGrabSuccess();
                RefreshUI();
            }
        }

        // ==================== REWARDS TAB ====================
        private void RefreshRewardsTab()
        {
            if (!ServiceLocator.TryGet<IRetentionService>(out var ret)) return;

            int streak = ret.CurrentStreak;
            if (streakHeaderSubtitle != null)
            {
                streakHeaderSubtitle.text = $"CURRENT STREAK: {streak}/7 DAYS";
            }

            // Streak 7 cards
            int[] rewards = { 50, 75, 100, 150, 200, 300, 500 };
            if (streakDayCards != null)
            {
                for (int i = 0; i < streakDayCards.Length; i++)
                {
                    if (streakDayCards[i] != null && i < rewards.Length)
                    {
                        string check = (i < streak) ? "✓ " : "";
                        streakDayCards[i].text = $"{check}DAY {i + 1}\n{rewards[i]} 🪙";
                    }
                }
            }

            RefreshDailyClaimButton();
            RefreshQuests();
        }

        private void RefreshDailyClaimButton()
        {
            if (claimDailyBtn == null || claimDailyBtnText == null) return;
            if (!ServiceLocator.TryGet<IRetentionService>(out var ret)) return;

            if (ret.CanClaimDailyReward)
            {
                claimDailyBtn.interactable = true;
                claimDailyBtnText.text = $"★ CLAIM DAY {ret.CurrentStreak + 1} REWARD (+{ret.NextDailyRewardCoins} 🪙) ★";
            }
            else
            {
                claimDailyBtn.interactable = false;
                TimeSpan rem = ret.TimeUntilNextDaily;
                claimDailyBtnText.text = $"NEXT REWARD IN: {rem.Hours:D2}h {rem.Minutes:D2}m {rem.Seconds:D2}s";
            }
        }

        private void RefreshQuests()
        {
            if (!ServiceLocator.TryGet<IRetentionService>(out var ret)) return;
            var quests = ret.GetActiveQuests();

            for (int i = 0; i < 3; i++)
            {
                if (i >= quests.Count) break;
                var q = quests[i];

                if (questTitleTexts != null && i < questTitleTexts.Length && questTitleTexts[i] != null)
                {
                    questTitleTexts[i].text = q.title;
                }
                if (questDescTexts != null && i < questDescTexts.Length && questDescTexts[i] != null)
                {
                    questDescTexts[i].text = q.description;
                }
                if (questProgressTexts != null && i < questProgressTexts.Length && questProgressTexts[i] != null)
                {
                    questProgressTexts[i].text = $"{q.currentProgress} / {q.targetGoal}";
                }
                if (questClaimBtns != null && i < questClaimBtns.Length && questClaimBtns[i] != null)
                {
                    bool canClaim = q.IsComplete && !q.isClaimed;
                    questClaimBtns[i].interactable = canClaim;

                    if (questClaimBtnTexts != null && i < questClaimBtnTexts.Length && questClaimBtnTexts[i] != null)
                    {
                        if (q.isClaimed) questClaimBtnTexts[i].text = "CLAIMED ✓";
                        else if (canClaim) questClaimBtnTexts[i].text = $"CLAIM +{q.rewardCoins} 🪙";
                        else questClaimBtnTexts[i].text = $"+{q.rewardCoins} 🪙";
                    }
                }
            }
        }

        private void OnClaimDaily()
        {
            if (ServiceLocator.TryGet<IRetentionService>(out var ret))
            {
                if (ret.ClaimDailyReward(out int coins))
                {
                    if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayWin();
                    ClawJuiceEffects.Instance?.PlayWinCelebration(transform.position);
                    RefreshUI();
                }
            }
        }

        private void OnClaimQuest(int questIndex)
        {
            if (ServiceLocator.TryGet<IRetentionService>(out var ret))
            {
                var quests = ret.GetActiveQuests();
                if (questIndex >= 0 && questIndex < quests.Count)
                {
                    if (ret.ClaimQuestReward(quests[questIndex].id, out int coins))
                    {
                        if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayWin();
                        RefreshUI();
                    }
                }
            }
        }

        private void HandleCoinsChanged(int newCoins) => RefreshUI();
        private void HandleUpgradePurchased(UpgradeType type, int level) => RefreshUI();
        private void HandleRetentionChanged() => RefreshUI();
    }
}
