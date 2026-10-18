using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Prize : MonoBehaviour
    {
        private static readonly Dictionary<PrizeDefinition, PhysicsMaterial> sharedMaterials = new Dictionary<PrizeDefinition, PhysicsMaterial>();

        [SerializeField] private PrizeDefinition definition;
        [SerializeField] private PrizeRarity fallbackRarity = PrizeRarity.Normal;

        public PrizeDefinition Definition => definition;
        public PrizeRarity Rarity => definition != null ? definition.rarity : fallbackRarity;
        public Rigidbody Rigidbody { get; private set; }

        public void SetFallbackRarity(PrizeRarity r)
        {
            fallbackRarity = r;
        }

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            if (definition != null)
            {
                ApplyDefinitionPhysics();
            }
        }

        public void Initialize(PrizeDefinition def)
        {
            definition = def;
            ApplyDefinitionPhysics();
        }

        private void ApplyDefinitionPhysics()
        {
            if (definition == null || Rigidbody == null) return;
            Rigidbody.mass = definition.mass;

            if (!sharedMaterials.TryGetValue(definition, out PhysicsMaterial mat) || mat == null)
            {
                mat = new PhysicsMaterial($"PhysMat_{definition.id}")
                {
                    bounciness = definition.bounciness,
                    dynamicFriction = definition.dynamicFriction,
                    staticFriction = definition.staticFriction,
                    frictionCombine = PhysicsMaterialCombine.Average,
                    bounceCombine = PhysicsMaterialCombine.Average
                };
                sharedMaterials[definition] = mat;
            }

            Collider[] cols = GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i] != null && !cols[i].isTrigger)
                {
                    cols[i].sharedMaterial = mat;
                }
            }
        }
    }
}
