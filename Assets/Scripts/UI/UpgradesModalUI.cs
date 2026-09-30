using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;

namespace ClawMachine.UI
{
    public class UpgradesModalUI : MonoBehaviour
    {
        [Header("Modal & Toggle")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;

        [Header("Header Info")]
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
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCoinsChanged += HandleCoinsChanged;
                CollectionManager.Instance.OnUpgradePurchased += HandleUpgradePurchased;
                CollectionManager.Instance.OnMachineCompleted += HandleMachineCompleted;
            }
        }

        private void OnDisable()
        {
            if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
                CollectionManager.Instance.OnUpgradePurchased -= HandleUpgradePurchased;
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
            if (CollectionManager.Instance == null) return;
            bool success = CollectionManager.Instance.TryPurchaseUpgrade(type);
            if (success)
            {
                ClawAudio.Instance?.PlayGrab();
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
            if (CollectionManager.Instance == null) return;

            int coins = CollectionManager.Instance.Coins;
            if (coinsText != null) coinsText.text = $"{coins} 🪙";

            // Set Bonus status
            if (setBonusText != null)
            {
                if (CollectionManager.Instance.HasGoldenClawUnlocked)
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
            UpdateRow(UpgradeType.TrolleySpeed, trolleyLevelText, trolleyCostText, trolleyBuyBtn, coins);

            // Grip Row
            UpdateRow(UpgradeType.GripPower, gripLevelText, gripCostText, gripBuyBtn, coins);

            // Drop Row
            UpdateRow(UpgradeType.DropPrecision, dropLevelText, dropCostText, dropBuyBtn, coins);
        }

        private void UpdateRow(UpgradeType type, Text lvlText, Text costText, Button buyBtn, int currentCoins)
        {
            int lvl = CollectionManager.Instance.GetUpgradeLevel(type);
            int cost = CollectionManager.Instance.GetUpgradeCost(type);

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
