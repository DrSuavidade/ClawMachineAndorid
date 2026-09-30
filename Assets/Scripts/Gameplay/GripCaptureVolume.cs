using System.Collections.Generic;
using UnityEngine;

namespace ClawMachine.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class GripCaptureVolume : MonoBehaviour
    {
        private readonly HashSet<Prize> candidates = new HashSet<Prize>();

        public IReadOnlyCollection<Prize> Candidates => candidates;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Prize>(out var prize))
            {
                candidates.Add(prize);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<Prize>(out var prize))
            {
                candidates.Add(prize);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<Prize>(out var prize))
            {
                candidates.Remove(prize);
            }
        }

        public void CleanUp()
        {
            candidates.RemoveWhere(p => p == null);
        }

        public void Clear()
        {
            candidates.Clear();
        }
    }
}
