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

        [Header("Stats for Prototype")]
        [SerializeField] private int totalAttempts;
        [SerializeField] private int successfulGrabs;

        public event Action<Prize> OnPrizeWon;
        public event Action<ClawState> OnStateChanged;

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
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.SetCurrentMachine(machine);
            }
        }

        private void Awake()
        {
            if (claw == null) claw = FindFirstObjectByType<ClawController>();
            if (chuteDetector == null) chuteDetector = FindFirstObjectByType<ChuteDetector>();
            if (prizeSpawner == null) prizeSpawner = FindFirstObjectByType<PrizeSpawner>();

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
            if (prizeSpawner != null)
            {
                prizeSpawner.PopulateInitialPile();
            }
        }

        public void TriggerDrop()
        {
            if (claw == null) return;
            if (claw.CurrentState == ClawState.Aiming)
            {
                if (ServiceLocator.TryGet<IEconomyService>(out var econService))
                {
                    var currentMach = ServiceLocator.TryGet<ICollectionService>(out var col) ? col.CurrentMachine : (CollectionManager.Instance != null ? CollectionManager.Instance.CurrentMachine : null);
                    if (!econService.TryDeductPlayCost(currentMach))
                    {
                        Debug.Log("[MachineController] Not enough coins to play!");
                        return;
                    }
                }
                else if (CollectionManager.Instance != null)
                {
                    if (!CollectionManager.Instance.TryDeductPlayCost(CollectionManager.Instance.CurrentMachine))
                    {
                        Debug.Log("[MachineController] Not enough coins to play!");
                        return;
                    }
                }

                totalAttempts++;
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
            if (claw != null)
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

            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.PlayWin();
            }
            else
            {
                ClawAudio.Instance?.PlayWin();
            }

            Vector3 winPos = chuteDetector != null ? chuteDetector.transform.position : (prize != null ? prize.transform.position : Vector3.zero);
            ClawJuiceEffects.Instance?.PlayWinCelebration(winPos);

            if (ServiceLocator.TryGet<ICollectionService>(out var colService))
            {
                colService.RegisterCollectedPrize(prize);
            }
            else if (CollectionManager.Instance != null)
            {
                CollectionManager.Instance.RegisterCollectedPrize(prize);
            }

            if (prizeSpawner != null)
            {
                prizeSpawner.SpawnRefill();
            }
        }
    }
}
