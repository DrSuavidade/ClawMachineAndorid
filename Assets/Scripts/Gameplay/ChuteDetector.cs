using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClawMachine.Gameplay
{
    [RequireComponent(typeof(Collider))]
    public class ChuteDetector : MonoBehaviour
    {
        public event Action<Prize> OnPrizeCollected;

        private readonly HashSet<Prize> collectedThisAttempt = new HashSet<Prize>();

        private void OnTriggerEnter(Collider other)
        {
            Prize prize = other.GetComponentInParent<Prize>();
            if (prize != null)
            {
                if (!collectedThisAttempt.Contains(prize))
                {
                    collectedThisAttempt.Add(prize);
                    ClawAudio.Instance?.PlayWin();
                    OnPrizeCollected?.Invoke(prize);
                    Destroy(prize.gameObject, 0.5f);
                }
            }
        }

        public void ResetAttempt()
        {
            collectedThisAttempt.Clear();
        }
    }
}
