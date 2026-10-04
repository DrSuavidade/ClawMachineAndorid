using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Data;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

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
            ICollectionService collService = ServiceLocator.Get<ICollectionService>();

            if (catalog == null && collService is CollectionManager cm && cm.Catalog != null)
            {
                catalog = cm.Catalog;
            }

            if (catalog != null && catalog.Machines != null && catalog.Machines.Length > 0)
            {
                availableMachines = catalog.Machines;
            }
            else if (availableMachines == null || availableMachines.Length == 0)
            {
                Debug.LogWarning("[ArcadeConsoleUI] No machines available. Please assign MachineCatalog in inspector.");
            }

            // Sync index with current machine
            if (collService != null && collService.CurrentMachine != null)
            {
                for (int i = 0; i < availableMachines.Length; i++)
                {
                    if (availableMachines[i] != null && availableMachines[i].machineId == collService.CurrentMachine.machineId)
                    {
                        currentMachineIndex = i;
                        break;
                    }
                }
            }

            UpdateCarouselDisplay();
        }

        private Vector2 swipeTouchStart;
        private bool isTouchSwiping;

        private void Update()
        {
            bool canSwitch = machineController == null || machineController.CanSwitchMachine;
            if (prevMachineButton != null && prevMachineButton.interactable != canSwitch)
            {
                prevMachineButton.interactable = canSwitch;
            }
            if (nextMachineButton != null && nextMachineButton.interactable != canSwitch)
            {
                nextMachineButton.interactable = canSwitch;
            }

            if (!canSwitch) return;

            // Keyboard shortcuts to switch machine (Q / E keys)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnPrevClicked();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                OnNextClicked();
            }

            // Touch / mouse horizontal swipe detection across playfield
            if (Input.GetMouseButtonDown(0))
            {
                swipeTouchStart = Input.mousePosition;
                isTouchSwiping = true;
            }
            else if (Input.GetMouseButtonUp(0) && isTouchSwiping)
            {
                isTouchSwiping = false;
                Vector2 delta = (Vector2)Input.mousePosition - swipeTouchStart;
                if (Mathf.Abs(delta.x) > 85f && Mathf.Abs(delta.y) < Mathf.Abs(delta.x) * 1.2f && Input.mousePosition.y > Screen.height * 0.32f)
                {
                    if (delta.x < 0) OnNextClicked();
                    else OnPrevClicked();
                }
            }
        }

        public void SetCatalog(MachineDefinition[] machines)
        {
            availableMachines = machines;
            UpdateCarouselDisplay();
        }

        public void OnPrevClicked()
        {
            if (machineController != null && !machineController.CanSwitchMachine) return;
            if (availableMachines == null || availableMachines.Length <= 1) return;
            int target = currentMachineIndex - 1;
            if (target < 0) target = availableMachines.Length - 1;
            TryNavigateToMachine(target);
        }

        public void OnNextClicked()
        {
            if (machineController != null && !machineController.CanSwitchMachine) return;
            if (availableMachines == null || availableMachines.Length <= 1) return;
            int target = (currentMachineIndex + 1) % availableMachines.Length;
            TryNavigateToMachine(target);
        }

        private void TryNavigateToMachine(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= availableMachines.Length) return;

            // Block switching while claw is active or carrying a prize
            if (machineController != null && !machineController.CanSwitchMachine)
            {
                return;
            }

            MachineDefinition target = availableMachines[targetIndex];
            if (target == null) return;

            ICollectionService collService = ServiceLocator.Get<ICollectionService>();

            bool isUnlocked = collService == null || collService.IsMachineUnlocked(target.machineId);

            if (isUnlocked)
            {
                SwitchToMachine(targetIndex);
            }
            else
            {
                OpenUnlockModal(target, targetIndex);
            }
        }

        private Coroutine titleBounceRoutine;

        private void BounceTitle()
        {
            if (machineNameText == null) return;
            if (titleBounceRoutine != null) StopCoroutine(titleBounceRoutine);
            titleBounceRoutine = StartCoroutine(TitleBounceRoutine());
        }

        private System.Collections.IEnumerator TitleBounceRoutine()
        {
            float elapsed = 0f;
            float duration = 0.25f;
            Vector3 punch = Vector3.one * 1.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                machineNameText.transform.localScale = Vector3.Lerp(punch, Vector3.one, t);
                yield return null;
            }
            machineNameText.transform.localScale = Vector3.one;
            titleBounceRoutine = null;
        }

        private void SwitchToMachine(int index)
        {
            currentMachineIndex = index;
            MachineDefinition target = availableMachines[currentMachineIndex];

            if (machineController != null)
            {
                machineController.SwipeToMachine(index, target);
            }
            else
            {
                ServiceLocator.Get<ICollectionService>()?.SetCurrentMachine(target);
            }

            BounceTitle();
            var audioSvc = ServiceLocator.Get<IAudioService>();
            if (audioSvc != null)
            {
                audioSvc.PlayClamp();
                audioSvc.TriggerHapticLight();
            }

            UpdateCarouselDisplay();
            Debug.Log($"[ArcadeConsoleUI] Switched to machine: {target.displayName}");
        }

        private void UpdateCarouselDisplay()
        {
            if (availableMachines == null || availableMachines.Length == 0 || currentMachineIndex >= availableMachines.Length) return;
            MachineDefinition cur = availableMachines[currentMachineIndex];
            if (cur == null) return;

            ICollectionService collService = ServiceLocator.Get<ICollectionService>();

            if (machineNameText != null)
            {
                machineNameText.text = cur.displayName.ToUpper();
            }

            if (machineStatusText != null)
            {
                if (collService != null && collService.IsMachineOwned(cur.machineId))
                {
                    machineStatusText.text = "★ OWNED (FREE PLAYS)";
                    machineStatusText.color = new Color(0.3f, 0.95f, 0.45f);
                }
                else if (collService != null && !collService.IsMachineUnlocked(cur.machineId))
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

            var btn = GetComponentInChildren<ArcadePushButtonUI>() ?? FindFirstObjectByType<ArcadePushButtonUI>();
            if (btn != null) btn.SetThemeColor(cur.cabinetFrameColor);

            var joy = GetComponentInChildren<ArcadeJoystickUI>() ?? FindFirstObjectByType<ArcadeJoystickUI>();
            if (joy != null) joy.SetThemeColor(cur.cabinetFrameColor);
        }

        private void OpenUnlockModal(MachineDefinition machine, int targetIndex)
        {
            pendingUnlockMachine = machine;
            if (unlockModal == null) return;

            int currentCoins = 0;
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                currentCoins = econ.Coins;
            }

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
            if (pendingUnlockMachine == null) return;

            bool success = false;
            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                success = coll.TryUnlockMachine(pendingUnlockMachine);
            }

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
