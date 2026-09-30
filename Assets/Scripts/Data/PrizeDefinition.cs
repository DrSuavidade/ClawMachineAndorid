using UnityEngine;

namespace ClawMachine.Data
{
    public enum PrizeRarity
    {
        Normal,
        Rare,
        Secret
    }

    [CreateAssetMenu(fileName = "Prize_", menuName = "ClawMachine/Prize Definition")]
    public class PrizeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string id;
        public string displayName;
        public PrizeRarity rarity = PrizeRarity.Normal;

        [Header("Visuals & Physics")]
        public GameObject prefab;
        public float mass = 0.5f;
        public float bounciness = 0.1f;
        public float dynamicFriction = 0.6f;
        public float staticFriction = 0.7f;

        [Header("Economy Factors")]
        public int duplicateCoinValue = 20;
        public float sellMultiplier = 1f;
        public float essenceMultiplier = 1f;
    }
}
