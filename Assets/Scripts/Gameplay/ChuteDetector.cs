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
        private float suppressUntilTime = 0f;

        public void Suppress(float durationSeconds)
        {
            suppressUntilTime = Time.time + durationSeconds;
            collectedThisAttempt.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            Prize prize = other.GetComponentInParent<Prize>();
            if (prize != null)
            {
                if (Time.time < suppressUntilTime)
                {
                    // Suppressed during machine setup/settling - quietly clean up stray
                    Destroy(prize.gameObject, 0.1f);
                    return;
                }

                if (!collectedThisAttempt.Contains(prize))
                {
                    collectedThisAttempt.Add(prize);
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
