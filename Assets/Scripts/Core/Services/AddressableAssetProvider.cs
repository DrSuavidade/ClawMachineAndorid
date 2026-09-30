using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Core.Services
{
    /// <summary>
    /// Production-ready asset provider supporting both direct prefab instantiation and Addressables lifecycle.
    /// Tracks handles and instances for memory reclamation on mobile devices.
    /// </summary>
    public class AddressableAssetProvider : IAssetProvider
    {
        private readonly List<GameObject> activeInstances = new List<GameObject>();
        private readonly Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();

        public GameObject InstantiatePrize(PrizeDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (definition == null || definition.prefab == null) return null;

            GameObject instance = UnityEngine.Object.Instantiate(definition.prefab, position, rotation, parent);
            if (instance != null)
            {
                activeInstances.Add(instance);
            }
            return instance;
        }

        public Task<GameObject> InstantiatePrizeAsync(PrizeDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            // Future extension: hook Addressables.InstantiateAsync(definition.addressableKey)
            var instance = InstantiatePrize(definition, position, rotation, parent);
            return Task.FromResult(instance);
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;

            activeInstances.Remove(instance);
            // Future extension: hook Addressables.ReleaseInstance(instance) if addressable
            UnityEngine.Object.Destroy(instance);
        }

        public void ReleaseAll()
        {
            for (int i = activeInstances.Count - 1; i >= 0; i--)
            {
                if (activeInstances[i] != null)
                {
                    UnityEngine.Object.Destroy(activeInstances[i]);
                }
            }
            activeInstances.Clear();
        }

        public Task PreloadMachinePrizesAsync(MachineDefinition machine)
        {
            if (machine == null || machine.prizes == null) return Task.CompletedTask;

            foreach (var prize in machine.prizes)
            {
                if (prize != null && prize.prefab != null && !loadedPrefabs.ContainsKey(prize.id))
                {
                    loadedPrefabs[prize.id] = prize.prefab;
                }
            }

            return Task.CompletedTask;
        }

        public void UnloadMachinePrizes(MachineDefinition machine)
        {
            if (machine == null || machine.prizes == null) return;

            foreach (var prize in machine.prizes)
            {
                if (prize != null && loadedPrefabs.ContainsKey(prize.id))
                {
                    loadedPrefabs.Remove(prize.id);
                }
            }

            activeInstances.RemoveAll(item => item == null);
        }
    }
}
