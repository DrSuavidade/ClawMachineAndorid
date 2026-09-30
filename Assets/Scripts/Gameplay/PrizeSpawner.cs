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
        private IAssetProvider assetProvider;

        public void SetAssetProvider(IAssetProvider provider)
        {
            assetProvider = provider;
        }

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
            if (assetProvider == null)
            {
                if (!ServiceLocator.TryGet<IAssetProvider>(out assetProvider))
                {
                    assetProvider = new DefaultAssetProvider();
                    ServiceLocator.Register<IAssetProvider>(assetProvider);
                }
            }
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

            Prize prize = null;
            if (def != null && def.prefab != null && assetProvider != null)
            {
                GameObject go = assetProvider.InstantiatePrize(def, position, rotation, transform);
                if (go != null)
                {
                    if (!go.TryGetComponent<Prize>(out prize))
                    {
                        prize = go.AddComponent<Prize>();
                    }
                    prize.Initialize(def);
                    activePrizes.Add(prize);
                }
            }
            else
            {
                // Fallback runtime primitive toy
                GameObject fallback = CreateFallbackPrimitive(position, rotation, def);
                prize = fallback.GetComponent<Prize>();
                activePrizes.Add(prize);
            }

            if (prize != null)
            {
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
        }

        private PrizeDefinition GetRandomDefinition()
        {
            EnsurePrizePool();
            if (prizePool == null || prizePool.Length == 0) return null;
            var valid = new List<PrizeDefinition>();
            for (int i = 0; i < prizePool.Length; i++)
            {
                if (prizePool[i] != null && prizePool[i].prefab != null)
                {
                    valid.Add(prizePool[i]);
                }
            }
            if (valid.Count > 0)
            {
                return valid[Random.Range(0, valid.Count)];
            }

            // Fallback: any non-null definition
            for (int i = 0; i < prizePool.Length; i++)
            {
                if (prizePool[i] != null) valid.Add(prizePool[i]);
            }
            if (valid.Count == 0) return null;
            return valid[Random.Range(0, valid.Count)];
        }

        private GameObject CreateFallbackPrimitive(Vector3 position, Quaternion rotation, PrizeDefinition def = null)
        {
            Color[] colors = {
                new Color(0.92f, 0.30f, 0.30f), // Red
                new Color(0.28f, 0.65f, 0.95f), // Blue
                new Color(0.30f, 0.85f, 0.45f), // Green
                new Color(0.98f, 0.82f, 0.22f), // Yellow
                new Color(0.95f, 0.55f, 0.22f), // Orange
                new Color(0.72f, 0.35f, 0.92f)  // Purple
            };

            string[] types = { "Cube", "Sphere", "Capsule", "LongBar", "LShape", "LargeSphere", "SmallSphere", "Cylinder", "Dumbbell", "Donut" };
            int pick = Random.Range(0, types.Length);
            string chosenType = types[pick];

            float roll = Random.value;
            PrizeRarity rarity = def != null ? def.rarity : PrizeRarity.Normal;
            Color toyColor;

            if (def == null)
            {
                if (roll > 0.88f)
                {
                    rarity = PrizeRarity.Secret; // 12% Radiant Magenta
                    toyColor = new Color(0.95f, 0.20f, 0.90f);
                }
                else if (roll > 0.65f)
                {
                    rarity = PrizeRarity.Rare; // 23% Lustrous Gold
                    toyColor = new Color(1.0f, 0.82f, 0.12f);
                }
                else
                {
                    rarity = PrizeRarity.Normal; // 65% Colorful Normal
                    toyColor = colors[Random.Range(0, colors.Length)];
                }
            }
            else
            {
                toyColor = rarity switch
                {
                    PrizeRarity.Secret => new Color(0.95f, 0.20f, 0.90f),
                    PrizeRarity.Rare => new Color(1.0f, 0.82f, 0.12f),
                    _ => colors[Random.Range(0, colors.Length)]
                };
            }

            string prizeTitle = def != null ? def.displayName : $"{rarity}_{chosenType}";
            GameObject root = new GameObject($"Prize_{prizeTitle}_{Random.Range(100, 999)}");
            root.transform.parent = transform;
            root.transform.position = position;
            root.transform.rotation = rotation;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = toyColor;

            float mass = 0.5f;

            switch (chosenType)
            {
                case "Cube":
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.parent = root.transform;
                    cube.transform.localPosition = Vector3.zero;
                    cube.transform.localScale = Vector3.one * 0.42f;
                    cube.GetComponent<Renderer>().sharedMaterial = mat;
                    cube.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.50f;
                    break;

                case "Sphere":
                    GameObject sph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sph.transform.parent = root.transform;
                    sph.transform.localPosition = Vector3.zero;
                    sph.transform.localScale = Vector3.one * 0.44f;
                    sph.GetComponent<Renderer>().sharedMaterial = mat;
                    sph.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.48f;
                    break;

                case "Capsule":
                    GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    cap.transform.parent = root.transform;
                    cap.transform.localPosition = Vector3.zero;
                    cap.transform.localScale = new Vector3(0.36f, 0.50f, 0.36f);
                    cap.GetComponent<Renderer>().sharedMaterial = mat;
                    cap.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.52f;
                    break;

                case "LongBar":
                    GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bar.transform.parent = root.transform;
                    bar.transform.localPosition = Vector3.zero;
                    bar.transform.localScale = new Vector3(0.22f, 0.22f, 0.96f);
                    bar.GetComponent<Renderer>().sharedMaterial = mat;
                    bar.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.65f;
                    break;

                case "LShape":
                    // Compound primitive: 2 joined cubes forming an L
                    GameObject baseLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    baseLeg.transform.parent = root.transform;
                    baseLeg.transform.localPosition = new Vector3(0f, -0.12f, 0.12f);
                    baseLeg.transform.localScale = new Vector3(0.24f, 0.24f, 0.55f);
                    baseLeg.GetComponent<Renderer>().sharedMaterial = mat;
                    baseLeg.GetComponent<Collider>().material = runtimePhysMat;

                    GameObject upLeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    upLeg.transform.parent = root.transform;
                    upLeg.transform.localPosition = new Vector3(0f, 0.16f, -0.12f);
                    upLeg.transform.localScale = new Vector3(0.24f, 0.45f, 0.24f);
                    upLeg.GetComponent<Renderer>().sharedMaterial = mat;
                    upLeg.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.68f;
                    break;

                case "LargeSphere":
                    GameObject bigSph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    bigSph.transform.parent = root.transform;
                    bigSph.transform.localPosition = Vector3.zero;
                    bigSph.transform.localScale = Vector3.one * 0.58f;
                    bigSph.GetComponent<Renderer>().sharedMaterial = mat;
                    bigSph.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.75f;
                    break;

                case "SmallSphere":
                    GameObject smSph = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    smSph.transform.parent = root.transform;
                    smSph.transform.localPosition = Vector3.zero;
                    smSph.transform.localScale = Vector3.one * 0.28f;
                    smSph.GetComponent<Renderer>().sharedMaterial = mat;
                    smSph.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.25f;
                    break;

                case "Cylinder":
                    GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cyl.transform.parent = root.transform;
                    cyl.transform.localPosition = Vector3.zero;
                    cyl.transform.localScale = new Vector3(0.40f, 0.22f, 0.40f);
                    cyl.GetComponent<Renderer>().sharedMaterial = mat;
                    cyl.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.52f;
                    break;

                case "Dumbbell":
                    // Bar
                    GameObject dBar = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    dBar.transform.parent = root.transform;
                    dBar.transform.localPosition = Vector3.zero;
                    dBar.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    dBar.transform.localScale = new Vector3(0.12f, 0.28f, 0.12f);
                    dBar.GetComponent<Renderer>().sharedMaterial = mat;
                    dBar.GetComponent<Collider>().material = runtimePhysMat;

                    // Left Bell
                    GameObject lBell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    lBell.transform.parent = root.transform;
                    lBell.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
                    lBell.transform.localScale = Vector3.one * 0.28f;
                    lBell.GetComponent<Renderer>().sharedMaterial = mat;
                    lBell.GetComponent<Collider>().material = runtimePhysMat;

                    // Right Bell
                    GameObject rBell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    rBell.transform.parent = root.transform;
                    rBell.transform.localPosition = new Vector3(0.25f, 0f, 0f);
                    rBell.transform.localScale = Vector3.one * 0.28f;
                    rBell.GetComponent<Renderer>().sharedMaterial = mat;
                    rBell.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.62f;
                    break;

                case "Donut":
                    // Ring constructed with 4 segments with hollow center
                    float ringOffset = 0.14f;
                    Vector3 barScale = new Vector3(0.38f, 0.14f, 0.14f);

                    GameObject segTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segTop.transform.parent = root.transform;
                    segTop.transform.localPosition = new Vector3(0f, 0f, ringOffset);
                    segTop.transform.localScale = barScale;
                    segTop.GetComponent<Renderer>().sharedMaterial = mat;
                    segTop.GetComponent<Collider>().material = runtimePhysMat;

                    GameObject segBot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segBot.transform.parent = root.transform;
                    segBot.transform.localPosition = new Vector3(0f, 0f, -ringOffset);
                    segBot.transform.localScale = barScale;
                    segBot.GetComponent<Renderer>().sharedMaterial = mat;
                    segBot.GetComponent<Collider>().material = runtimePhysMat;

                    Vector3 sideScale = new Vector3(0.14f, 0.14f, 0.16f);
                    GameObject segLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segLeft.transform.parent = root.transform;
                    segLeft.transform.localPosition = new Vector3(-ringOffset, 0f, 0f);
                    segLeft.transform.localScale = sideScale;
                    segLeft.GetComponent<Renderer>().sharedMaterial = mat;
                    segLeft.GetComponent<Collider>().material = runtimePhysMat;

                    GameObject segRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    segRight.transform.parent = root.transform;
                    segRight.transform.localPosition = new Vector3(ringOffset, 0f, 0f);
                    segRight.transform.localScale = sideScale;
                    segRight.GetComponent<Renderer>().sharedMaterial = mat;
                    segRight.GetComponent<Collider>().material = runtimePhysMat;
                    mass = 0.50f;
                    break;
            }

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            Prize prize = root.AddComponent<Prize>();
            if (def != null)
            {
                prize.Initialize(def);
            }
            prize.SetFallbackRarity(rarity);
            return root;
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
                    if (assetProvider != null)
                    {
                        assetProvider.ReleaseInstance(activePrizes[i].gameObject);
                    }
                    else
                    {
                        Destroy(activePrizes[i].gameObject);
                    }
                }
            }
            activePrizes.Clear();
        }
    }
}
