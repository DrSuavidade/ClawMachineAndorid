using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class UpgradesModalUI : MonoBehaviour
    {
        [Header("Modal & Toggle")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text setBonusText;

        [Header("Trolley Speed Row")]
        [SerializeField] private Text trolleyLevelText;
        [SerializeField] private Text trolleyCostText;
        [SerializeField] private Button trolleyBuyBtn;

        [Header("Grip Power Row")]
        [SerializeField] private Text gripLevelText;
        [SerializeField] private Text gripCostText;
        [SerializeField] private Button gripBuyBtn;

        [Header("Drop Precision Row")]
        [SerializeField] private Text dropLevelText;
        [SerializeField] private Text dropCostText;
        [SerializeField] private Button dropBuyBtn;

        private void Awake()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(OpenModal);
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);

            if (trolleyBuyBtn != null) trolleyBuyBtn.onClick.AddListener(() => OnBuyClicked(UpgradeType.TrolleySpeed));
            if (gripBuyBtn != null) gripBuyBtn.onClick.AddListener(() => OnBuyClicked(UpgradeType.GripPower));
            if (dropBuyBtn != null) dropBuyBtn.onClick.AddListener(() => OnBuyClicked(UpgradeType.DropPrecision));

            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged += HandleCoinsChanged;
                econ.OnUpgradePurchased += HandleUpgradePurchased;
            }
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCoinsChanged += HandleCoinsChanged;
                CollectionManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
            }

            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.OnMachineCompleted += HandleMachineCompleted;
            }
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnMachineCompleted += HandleMachineCompleted;
            }
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged -= HandleCoinsChanged;
                econ.OnUpgradePurchased -= HandleUpgradePurchased;
            }
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
                CollectionManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
            }

            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.OnMachineCompleted -= HandleMachineCompleted;
            }
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnMachineCompleted -= HandleMachineCompleted;
            }
        }

        public void OpenModal()
        {
            if (modalPanel != null) modalPanel.SetActive(true);
            RefreshUI();
        }

        public void CloseModal()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnBuyClicked(UpgradeType type)
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
                if (ServiceLocator.TryGet<IAudioService>(out var audio))
                {
                    audio.PlayGrabSuccess();
                }
                else
                {
                    ClawAudio.Instance?.PlayGrab();
                }
                RefreshUI();
            }
        }

        private void HandleCoinsChanged(int newCoins)
        {
            if (modalPanel != null && modalPanel.activeSelf)
            {
                RefreshUI();
            }
        }

        private void HandleUpgradePurchased(UpgradeType type, int level)
        {
            RefreshUI();
        }

        private void HandleMachineCompleted(Data.MachineDefinition def)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            IEconomyService econ = null;
            if (!ServiceLocator.TryGet(out econ))
            {
                econ = CollectionManager.Instance;
            }
            if (econ == null) return;

            ICollectionService coll = null;
            if (!ServiceLocator.TryGet(out coll))
            {
                coll = CollectionManager.Instance;
            }

            int coins = econ.Coins;
            if (coinsText != null) coinsText.text = $"{coins} 🪙";

            // Set Bonus status
            if (setBonusText != null)
            {
                bool hasGoldenClaw = coll != null ? coll.HasGoldenClawUnlocked : (CollectionManager.Instance != null && CollectionManager.Instance.HasGoldenClawUnlocked);
                if (hasGoldenClaw)
                {
                    setBonusText.text = "★ SET BONUS ACTIVE: +50% PASSIVE COINS & GOLD CLAW ★";
                    setBonusText.color = new Color(1f, 0.88f, 0.25f);
                }
                else
                {
                    setBonusText.text = "COLLECT ALL 9 PRIZES IN ANY CABINET FOR SET BONUS & GOLD CLAW";
                    setBonusText.color = new Color(0.65f, 0.70f, 0.80f);
                }
            }

            // Trolley Row
            UpdateRow(UpgradeType.TrolleySpeed, trolleyLevelText, trolleyCostText, trolleyBuyBtn, coins, econ);

            // Grip Row
            UpdateRow(UpgradeType.GripPower, gripLevelText, gripCostText, gripBuyBtn, coins, econ);

            // Drop Row
            UpdateRow(UpgradeType.DropPrecision, dropLevelText, dropCostText, dropBuyBtn, coins, econ);
        }

        private void UpdateRow(UpgradeType type, Text lvlText, Text costText, Button buyBtn, int currentCoins, IEconomyService econ)
        {
            int lvl = econ.GetUpgradeLevel(type);
            int cost = econ.GetUpgradeCost(type);

            if (lvlText != null)
            {
                string pips = "";
                for (int i = 1; i <= 5; i++)
                {
                    pips += (i <= lvl) ? "■ " : "□ ";
                }
                lvlText.text = $"LVL {lvl}/5  {pips.TrimEnd()}";
            }

            if (cost < 0)
            {
                if (costText != null) costText.text = "MAX LEVEL";
                if (buyBtn != null)
                {
                    buyBtn.interactable = false;
                    var img = buyBtn.GetComponent<Image>();
                    if (img != null) img.color = new Color(0.3f, 0.35f, 0.40f);
                }
            }
            else
            {
                if (costText != null) costText.text = $"{cost} 🪙";
                if (buyBtn != null)
                {
                    bool canAfford = currentCoins >= cost;
                    buyBtn.interactable = canAfford;
                    var img = buyBtn.GetComponent<Image>();
                    if (img != null) img.color = canAfford ? new Color(0.22f, 0.75f, 0.35f) : new Color(0.40f, 0.42f, 0.48f);
                }
            }
        }
    }
}
