using System.Threading.Tasks;
using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Core.Services
{
    public interface IAssetProvider
    {
        GameObject InstantiatePrize(PrizeDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null);
        Task<GameObject> InstantiatePrizeAsync(PrizeDefinition definition, Vector3 position, Quaternion rotation, Transform parent = null);
        void ReleaseInstance(GameObject instance);
        void ReleaseAll();
        Task PreloadMachinePrizesAsync(MachineDefinition machine);
        void UnloadMachinePrizes(MachineDefinition machine);
    }
}
