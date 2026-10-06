using System;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    public class MachineController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private ClawController claw;
        [SerializeField] private ChuteDetector chuteDetector;
        [SerializeField] private PrizeSpawner prizeSpawner;

        [Header("Cabinet Visuals")]
        [SerializeField] private MeshRenderer[] cabinetFrameRenderers;
        [SerializeField] private MeshRenderer cabinetBackdropRenderer;

        [Header("Carousel (3-Slot Virtual Sliding Window)")]
        [SerializeField] private Transform carouselContainer;
        [SerializeField] private float machineSpacing = 14.0f;
        [SerializeField] private MachineCatalog catalog;
        [SerializeField] private MachineSlot slotPrev;
        [SerializeField] private MachineSlot slotCurr;
        [SerializeField] private MachineSlot slotNext;
        private int currentSlotIndex = 0;
        private Coroutine swipeRoutine;

        [Header("Atmosphere Transitions")]
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private ClawCameraController cameraController;

        public bool IsSwiping => swipeRoutine != null;
        public ClawController Claw => claw;
        public bool CanSwitchMachine => !IsSwiping && (claw == null || claw.CanSwitchMachine);

        [Header("Stats for Prototype")]
        [SerializeField] private int totalAttempts;
        [SerializeField] private int successfulGrabs;

        public event Action<Prize> OnPrizeWon;
        public event Action<ClawState> OnStateChanged;

        public void SwipeToMachine(int targetIndex, MachineDefinition machine, float duration = 0.40f)
        {
            if (machine == null || !CanSwitchMachine) return;
            if (carouselContainer == null || slotCurr == null)
            {
                ApplyMachine(machine);
                return;
            }

            if (swipeRoutine != null) StopCoroutine(swipeRoutine);
            swipeRoutine = StartCoroutine(SwipeRoutine(targetIndex, machine, duration));
        }

        private System.Collections.IEnumerator SwipeRoutine(int targetIndex, MachineDefinition machine, float duration)
        {
            if (chuteDetector != null)
            {
                chuteDetector.OnPrizeCollected -= HandlePrizeCollected;
                chuteDetector.Suppress(2.0f);
            }

            if (claw != null)
            {
                claw.SetInputDelta(Vector2.zero);
            }

            int step = targetIndex - currentSlotIndex;

            // Direct jump if non-adjacent
            if (Mathf.Abs(step) > 1 || (step > 0 && slotNext == null) || (step < 0 && slotPrev == null))
            {
                carouselContainer.localPosition = Vector3.zero;
                currentSlotIndex = targetIndex;
                slotCurr.transform.localPosition = Vector3.zero;
                slotCurr.Bind(machine, currentSlotIndex, spawnPrizes: true);

                int prevIdx = currentSlotIndex - 1;
                if (slotPrev != null)
                {
                    slotPrev.transform.localPosition = new Vector3(-machineSpacing, 0f, 0f);
                    slotPrev.Bind(catalog != null && prevIdx >= 0 ? catalog.GetMachine(prevIdx) : null, prevIdx, spawnPrizes: false);
                }

                int nextIdx = currentSlotIndex + 1;
                if (slotNext != null)
                {
                    slotNext.transform.localPosition = new Vector3(machineSpacing, 0f, 0f);
                    slotNext.Bind(catalog != null && nextIdx < catalog.Count ? catalog.GetMachine(nextIdx) : null, nextIdx, spawnPrizes: false);
                }

                ApplyAtmosphereImmediate(machine);
                ReconnectActiveSlot();
                swipeRoutine = null;
                yield break;
            }

            // Adjacent slide
            Vector3 startPos = Vector3.zero;
            Vector3 targetPos = (step > 0) ? new Vector3(-machineSpacing, 0f, 0f) : new Vector3(machineSpacing, 0f, 0f);

            // Preload incoming slot with prizes before animating
            if (step > 0 && slotNext != null)
            {
                slotNext.Bind(machine, targetIndex, spawnPrizes: true);
            }
            else if (step < 0 && slotPrev != null)
            {
                slotPrev.Bind(machine, targetIndex, spawnPrizes: true);
            }

            Color startAmbient = RenderSettings.ambientLight;
            Color targetAmbient = (machine != null) ? machine.ambientLightColor : startAmbient;

            Color startDirCol = (mainDirectionalLight != null) ? mainDirectionalLight.color : Color.white;
            Color targetDirCol = (machine != null) ? machine.directionalLightColor : startDirCol;

            float startFov = (cameraController != null && cameraController.TargetCamera != null) ? cameraController.TargetCamera.fieldOfView : 45f;
            float targetFov = (machine != null && machine.cameraFieldOfView > 0f) ? machine.cameraFieldOfView : 45f;
            float targetPitch = (machine != null) ? machine.cameraPitch - 28f : 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                carouselContainer.localPosition = Vector3.Lerp(startPos, targetPos, smoothT);

                RenderSettings.ambientLight = Color.Lerp(startAmbient, targetAmbient, smoothT);
                if (mainDirectionalLight != null) mainDirectionalLight.color = Color.Lerp(startDirCol, targetDirCol, smoothT);
                if (cameraController != null && cameraController.TargetCamera != null)
                {
                    cameraController.TargetCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, smoothT);
                }

                yield return null;
            }

            // Snap carousel back to 0 and shift slot transforms
            carouselContainer.localPosition = Vector3.zero;

            if (step > 0) // Swiped right: Next becomes Curr, Curr becomes Prev, Prev recycled to Next
            {
                MachineSlot temp = slotPrev;
                slotPrev = slotCurr;
                slotCurr = slotNext;
                slotNext = temp;

                slotPrev.transform.localPosition = new Vector3(-machineSpacing, 0f, 0f);
                slotCurr.transform.localPosition = Vector3.zero;
                slotNext.transform.localPosition = new Vector3(machineSpacing, 0f, 0f);

                currentSlotIndex = targetIndex;

                int newNextIdx = currentSlotIndex + 1;
                MachineDefinition futureDef = (catalog != null && newNextIdx < catalog.Count) ? catalog.GetMachine(newNextIdx) : null;
                slotNext.Bind(futureDef, newNextIdx, spawnPrizes: false);
            }
            else // Swiped left: Prev becomes Curr, Curr becomes Next, Next recycled to Prev
            {
                MachineSlot temp = slotNext;
                slotNext = slotCurr;
                slotCurr = slotPrev;
                slotPrev = temp;

                slotPrev.transform.localPosition = new Vector3(-machineSpacing, 0f, 0f);
                slotCurr.transform.localPosition = Vector3.zero;
                slotNext.transform.localPosition = new Vector3(machineSpacing, 0f, 0f);

                currentSlotIndex = targetIndex;

                int newPrevIdx = currentSlotIndex - 1;
                MachineDefinition pastDef = (catalog != null && newPrevIdx >= 0) ? catalog.GetMachine(newPrevIdx) : null;
                slotPrev.Bind(pastDef, newPrevIdx, spawnPrizes: false);
            }

            ApplyAtmosphereImmediate(machine);
            ReconnectActiveSlot();

            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.SetCurrentMachine(machine);
            }

            swipeRoutine = null;
        }

        public void ApplyAtmosphereImmediate(MachineDefinition machine)
        {
            if (machine == null) return;
            RenderSettings.ambientLight = machine.ambientLightColor;
            if (mainDirectionalLight != null) mainDirectionalLight.color = machine.directionalLightColor;
            if (cameraController != null)
            {
                float targetFov = machine.cameraFieldOfView > 0f ? machine.cameraFieldOfView : 45f;
                float targetPitch = machine.cameraPitch - 28f;
                cameraController.SetThemePreset(targetFov, targetPitch);
            }
        }

        public void RefreshAtmosphere()
        {
            var def = (slotCurr != null && slotCurr.BoundDefinition != null) 
                ? slotCurr.BoundDefinition 
                : (catalog != null && catalog.Count > 0 ? catalog.GetMachine(0) : null);
            ApplyAtmosphereImmediate(def);
        }

        private void ReconnectActiveSlot()
        {
            if (slotCurr != null)
            {
                chuteDetector = slotCurr.ChuteDetector;
                prizeSpawner = slotCurr.PrizeSpawner;
                if (chuteDetector != null)
                {
                    chuteDetector.OnPrizeCollected += HandlePrizeCollected;
                }
            }
        }

        public void TransitionToMachine(MachineDefinition machine, float duration = 0.35f)
        {
            if (machine == null || !CanSwitchMachine) return;
            if (carouselContainer != null)
            {
                int idx = 0;
                if (catalog != null) idx = catalog.IndexOf(machine);
                SwipeToMachine(Mathf.Max(0, idx), machine, duration);
                return;
            }

            ApplyMachine(machine);
        }

        public void ApplyMachine(MachineDefinition machine)
        {
            if (machine == null) return;

            // Re-skin cabinet materials
            if (cabinetFrameRenderers != null)
            {
                for (int i = 0; i < cabinetFrameRenderers.Length; i++)
                {
                    if (cabinetFrameRenderers[i] != null)
                    {
                        cabinetFrameRenderers[i].material.color = machine.cabinetFrameColor;
                    }
                }
            }
            if (cabinetBackdropRenderer != null)
            {
                cabinetBackdropRenderer.material.color = machine.cabinetBackdropColor;
            }

            // Suppress chute detection during switch/settling
            if (chuteDetector != null)
            {
                chuteDetector.Suppress(2.5f);
            }

            // Repopulate prize pile
            if (prizeSpawner != null)
            {
                prizeSpawner.SetMachine(machine);
            }

            // Sync with collection manager
            if (ServiceLocator.TryGet<ICollectionService>(out var collectionService))
            {
                collectionService.SetCurrentMachine(machine);
            }
        }

        private void Awake()
        {
            if (claw == null) claw = FindFirstObjectByType<ClawController>();
            if (chuteDetector == null) chuteDetector = FindFirstObjectByType<ChuteDetector>();
            if (prizeSpawner == null) prizeSpawner = FindFirstObjectByType<PrizeSpawner>();
            if (cameraController == null) cameraController = FindFirstObjectByType<ClawCameraController>();
            if (mainDirectionalLight == null)
            {
                var lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i].type == LightType.Directional) { mainDirectionalLight = lights[i]; break; }
                }
            }

            EnsurePerimeterBarriers();
        }

        private void EnsurePerimeterBarriers()
        {
            // Front invisible barrier (keeps prizes inside without blocking camera)
            if (transform.Find("FrontGlassBarrier") == null)
            {
                GameObject front = new GameObject("FrontGlassBarrier");
                front.transform.parent = transform;
                front.transform.localPosition = new Vector3(0f, 1.8f, -1.6f);
                BoxCollider col = front.AddComponent<BoxCollider>();
                col.size = new Vector3(3.4f, 3.6f, 0.15f);
            }

            // Left invisible barrier
            if (transform.Find("LeftGlassBarrier") == null)
            {
                GameObject left = new GameObject("LeftGlassBarrier");
                left.transform.parent = transform;
                left.transform.localPosition = new Vector3(-1.6f, 1.8f, 0f);
                BoxCollider col = left.AddComponent<BoxCollider>();
                col.size = new Vector3(0.15f, 3.6f, 3.4f);
            }

            // Right invisible barrier
            if (transform.Find("RightGlassBarrier") == null)
            {
                GameObject right = new GameObject("RightGlassBarrier");
                right.transform.parent = transform;
                right.transform.localPosition = new Vector3(1.6f, 1.8f, 0f);
                BoxCollider col = right.AddComponent<BoxCollider>();
                col.size = new Vector3(0.15f, 3.6f, 3.4f);
            }
        }

        private void OnEnable()
        {
            if (claw != null)
            {
                claw.OnStateChanged += HandleClawStateChanged;
            }
            if (chuteDetector != null)
            {
                chuteDetector.OnPrizeCollected += HandlePrizeCollected;
            }
        }

        private void OnDisable()
        {
            if (claw != null)
            {
                claw.OnStateChanged -= HandleClawStateChanged;
            }
            if (chuteDetector != null)
            {
                chuteDetector.OnPrizeCollected -= HandlePrizeCollected;
            }
        }

        private void Start()
        {
            if (slotCurr != null && catalog != null)
            {
                slotCurr.transform.localPosition = Vector3.zero;
                slotCurr.Bind(catalog.GetMachine(currentSlotIndex), currentSlotIndex, spawnPrizes: true);

                int prevIdx = currentSlotIndex - 1;
                if (slotPrev != null)
                {
                    slotPrev.transform.localPosition = new Vector3(-machineSpacing, 0f, 0f);
                    slotPrev.Bind(prevIdx >= 0 ? catalog.GetMachine(prevIdx) : null, prevIdx, spawnPrizes: false);
                }

                int nextIdx = currentSlotIndex + 1;
                if (slotNext != null)
                {
                    slotNext.transform.localPosition = new Vector3(machineSpacing, 0f, 0f);
                    slotNext.Bind(nextIdx < catalog.Count ? catalog.GetMachine(nextIdx) : null, nextIdx, spawnPrizes: false);
                }

                ReconnectActiveSlot();
                ApplyAtmosphereImmediate(slotCurr.BoundDefinition);
            }
            else if (prizeSpawner != null)
            {
                prizeSpawner.PopulateInitialPile();
            }
        }

        public void TriggerDrop()
        {
            if (claw == null || IsSwiping) return;
            if (claw.CurrentState == ClawState.Aiming)
            {
                if (ServiceLocator.TryGet<IEconomyService>(out var econService))
                {
                    var currentMach = ServiceLocator.TryGet<ICollectionService>(out var col) ? col.CurrentMachine : null;
                    if (!econService.TryDeductPlayCost(currentMach))
                    {
                        Debug.Log("[MachineController] Not enough coins to play!");
                        return;
                    }
                }

                totalAttempts++;
                if (ServiceLocator.TryGet<IRetentionService>(out var retService))
                {
                    retService.RecordDrop();
                }
                if (chuteDetector != null)
                {
                    chuteDetector.ResetAttempt();
                }
                claw.TriggerDrop();
            }
            else if (claw.CurrentState == ClawState.Carrying)
            {
                claw.TriggerDrop();
            }
        }

        public void SetDragInput(Vector2 delta)
        {
            if (claw != null && !IsSwiping)
            {
                claw.SetInputDelta(delta);
            }
        }

        private void HandleClawStateChanged(ClawState state)
        {
            OnStateChanged?.Invoke(state);
        }

        private void HandlePrizeCollected(Prize prize)
        {
            successfulGrabs++;
            OnPrizeWon?.Invoke(prize);

            if (ServiceLocator.TryGet<IRetentionService>(out var retService))
            {
                var curMach = ServiceLocator.TryGet<ICollectionService>(out var col) ? col.CurrentMachine : null;
                retService.RecordPrizeWon(curMach != null ? curMach.machineId : "", prize != null ? (int)prize.Rarity : 0);
            }

            ServiceLocator.Get<IAudioService>()?.PlayWin();

            Vector3 winPos = chuteDetector != null ? chuteDetector.transform.position : (prize != null ? prize.transform.position : Vector3.zero);
            ClawJuiceEffects.Instance?.PlayWinCelebration(winPos);

            bool isFirstDiscovery = false;
            if (ServiceLocator.TryGet<ICollectionService>(out var colService))
            {
                string prizeId = (prize != null && prize.Definition != null) ? prize.Definition.id : (prize != null ? prize.name : "");
                isFirstDiscovery = !colService.IsPrizeDiscovered(prizeId);
                colService.RegisterCollectedPrize(prize);
            }

            if (prizeSpawner != null)
            {
                prizeSpawner.SpawnRefill();
            }

            // Only show celebration win modal on first-time discovery; repeat prizes skip modal
            if (isFirstDiscovery)
            {
                var winModal = ClawMachine.UI.PrizeWinModalUI.Instance;
                if (winModal == null)
                {
                    winModal = FindFirstObjectByType<ClawMachine.UI.PrizeWinModalUI>(FindObjectsInactive.Include);
                }
                if (winModal != null)
                {
                    winModal.ShowWin(prize);
                }
            }
        }
    }
}
