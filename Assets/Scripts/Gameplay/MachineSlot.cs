using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Gameplay
{
    /// <summary>
    /// Represents one physical slot in the 3-slot virtual machine carousel (Prev, Current, Next).
    /// Dynamically binds cabinet styling, environment prefab, and prizes for whichever machine is slotted in.
    /// </summary>
    public class MachineSlot : MonoBehaviour
    {
        [Header("Cabinet Components")]
        [SerializeField] private GameObject cabinet;
        [SerializeField] private MeshRenderer[] frameRenderers;
        [SerializeField] private MeshRenderer backdropRenderer;
        [SerializeField] private ChuteDetector chuteDetector;
        [SerializeField] private PrizeSpawner prizeSpawner;

        private GameObject environmentInstance;
        private int boundIndex = -1;
        private MachineDefinition boundDefinition;

        public int BoundIndex => boundIndex;
        public MachineDefinition BoundDefinition => boundDefinition;
        public ChuteDetector ChuteDetector => chuteDetector;
        public PrizeSpawner PrizeSpawner => prizeSpawner;

        public void Initialize(GameObject cabinetObj, MeshRenderer[] frames, MeshRenderer backdrop, ChuteDetector chute, PrizeSpawner spawner)
        {
            cabinet = cabinetObj;
            frameRenderers = frames;
            backdropRenderer = backdrop;
            chuteDetector = chute;
            prizeSpawner = spawner;
        }

        public void Bind(MachineDefinition def, int index, bool spawnPrizes)
        {
            boundIndex = index;
            boundDefinition = def;

            if (def == null || index < 0)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // 1. Recolor cabinet frame
            if (frameRenderers != null)
            {
                for (int i = 0; i < frameRenderers.Length; i++)
                {
                    if (frameRenderers[i] != null)
                    {
                        frameRenderers[i].material.color = def.cabinetFrameColor;
                    }
                }
            }

            // 2. Recolor backdrop
            if (backdropRenderer != null)
            {
                backdropRenderer.material.color = def.cabinetBackdropColor;
            }

            // 3. Swap environment prefab if changed
            string expectedEnvName = $"Environment_{def.machineId}";
            if (environmentInstance != null && environmentInstance.name != expectedEnvName)
            {
                if (Application.isPlaying) Destroy(environmentInstance);
                else DestroyImmediate(environmentInstance);
                environmentInstance = null;
            }

            if (environmentInstance == null && def.environmentPrefab != null)
            {
                environmentInstance = Instantiate(def.environmentPrefab, transform);
                environmentInstance.name = expectedEnvName;
                environmentInstance.transform.localPosition = Vector3.zero;
                environmentInstance.transform.localRotation = Quaternion.identity;
            }

            // 4. Setup prize spawner
            if (prizeSpawner != null)
            {
                if (spawnPrizes)
                {
                    prizeSpawner.SetMachine(def);
                }
                else
                {
                    prizeSpawner.ClearExistingPrizes();
                }
            }
        }
    }
}
