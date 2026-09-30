using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.UI
{
    public class ArcadeConsoleUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Controllers")]
        [SerializeField] private MachineController machineController;

        [Header("Machine Catalog")]
        [SerializeField] private MachineCatalog catalog;
        [SerializeField] private MachineDefinition[] availableMachines;
        [SerializeField] private int currentMachineIndex = 0;

        [Header("Carousel UI Elements")]
        [SerializeField] private Button prevMachineButton;
        [SerializeField] private Button nextMachineButton;
        [SerializeField] private Text machineNameText;
        [SerializeField] private Text machineStatusText;

        [Header("Unlock Modal")]
        [SerializeField] private GameObject unlockModal;
        [SerializeField] private Text unlockTitleText;
        [SerializeField] private Text unlockDescriptionText;
        [SerializeField] private Text unlockCoinsBalanceText;
        [SerializeField] private Button unlockConfirmButton;
        [SerializeField] private Text unlockConfirmButtonText;
        [SerializeField] private Button unlockCancelButton;

        private MachineDefinition pendingUnlockMachine;
        private Vector2 dragStartPos;
        private const float SWIPE_THRESHOLD = 70f;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindFirstObjectByType<MachineController>();
            }

            if (prevMachineButton != null)
            {
                prevMachineButton.onClick.AddListener(OnPrevClicked);
            }
            if (nextMachineButton != null)
            {
                nextMachineButton.onClick.AddListener(OnNextClicked);
            }

            if (unlockConfirmButton != null)
            {
                unlockConfirmButton.onClick.AddListener(OnConfirmUnlockClicked);
            }
            if (unlockCancelButton != null)
            {
                unlockCancelButton.onClick.AddListener(CloseUnlockModal);
            }

            if (unlockModal != null)
            {
                unlockModal.SetActive(false);
            }
        }

        private void Start()
        {
            if (catalog == null && CollectionManager.Instance != null && CollectionManager.Instance.Catalog != null)
            {
                catalog = CollectionManager.Instance.Catalog;
            }

            if (catalog != null && catalog.Machines != null && catalog.Machines.Length > 0)
            {
                availableMachines = catalog.Machines;
            }
            else if (availableMachines == null || availableMachines.Length == 0)
            {
                var list = new List<MachineDefinition>();
#if UNITY_EDITOR
                var loadedCatalog = UnityEditor.AssetDatabase.LoadAssetAtPath<MachineCatalog>("Assets/Data/MachineCatalog.asset");
                if (loadedCatalog != null && loadedCatalog.Machines != null)
                {
                    catalog = loadedCatalog;
                    availableMachines = loadedCatalog.Machines;
                }
                else
                {
                    var toyBox = UnityEditor.AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_ToyBox.asset");
                    if (toyBox != null) list.Add(toyBox);
                    var retro = UnityEditor.AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_RetroArcade.asset");
                    if (retro != null) list.Add(retro);
                    availableMachines = list.ToArray();
                }
#endif
            }

            // Sync index with current machine
            if (CollectionManager.Instance != null && CollectionManager.Instance.CurrentMachine != null)
            {
                for (int i = 0; i < availableMachines.Length; i++)
                {
                    if (availableMachines[i] != null && availableMachines[i].machineId == CollectionManager.Instance.CurrentMachine.machineId)
                    {
                        currentMachineIndex = i;
                        break;
                    }
                }
            }

            UpdateCarouselDisplay();
        }

        private void Update()
        {
            // Keyboard shortcuts to switch machine (Q / E keys)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnPrevClicked();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                OnNextClicked();
            }
        }

        public void SetCatalog(MachineDefinition[] machines)
        {
            availableMachines = machines;
            UpdateCarouselDisplay();
        }

        public void OnPrevClicked()
        {
            if (availableMachines == null || availableMachines.Length <= 1) return;
            int target = currentMachineIndex - 1;
            if (target < 0) target = availableMachines.Length - 1;
            TryNavigateToMachine(target);
        }

        public void OnNextClicked()
        {
            if (availableMachines == null || availableMachines.Length <= 1) return;
            int target = (currentMachineIndex + 1) % availableMachines.Length;
            TryNavigateToMachine(target);
        }

        private void TryNavigateToMachine(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= availableMachines.Length) return;

            // Block switching while claw is active
            if (machineController != null)
            {
                var claw = machineController.GetComponentInChildren<ClawController>();
                if (claw != null && !claw.IsIdle) return;
            }

            MachineDefinition target = availableMachines[targetIndex];
            if (target == null) return;

            bool isUnlocked = CollectionManager.Instance == null || CollectionManager.Instance.IsMachineUnlocked(target.machineId);

            if (isUnlocked)
            {
                SwitchToMachine(targetIndex);
            }
            else
            {
                OpenUnlockModal(target, targetIndex);
            }
        }

        private void SwitchToMachine(int index)
        {
            currentMachineIndex = index;
            MachineDefinition target = availableMachines[currentMachineIndex];

            if (machineController != null)
            {
                machineController.ApplyMachine(target);
            }
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.SetCurrentMachine(target);
            }

            UpdateCarouselDisplay();
            Debug.Log($"[ArcadeConsoleUI] Switched to machine: {target.displayName}");
        }

        private void UpdateCarouselDisplay()
        {
            if (availableMachines == null || availableMachines.Length == 0 || currentMachineIndex >= availableMachines.Length) return;
            MachineDefinition cur = availableMachines[currentMachineIndex];
            if (cur == null) return;

            if (machineNameText != null)
            {
                machineNameText.text = cur.displayName.ToUpper();
            }

            if (machineStatusText != null)
            {
                if (CollectionManager.Instance != null && CollectionManager.Instance.IsMachineOwned(cur.machineId))
                {
                    machineStatusText.text = "★ OWNED (FREE PLAYS)";
                    machineStatusText.color = new Color(0.3f, 0.95f, 0.45f);
                }
                else if (CollectionManager.Instance != null && !CollectionManager.Instance.IsMachineUnlocked(cur.machineId))
                {
                    machineStatusText.text = $"🔒 LOCKED (UNLOCK: {cur.unlockCost} 🪙)";
                    machineStatusText.color = new Color(1f, 0.45f, 0.45f);
                }
                else
                {
                    machineStatusText.text = cur.entryCost > 0 ? $"ENTRY: {cur.entryCost} 🪙" : "FREE ENTRY";
                    machineStatusText.color = new Color(0.85f, 0.88f, 0.95f);
                }
            }
        }

        private void OpenUnlockModal(MachineDefinition machine, int targetIndex)
        {
            pendingUnlockMachine = machine;
            if (unlockModal == null) return;

            int currentCoins = CollectionManager.Instance != null ? CollectionManager.Instance.Coins : 0;
            bool canAfford = currentCoins >= machine.unlockCost;

            if (unlockTitleText != null)
            {
                unlockTitleText.text = $"UNLOCK {machine.displayName.ToUpper()}";
            }
            if (unlockDescriptionText != null)
            {
                unlockDescriptionText.text = $"Unlock access to the {machine.displayName} arcade cabinet with 9 exclusive retro collectibles!";
            }
            if (unlockCoinsBalanceText != null)
            {
                unlockCoinsBalanceText.text = $"Cost: {machine.unlockCost} 🪙  |  Your Balance: {currentCoins} 🪙";
                unlockCoinsBalanceText.color = canAfford ? new Color(1f, 0.85f, 0.2f) : new Color(0.95f, 0.35f, 0.35f);
            }
            if (unlockConfirmButton != null)
            {
                unlockConfirmButton.interactable = canAfford;
            }
            if (unlockConfirmButtonText != null)
            {
                unlockConfirmButtonText.text = canAfford ? $"UNLOCK ({machine.unlockCost} 🪙)" : "NOT ENOUGH COINS";
            }

            unlockModal.SetActive(true);
        }

        private void OnConfirmUnlockClicked()
        {
            if (pendingUnlockMachine == null || CollectionManager.Instance == null) return;

            bool success = CollectionManager.Instance.TryUnlockMachine(pendingUnlockMachine);
            if (success)
            {
                CloseUnlockModal();
                // Find index and switch
                for (int i = 0; i < availableMachines.Length; i++)
                {
                    if (availableMachines[i] == pendingUnlockMachine)
                    {
                        SwitchToMachine(i);
                        break;
                    }
                }
            }
            else
            {
                if (unlockConfirmButtonText != null)
                {
                    unlockConfirmButtonText.text = "NOT ENOUGH COINS!";
                }
            }
        }

        public void CloseUnlockModal()
        {
            pendingUnlockMachine = null;
            if (unlockModal != null)
            {
                unlockModal.SetActive(false);
            }
        }

        // Swipe handling
        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStartPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Pass-through
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - dragStartPos;
            if (Mathf.Abs(delta.x) > SWIPE_THRESHOLD && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x < 0)
                {
                    // Swipe left -> Next machine
                    OnNextClicked();
                }
                else
                {
                    // Swipe right -> Previous machine
                    OnPrevClicked();
                }
            }
        }
    }
}
