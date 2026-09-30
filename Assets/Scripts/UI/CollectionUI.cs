using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.UI
{
    public class CollectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text ownershipText;
        [SerializeField] private Transform gridContainer;

        [Header("Celebration Modal")]
        [SerializeField] private GameObject completionModal;
        [SerializeField] private Text completionText;
        [SerializeField] private Button completionCloseButton;

        private readonly List<GameObject> cardObjects = new List<GameObject>();

        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleModal);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseModal);
            }
            if (completionCloseButton != null)
            {
                completionCloseButton.onClick.AddListener(() => {
                    if (completionModal != null) completionModal.SetActive(false);
                });
            }

            if (modalPanel != null) modalPanel.SetActive(false);
            if (completionModal != null) completionModal.SetActive(false);
        }

        private bool isSubscribed;

        private void OnEnable()
        {
            SubscribeToManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }

        private void SubscribeToManager()
        {
            if (isSubscribed || CollectionManager.Instance == null) return;
            CollectionManager.Instance.OnCoinsChanged += HandleCoinsChanged;
            CollectionManager.Instance.OnPrizeRegistered += HandlePrizeRegistered;
            CollectionManager.Instance.OnMachineCompleted += HandleMachineCompleted;
            CollectionManager.Instance.OnCurrentMachineChanged += HandleCurrentMachineChanged;
            isSubscribed = true;
        }

        private void UnsubscribeFromManager()
        {
            if (!isSubscribed || CollectionManager.Instance == null) return;
            CollectionManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
            CollectionManager.Instance.OnPrizeRegistered -= HandlePrizeRegistered;
            CollectionManager.Instance.OnMachineCompleted -= HandleMachineCompleted;
            CollectionManager.Instance.OnCurrentMachineChanged -= HandleCurrentMachineChanged;
            isSubscribed = false;
        }

        private void HandleCurrentMachineChanged(MachineDefinition def)
        {
            RefreshCollectionGrid();
        }

        private void Start()
        {
            SubscribeToManager();
            UpdateCoinsUI();
            RefreshCollectionGrid();
        }

        public void ToggleModal()
        {
            if (modalPanel == null) return;
            bool active = !modalPanel.activeSelf;
            modalPanel.SetActive(active);
            if (active)
            {
                RefreshCollectionGrid();
            }
        }

        public void CloseModal()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void HandleCoinsChanged(int newCoins)
        {
            UpdateCoinsUI();
        }

        private void HandlePrizeRegistered(PrizeDefinition prize, bool isNew)
        {
            UpdateCoinsUI();
            if (modalPanel != null && modalPanel.activeSelf)
            {
                RefreshCollectionGrid();
            }
        }

        private void HandleMachineCompleted(MachineDefinition machine)
        {
            RefreshCollectionGrid();
            if (completionModal != null)
            {
                if (completionText != null)
                {
                    completionText.text = $"★ CONGRATULATIONS! ★\n\nYOU COMPLETED {machine.displayName.ToUpper()}!\n\n• Machine is now permanently YOURS!\n• Free plays unlocked forever!\n• Passive income: +{machine.passiveIncomePerMinute} coins/min!";
                }
                completionModal.SetActive(true);
            }
        }

        private void UpdateCoinsUI()
        {
            if (coinsText != null && CollectionManager.Instance != null)
            {
                coinsText.text = $"{CollectionManager.Instance.Coins} 🪙";
            }
        }

        public void RefreshCollectionGrid()
        {
            if (CollectionManager.Instance == null) return;

            MachineDefinition machine = CollectionManager.Instance.CurrentMachine;
            if (machine == null) return;

            int discovered = CollectionManager.Instance.GetDiscoveredCount(machine);
            bool isOwned = CollectionManager.Instance.IsMachineOwned(machine.machineId);

            if (titleText != null)
            {
                titleText.text = $"{machine.displayName.ToUpper()}";
            }
            if (progressText != null)
            {
                int pct = (int)((discovered / 9f) * 100f);
                progressText.text = $"COLLECTION: {discovered} / 9 ({pct}%)";
            }
            if (ownershipText != null)
            {
                ownershipText.text = isOwned ? "★ OWNED (FREE PLAYS + PASSIVE INCOME)" : $"UNOWNED (ENTRY: {machine.entryCost} COINS)";
                ownershipText.color = isOwned ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.85f, 0.85f, 0.9f);
            }

            // Populate cards
            if (gridContainer == null || machine.prizes == null) return;

            for (int i = 0; i < machine.prizes.Length; i++)
            {
                PrizeDefinition def = machine.prizes[i];
                if (def == null) continue;

                Transform cardTr = gridContainer.childCount > i ? gridContainer.GetChild(i) : null;
                if (cardTr == null) continue;

                bool hasFound = CollectionManager.Instance.IsPrizeDiscovered(def.id);
                UpdateCard(cardTr.gameObject, def, hasFound);
            }
        }

        private void UpdateCard(GameObject card, PrizeDefinition def, bool discovered)
        {
            Image bg = card.GetComponent<Image>();
            Text[] texts = card.GetComponentsInChildren<Text>();

            Color rarityBorderColor = def.rarity switch
            {
                PrizeRarity.Normal => new Color(0.4f, 0.45f, 0.55f),
                PrizeRarity.Rare => new Color(1.0f, 0.82f, 0.15f),
                PrizeRarity.Secret => new Color(0.95f, 0.25f, 0.90f),
                _ => Color.gray
            };

            if (bg != null)
            {
                bg.color = discovered ? new Color(0.18f, 0.20f, 0.28f) : new Color(0.12f, 0.13f, 0.16f);
            }

            Transform nameChild = card.transform.Find("Name");
            Transform rarityChild = card.transform.Find("Rarity");
            Transform statusChild = card.transform.Find("Status");

            Text nameText = nameChild != null ? nameChild.GetComponent<Text>() : (texts.Length > 0 ? texts[0] : null);
            Text rarityText = rarityChild != null ? rarityChild.GetComponent<Text>() : (texts.Length > 1 ? texts[1] : null);
            Text statusText = statusChild != null ? statusChild.GetComponent<Text>() : (texts.Length > 2 ? texts[2] : null);

            if (nameText != null)
            {
                nameText.text = discovered ? def.displayName : "???";
                nameText.color = discovered ? Color.white : new Color(0.55f, 0.55f, 0.60f);
            }
            if (rarityText != null)
            {
                rarityText.text = def.rarity.ToString().ToUpper();
                rarityText.color = rarityBorderColor;
            }
            if (statusText != null)
            {
                statusText.text = discovered ? "✓ COLLECTED" : "LOCKED";
                statusText.color = discovered ? new Color(0.35f, 0.90f, 0.45f) : new Color(0.5f, 0.5f, 0.5f);
            }
        }
    }
}
