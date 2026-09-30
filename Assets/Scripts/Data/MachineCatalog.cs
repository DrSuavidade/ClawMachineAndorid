using System;
using UnityEngine;

namespace ClawMachine.Data
{
    [CreateAssetMenu(fileName = "MachineCatalog", menuName = "ClawMachine/Machine Catalog")]
    public class MachineCatalog : ScriptableObject
    {
        [Header("Master Machine Registry")]
        [SerializeField] private MachineDefinition[] machines = Array.Empty<MachineDefinition>();

        public MachineDefinition[] Machines => machines;
        public int Count => machines != null ? machines.Length : 0;

        public MachineDefinition GetMachine(int index)
        {
            if (machines == null || machines.Length == 0) return null;
            if (index < 0 || index >= machines.Length) return machines[0];
            return machines[index];
        }

        public MachineDefinition GetMachine(string id)
        {
            if (machines == null) return null;
            for (int i = 0; i < machines.Length; i++)
            {
                if (machines[i] != null && machines[i].machineId.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    return machines[i];
                }
            }
            return null;
        }

        public int IndexOf(MachineDefinition def)
        {
            if (machines == null || def == null) return -1;
            for (int i = 0; i < machines.Length; i++)
            {
                if (machines[i] == def || (machines[i] != null && machines[i].machineId == def.machineId))
                {
                    return i;
                }
            }
            return -1;
        }

        public MachineDefinition GetNext(MachineDefinition cur)
        {
            if (machines == null || machines.Length == 0) return null;
            int idx = IndexOf(cur);
            if (idx == -1) return machines[0];
            return machines[(idx + 1) % machines.Length];
        }

        public MachineDefinition GetPrev(MachineDefinition cur)
        {
            if (machines == null || machines.Length == 0) return null;
            int idx = IndexOf(cur);
            if (idx == -1) return machines[0];
            int prev = idx - 1;
            if (prev < 0) prev = machines.Length - 1;
            return machines[prev];
        }

#if UNITY_EDITOR
        public void SetMachinesInEditor(MachineDefinition[] newMachines)
        {
            machines = newMachines;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
