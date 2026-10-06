using System;
using ClawMachine.Data;
using ClawMachine.Gameplay;

namespace ClawMachine.Core.Services
{
    public interface ICollectionService
    {
        MachineDefinition CurrentMachine { get; }
        bool HasGoldenClawUnlocked { get; }

        event Action<PrizeDefinition, bool> OnPrizeRegistered;
        event Action<PrizeDefinition, bool, int> OnPrizeAwarded;
        event Action<int> OnDuplicatesSold;
        event Action<MachineDefinition> OnMachineCompleted;
        event Action<MachineDefinition> OnMachineUnlocked;
        event Action<MachineDefinition> OnCurrentMachineChanged;

        void SetCurrentMachine(MachineDefinition machine);
        void RegisterCollectedPrize(Prize prize);
        bool IsPrizeDiscovered(string prizeId);
        int GetPrizeCount(string prizeId);
        int GetTotalDuplicateValue();
        int SellAllDuplicates();
        bool TrySellPrize(string prizeId, int count = 1);
        bool IsMachineUnlocked(string machineId);
        bool IsMachineOwned(string machineId);
        bool TryUnlockMachine(MachineDefinition machine);
        int GetDiscoveredCount(MachineDefinition machine);
        void ResetProgress();
    }
}
