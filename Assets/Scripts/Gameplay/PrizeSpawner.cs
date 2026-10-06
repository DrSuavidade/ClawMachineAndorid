using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    public class PrizeSpawner : MonoBehaviour
    {
        [Header("Pool Setup")]
        [SerializeField] private MachineDefinition machineDefinition;
        [SerializeField] private PrizeDefinition[] prizePool;
        [SerializeField] private Transform spawnAreaCenter;
        [SerializeField] private Vector3 spawnAreaExtents = new Vector3(0.85f, 0.60f, 0.85f);
        [SerializeField] private Transform refillDropPoint;

        [Header("Counts")]
        [SerializeField] private int initialPileCount = 20;

        [Header("Chute Exclusion")]
        [SerializeField] private Transform chuteTransform;
        [SerializeField] private ChuteDetector chuteDetector;
        [SerializeField] private float chuteSafetyMargin = 0.20f;

        private readonly List<Prize> activePrizes = new List<Prize>();
        private PhysicsMaterial runtimePhysMat;

        private void EnsureChuteReferences()
        {
            if (chuteDetector == null)
            {
                chuteDetector = FindFirstObjectByType<ChuteDetector>();
            }
            if (chuteTransform == null && chuteDetector != null)
            {
                chuteTransform = chuteDetector.transform.parent != null ? chuteDetector.transform.parent : chuteDetector.transform;
            }
        }

        private void Awake()
        {
            EnsureChuteReferences();
            runtimePhysMat = new PhysicsMaterial("RuntimeToyMat")
            {
                dynamicFriction = 0.65f,
                staticFriction = 0.75f,
                bounciness = 0.12f,
                frictionCombine = PhysicsMaterialCombine.Maximum,
                bounceCombine = PhysicsMaterialCombine.Average
            };

            EnsurePrizePool();
        }

        public void EnsurePrizePool()
        {
            bool needsPool = (prizePool == null || prizePool.Length == 0);
            if (!needsPool)
            {
                bool hasAnyValid = false;
                for (int i = 0; i < prizePool.Length; i++)
                {
                    if (prizePool[i] != null)
                    {
                        hasAnyValid = true;
                        break;
                    }
                }
                needsPool = !hasAnyValid;
            }

            if (needsPool)
            {
                if (machineDefinition != null && machineDefinition.prizes != null && machineDefinition.prizes.Length > 0)
                {
                    prizePool = machineDefinition.prizes;
                }
                else if (ServiceLocator.TryGet<ICollectionService>(out var collService) && collService.CurrentMachine != null && collService.CurrentMachine.prizes != null)
                {
                    prizePool = collService.CurrentMachine.prizes;
                    machineDefinition = collService.CurrentMachine;
                }
                else
                {
                    Debug.LogWarning("[PrizeSpawner] No prize pool available. Please assign MachineDefinition or configure current machine in inspector.");
                }
            }
        }

        private void Start()
        {
            EnsurePrizePool();
            if (activePrizes.Count == 0)
            {
                PopulateInitialPile();
            }
        }

        public void SetMachine(MachineDefinition machine)
        {
            if (machine == null) return;
            machineDefinition = machine;
            if (machine.prizes != null && machine.prizes.Length > 0)
            {
                prizePool = machine.prizes;
            }
            PopulateInitialPile();
        }

        public void PopulateInitialPile()
        {
            EnsureChuteReferences();
            if (chuteDetector != null)
            {
                chuteDetector.Suppress(2.5f);
            }
            EnsurePrizePool();
            ClearExistingPrizes();

            for (int i = 0; i < initialPileCount; i++)
            {
                Vector3 spawnPos = GetRandomPilePosition(i);
                SpawnSingle(spawnPos, Random.rotation);
            }
        }

        public void SpawnRefill()
        {
            Vector3 dropPos = refillDropPoint != null ? refillDropPoint.position : (GetSpawnCenter() + Vector3.up * 2.5f);
            dropPos += new Vector3(Random.Range(-0.20f, 0.20f), 0, Random.Range(-0.20f, 0.20f));
            if (IsInsideChuteZone(dropPos))
            {
                dropPos.x = Mathf.Max(dropPos.x, -0.30f);
                dropPos.z = Mathf.Max(dropPos.z, -0.30f);
            }
            SpawnSingle(dropPos, Random.rotation);
        }

        private void SpawnSingle(Vector3 position, Quaternion rotation)
        {
            PrizeDefinition def = GetRandomDefinition();
            if (def == null || def.prefab == null)
            {
                Debug.LogWarning("[PrizeSpawner] No valid prize definition with prefab found. Skipping spawn.");
                return;
            }

            GameObject go = Instantiate(def.prefab, position, rotation, transform);
            if (!go.TryGetComponent<Prize>(out var prize))
            {
                prize = go.AddComponent<Prize>();
            }
            prize.Initialize(def);
            activePrizes.Add(prize);

            int prizeLayer = LayerMask.NameToLayer("Prize");
            if (prizeLayer >= 0)
            {
                prize.gameObject.layer = prizeLayer;
                foreach (Transform child in prize.transform)
                {
                    child.gameObject.layer = prizeLayer;
                }
            }
        }

        private PrizeDefinition GetRandomDefinition()
        {
            EnsurePrizePool();
            if (prizePool == null || prizePool.Length == 0) return null;

            var normals = new List<PrizeDefinition>();
            var rares = new List<PrizeDefinition>();
            var secrets = new List<PrizeDefinition>();

            for (int i = 0; i < prizePool.Length; i++)
            {
                var p = prizePool[i];
                if (p != null && p.prefab != null)
                {
                    if (p.rarity == PrizeRarity.Secret) secrets.Add(p);
                    else if (p.rarity == PrizeRarity.Rare) rares.Add(p);
                    else normals.Add(p);
                }
            }

            // Weighted roll: Secret = 5%, Rare = 25%, Normal = 70%
            float roll = Random.value;
            if (roll < 0.05f && secrets.Count > 0)
            {
                return secrets[Random.Range(0, secrets.Count)];
            }
            if (roll < 0.30f && rares.Count > 0)
            {
                return rares[Random.Range(0, rares.Count)];
            }
            if (normals.Count > 0)
            {
                return normals[Random.Range(0, normals.Count)];
            }

            // Fallback to any valid prize
            var fallback = new List<PrizeDefinition>();
            fallback.AddRange(normals);
            fallback.AddRange(rares);
            fallback.AddRange(secrets);
            return fallback.Count > 0 ? fallback[Random.Range(0, fallback.Count)] : null;
        }

        private Vector3 GetSpawnCenter()
        {
            if (spawnAreaCenter != null) return spawnAreaCenter.position;
            return new Vector3(0.2f, 0.25f, 0.1f);
        }

        private bool IsInsideChuteZone(Vector3 pos)
        {
            float maxX = -0.36f;
            float maxZ = -0.36f;
            if (chuteTransform != null)
            {
                maxX = Mathf.Max(maxX, chuteTransform.position.x + 0.50f + chuteSafetyMargin);
                maxZ = Mathf.Max(maxZ, chuteTransform.position.z + 0.50f + chuteSafetyMargin);
            }
            return (pos.x < maxX && pos.z < maxZ);
        }

        private Vector3 GetRandomPilePosition(int index)
        {
            Vector3 center = GetSpawnCenter();
            float heightOffset = (index / (float)initialPileCount) * spawnAreaExtents.y;

            for (int attempt = 0; attempt < 50; attempt++)
            {
                float rx = Random.Range(-spawnAreaExtents.x, spawnAreaExtents.x);
                float rz = Random.Range(-spawnAreaExtents.z, spawnAreaExtents.z);
                Vector3 candidate = center + new Vector3(rx, heightOffset + 0.15f, rz);
                if (!IsInsideChuteZone(candidate))
                {
                    return candidate;
                }
            }

            // Safe fallback quadrant (+X, +Z away from front-left chute)
            float safeRx = Random.Range(0.05f, spawnAreaExtents.x);
            float safeRz = Random.Range(0.05f, spawnAreaExtents.z);
            return center + new Vector3(safeRx, heightOffset + 0.15f, safeRz);
        }

        public void ClearExistingPrizes()
        {
            for (int i = activePrizes.Count - 1; i >= 0; i--)
            {
                if (activePrizes[i] != null)
                {
                    Destroy(activePrizes[i].gameObject);
                }
            }
            activePrizes.Clear();
        }
    }
}
