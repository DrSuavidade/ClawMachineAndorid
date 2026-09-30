using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.Core.Services
{
    public class DefaultAssetProvider : IAssetProvider
    {
        private readonly List<GameObject> activeInstances = new List<GameObject>();

        public GameObject InstantiatePrize(PrizeDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject instance = null;

            if (definition != null && definition.prefab != null)
            {
                instance = Object.Instantiate(definition.prefab, position, rotation, parent);
            }

            if (instance != null)
            {
                activeInstances.Add(instance);
            }

            return instance;
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance != null)
            {
                activeInstances.Remove(instance);
                Object.Destroy(instance);
            }
        }

        public void ReleaseAll()
        {
            for (int i = activeInstances.Count - 1; i >= 0; i--)
            {
                if (activeInstances[i] != null)
                {
                    Object.Destroy(activeInstances[i]);
                }
            }
            activeInstances.Clear();
        }
    }
}
