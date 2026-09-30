using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public class Prize : MonoBehaviour
    {
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
        }
    }
}
