using System;

namespace ClawMachine.Core.Services
{
    public interface IEconomyService
    {
        int Coins { get; }
        event Action<int> OnCoinsChanged;
        event Action<Gameplay.UpgradeType, int> OnUpgradePurchased;

        bool TryDeductPlayCost(Data.MachineDefinition machine);
        void AwardCoins(int amount);
        bool TryPurchaseUpgrade(Gameplay.UpgradeType type);
        int GetUpgradeLevel(Gameplay.UpgradeType type);
        int GetUpgradeCost(Gameplay.UpgradeType type);
        float GetTrolleySpeedMultiplier();
        float GetGripPowerMultiplier();
        float GetDropSpeedMultiplier();
    }
}
