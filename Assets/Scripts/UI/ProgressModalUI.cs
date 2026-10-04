using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class MilestoneCardTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public string hintMessage = "";
        public Action<string> onHoverAction;
        public Action onExitAction;

        public void OnPointerEnter(PointerEventData eventData) => onHoverAction?.Invoke(hintMessage);
        public void OnPointerExit(PointerEventData eventData) => onExitAction?.Invoke();
        public void OnPointerClick(PointerEventData eventData) => onHoverAction?.Invoke(hintMessage);
    }

    public class ProgressModalUI : MonoBehaviour
    {
        [Header("Modal & Toggle")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text coinsText;
        [SerializeField] private GameObject marqueeNotificationPip;

        [Header("Tabs")]
        [SerializeField] private Button upgradesTabBtn;
        [SerializeField] private Button rewardsTabBtn;
        [SerializeField] private Image upgradesTabBg;
        [SerializeField] private Image rewardsTabBg;
        [SerializeField] private GameObject upgradesView;
        [SerializeField] private GameObject rewardsView;
        [SerializeField] private GameObject rewardsTabNotificationPip;

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

        [Header("Milestone Mystery Rewards")]
        [SerializeField] private Text milestoneHintBannerText;
        [SerializeField] private GameObject[] milestoneCardRoots; // 6 elements
        [SerializeField] private Text[] milestoneIconTexts;       // 6 elements (shows ?, 🎁, ✓)
        [SerializeField] private Text[] milestoneTitleTexts;      // 6 elements (shows ??? or name)
        [SerializeField] private Text[] milestoneRewardTexts;     // 6 elements (+100 🪙)
        [SerializeField] private Button[] milestoneClaimBtns;     // 6 elements
        [SerializeField] private Text[] milestoneClaimBtnTexts;   // 6 elements

        private int activeTabIndex = 0; // 0 = Upgrades, 1 = Rewards
        private float timerUpdateInterval = 0.5f;
        private float nextTimerUpdate = 0f;

        private void Awake()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void Start()
        {
            SetupListeners();
            RefreshNotificationPips();
        }

        public void SetupListeners()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveAllListeners();
                toggleButton.onClick.AddListener(OpenModal);
            }
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseModal);
            }

            AutoFindTabsIfNull();

            if (upgradesTabBtn != null)
            {
                upgradesTabBtn.onClick.RemoveAllListeners();
                upgradesTabBtn.onClick.AddListener(() => SwitchTab(0));
            }
            if (rewardsTabBtn != null)
            {
                rewardsTabBtn.onClick.RemoveAllListeners();
                rewardsTabBtn.onClick.AddListener(() => SwitchTab(1));
            }

            // Also attach IPointerClickHandler directly to tab gameobjects so clicks never get dropped
            AttachTabClickTrigger(upgradesTabBtn != null ? upgradesTabBtn.gameObject : null, 0);
            AttachTabClickTrigger(rewardsTabBtn != null ? rewardsTabBtn.gameObject : null, 1);

            if (trolleyBuyBtn != null)
            {
                trolleyBuyBtn.onClick.RemoveAllListeners();
                trolleyBuyBtn.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.TrolleySpeed));
            }
            if (gripBuyBtn != null)
            {
                gripBuyBtn.onClick.RemoveAllListeners();
                gripBuyBtn.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.GripPower));
            }
            if (dropBuyBtn != null)
            {
                dropBuyBtn.onClick.RemoveAllListeners();
                dropBuyBtn.onClick.AddListener(() => OnBuyUpgrade(UpgradeType.DropPrecision));
            }

            if (claimDailyBtn != null)
            {
                claimDailyBtn.onClick.RemoveAllListeners();
                claimDailyBtn.onClick.AddListener(OnClaimDaily);
            }

            if (questClaimBtns != null)
            {
                for (int i = 0; i < questClaimBtns.Length; i++)
                {
                    int index = i;
                    if (questClaimBtns[i] != null)
                    {
                        questClaimBtns[i].onClick.RemoveAllListeners();
                        questClaimBtns[i].onClick.AddListener(() => OnClaimQuest(index));
                    }
                }
            }

            if (milestoneClaimBtns != null)
            {
                for (int i = 0; i < milestoneClaimBtns.Length; i++)
                {
                    int index = i;
                    if (milestoneClaimBtns[i] != null)
                    {
                        milestoneClaimBtns[i].onClick.RemoveAllListeners();
                        milestoneClaimBtns[i].onClick.AddListener(() => OnClaimMilestone(index));
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged += HandleCoinsChanged;
                econ.OnUpgradePurchased += HandleUpgradePurchased;
            }
            var ret = GetRetentionService();
            if (ret != null)
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
            var ret = GetRetentionService();
            if (ret != null)
            {
                ret.OnRetentionStateChanged -= HandleRetentionChanged;
            }
        }

        private IRetentionService GetRetentionService()
        {
            if (ServiceLocator.TryGet<IRetentionService>(out var ret)) return ret;
            return FindFirstObjectByType<RetentionService>();
        }

        public void OpenModal()
        {
            SetupListeners();
            if (modalPanel != null) modalPanel.SetActive(true);
            SwitchTab(activeTabIndex);
        }

        public void CloseModal()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void AutoFindTabsIfNull()
        {
            if (modalPanel == null)
            {
                var found = transform.Find("ProgressModal");
                if (found == null)
                {
                    var go = GameObject.Find("ProgressModal");
                    if (go != null) found = go.transform;
                }
                if (found != null) modalPanel = found.gameObject;
            }

            if (modalPanel != null)
            {
                if (upgradesView == null)
                {
                    var uv = modalPanel.transform.Find("UpgradesView");
                    if (uv != null) upgradesView = uv.gameObject;
                }
                if (rewardsView == null)
                {
                    var rv = modalPanel.transform.Find("RewardsView");
                    if (rv != null) rewardsView = rv.gameObject;
                }
                if (upgradesTabBtn == null)
                {
                    var tb = modalPanel.transform.Find("TabBar/Tab_Upgrades");
                    if (tb != null) upgradesTabBtn = tb.GetComponent<Button>();
                }
                if (rewardsTabBtn == null)
                {
                    var tb = modalPanel.transform.Find("TabBar/Tab_Rewards");
                    if (tb != null) rewardsTabBtn = tb.GetComponent<Button>();
                }
                if (upgradesTabBg == null)
                {
                    var tb = modalPanel.transform.Find("TabBar/Tab_Upgrades");
                    if (tb != null) upgradesTabBg = tb.GetComponent<Image>();
                }
                if (rewardsTabBg == null)
                {
                    var tb = modalPanel.transform.Find("TabBar/Tab_Rewards");
                    if (tb != null) rewardsTabBg = tb.GetComponent<Image>();
                }
            }
        }

        private void AttachTabClickTrigger(GameObject targetObj, int tabIndex)
        {
            if (targetObj == null) return;
            var trigger = targetObj.GetComponent<TabPointerClicker>();
            if (trigger == null) trigger = targetObj.AddComponent<TabPointerClicker>();
            trigger.targetModal = this;
            trigger.tabIndex = tabIndex;
        }

        public void SwitchTab(int tabIndex)
        {
            activeTabIndex = tabIndex;
            Debug.Log($"[ProgressModalUI] SwitchTab({tabIndex}) - Upgrades: {tabIndex == 0}, Rewards: {tabIndex == 1}");

            if (upgradesView != null) upgradesView.SetActive(tabIndex == 0);
            if (rewardsView != null) rewardsView.SetActive(tabIndex == 1);

            Color activeColor = new Color(0.24f, 0.42f, 0.70f);
            Color inactiveColor = new Color(0.14f, 0.16f, 0.22f);

            if (upgradesTabBg != null) upgradesTabBg.color = tabIndex == 0 ? activeColor : inactiveColor;
            if (rewardsTabBg != null) rewardsTabBg.color = tabIndex == 1 ? activeColor : inactiveColor;

            RefreshUI();
        }

        private void Update()
        {
            RefreshNotificationPips();

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

        private void RefreshNotificationPips()
        {
            var ret = GetRetentionService();
            bool hasClaimable = ret != null && ret.HasAnyClaimableReward;
            if (marqueeNotificationPip != null) marqueeNotificationPip.SetActive(hasClaimable);
            if (rewardsTabNotificationPip != null) rewardsTabNotificationPip.SetActive(hasClaimable);
        }

        private void RefreshUI()
        {
            RefreshCoins();
            RefreshNotificationPips();
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
            coinsText.text = $"🪙 {coins:N0}";
        }

        // ==================== UPGRADES TAB ====================
        private void RefreshUpgradesTab()
        {
            IEconomyService econ = null;
            ServiceLocator.TryGet<IEconomyService>(out econ);
            if (econ == null) return;

            ICollectionService coll = null;
            ServiceLocator.TryGet<ICollectionService>(out coll);
            var mach = coll != null ? coll.CurrentMachine : null;

            int currentCoins = econ.Coins;

            // Speed
            int spdLvl = econ.GetUpgradeLevel(UpgradeType.TrolleySpeed);
            int spdCost = econ.GetUpgradeCost(UpgradeType.TrolleySpeed);
            bool spdMax = spdCost < 0;
            if (trolleyLevelText != null) trolleyLevelText.text = $"LVL {spdLvl}/5";
            if (trolleyCostText != null) trolleyCostText.text = spdMax ? "MAX" : $"{spdCost} 🪙";
            if (trolleyBuyBtn != null) trolleyBuyBtn.interactable = !spdMax && currentCoins >= spdCost;

            // Magnet (was Grip Power)
            int grpLvl = econ.GetUpgradeLevel(UpgradeType.GripPower);
            int grpCost = econ.GetUpgradeCost(UpgradeType.GripPower);
            bool grpMax = grpCost < 0;
            if (gripLevelText != null) gripLevelText.text = $"LVL {grpLvl}/10 (+{grpLvl}% Magnet)";
            if (gripCostText != null) gripCostText.text = grpMax ? "MAX" : $"{grpCost} 🪙";
            if (gripBuyBtn != null) gripBuyBtn.interactable = !grpMax && currentCoins >= grpCost;

            // Aim Grab Range (was Precision)
            int prcLvl = econ.GetUpgradeLevel(UpgradeType.DropPrecision);
            int prcCost = econ.GetUpgradeCost(UpgradeType.DropPrecision);
            bool prcMax = prcCost < 0;
            if (dropLevelText != null) dropLevelText.text = $"LVL {prcLvl}/5 (Aim Area)";
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

            if (success)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayGrabSuccess();
                RefreshUI();
            }
        }

        // ==================== REWARDS TAB ====================
        private void RefreshRewardsTab()
        {
            try
            {
                var ret = GetRetentionService();
                if (ret == null)
                {
                    Debug.LogWarning("[ProgressModalUI] RetentionService not found.");
                    return;
                }

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
                RefreshMilestones();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ProgressModalUI] Exception in RefreshRewardsTab: {ex}");
            }
        }

        private void RefreshDailyClaimButton()
        {
            if (claimDailyBtn == null || claimDailyBtnText == null) return;
            var ret = GetRetentionService();
            if (ret == null) return;

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
            var ret = GetRetentionService();
            if (ret == null) return;
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

        private void RefreshMilestones()
        {
            var ret = GetRetentionService();
            if (ret == null) return;
            var milestones = ret.GetMilestones();

            if (milestoneCardRoots == null) return;

            for (int i = 0; i < milestoneCardRoots.Length; i++)
            {
                if (i >= milestones.Count) break;
                var m = milestones[i];
                var root = milestoneCardRoots[i];
                if (root == null) continue;

                // Tooltip trigger
                MilestoneCardTrigger trigger = root.GetComponent<MilestoneCardTrigger>();
                if (trigger == null) trigger = root.AddComponent<MilestoneCardTrigger>();
                trigger.hintMessage = $"★ {m.title.ToUpper()}: {m.hint} (+{m.rewardCoins} 🪙)";
                trigger.onHoverAction = (msg) =>
                {
                    if (milestoneHintBannerText != null) milestoneHintBannerText.text = msg;
                };
                trigger.onExitAction = () =>
                {
                    if (milestoneHintBannerText != null)
                        milestoneHintBannerText.text = "★ Hover or tap any [ ? ] to reveal unlock requirements";
                };

                // Icon text: "?" when locked, "🎁" when unlocked, "✓" when claimed
                if (milestoneIconTexts != null && i < milestoneIconTexts.Length && milestoneIconTexts[i] != null)
                {
                    if (m.isClaimed)
                    {
                        milestoneIconTexts[i].text = "✓";
                        milestoneIconTexts[i].color = new Color(0.4f, 0.9f, 0.4f);
                    }
                    else if (m.isUnlocked)
                    {
                        milestoneIconTexts[i].text = "🎁";
                        milestoneIconTexts[i].color = new Color(1f, 0.85f, 0.2f);
                    }
                    else
                    {
                        milestoneIconTexts[i].text = "?";
                        milestoneIconTexts[i].color = new Color(0.7f, 0.75f, 0.85f);
                    }
                }

                // Title: "???" when locked
                if (milestoneTitleTexts != null && i < milestoneTitleTexts.Length && milestoneTitleTexts[i] != null)
                {
                    milestoneTitleTexts[i].text = m.isClaimed ? m.title : (m.isUnlocked ? m.title : "???");
                    milestoneTitleTexts[i].color = m.isUnlocked || m.isClaimed ? Color.white : new Color(0.6f, 0.65f, 0.75f);
                }

                if (milestoneRewardTexts != null && i < milestoneRewardTexts.Length && milestoneRewardTexts[i] != null)
                {
                    milestoneRewardTexts[i].text = $"+{m.rewardCoins} 🪙";
                }

                if (milestoneClaimBtns != null && i < milestoneClaimBtns.Length && milestoneClaimBtns[i] != null)
                {
                    bool canClaim = m.isUnlocked && !m.isClaimed;
                    milestoneClaimBtns[i].interactable = canClaim;

                    if (milestoneClaimBtnTexts != null && i < milestoneClaimBtnTexts.Length && milestoneClaimBtnTexts[i] != null)
                    {
                        if (m.isClaimed) milestoneClaimBtnTexts[i].text = "CLAIMED ✓";
                        else if (canClaim) milestoneClaimBtnTexts[i].text = "CLAIM";
                        else milestoneClaimBtnTexts[i].text = "LOCKED";
                    }
                }
            }
        }

        private void OnClaimDaily()
        {
            var ret = GetRetentionService();
            if (ret != null)
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
            var ret = GetRetentionService();
            if (ret != null)
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

        private void OnClaimMilestone(int milestoneIndex)
        {
            var ret = GetRetentionService();
            if (ret != null)
            {
                var milestones = ret.GetMilestones();
                if (milestoneIndex >= 0 && milestoneIndex < milestones.Count)
                {
                    if (ret.ClaimMilestoneReward(milestones[milestoneIndex].id, out int coins))
                    {
                        if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.PlayWin();
                        ClawJuiceEffects.Instance?.PlayWinCelebration(transform.position);
                        RefreshUI();
                    }
                }
            }
        }

        private void HandleCoinsChanged(int newCoins) => RefreshUI();
        private void HandleUpgradePurchased(UpgradeType type, int level) => RefreshUI();
        private void HandleRetentionChanged() => RefreshUI();
    }

    public class TabPointerClicker : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public ProgressModalUI targetModal;
        public int tabIndex;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (targetModal == null) targetModal = UnityEngine.Object.FindFirstObjectByType<ProgressModalUI>();
            if (targetModal != null)
            {
                targetModal.SwitchTab(tabIndex);
            }
        }
    }
}
