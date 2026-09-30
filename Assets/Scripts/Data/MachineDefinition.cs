using UnityEngine;

namespace ClawMachine.Data
{
    [CreateAssetMenu(fileName = "Machine_", menuName = "ClawMachine/Machine Definition")]
    public class MachineDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string machineId = "toy_box";
        public string displayName = "Toy Box";
        [TextArea] public string description = "A colorful starter machine filled with classic nursery and playroom favorites.";

        [Header("Economy & Access")]
        public int unlockCost = 0;
        public int entryCost = 0;
        public int passiveIncomePerMinute = 5;

        [Header("Visual Theme")]
        public Color cabinetFrameColor = new Color(0.85f, 0.25f, 0.25f);
        public Color cabinetBackdropColor = new Color(0.40f, 0.48f, 0.62f);

        [Header("Prizes (Exactly 9: 5 Normal, 3 Rare, 1 Secret)")]
        public PrizeDefinition[] prizes = new PrizeDefinition[9];

        public int NormalCount => CountRarity(PrizeRarity.Normal);
        public int RareCount => CountRarity(PrizeRarity.Rare);
        public int SecretCount => CountRarity(PrizeRarity.Secret);

        private int CountRarity(PrizeRarity r)
        {
            if (prizes == null) return 0;
            int c = 0;
            for (int i = 0; i < prizes.Length; i++)
            {
                if (prizes[i] != null && prizes[i].rarity == r) c++;
            }
            return c;
        }
    }
}
