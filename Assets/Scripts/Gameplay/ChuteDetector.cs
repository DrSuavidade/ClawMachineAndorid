using System;
using System.Collections;
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

        private void OnTriggerEnter(Collider other) => TryCollect(other);
        private void OnTriggerStay(Collider other) => TryCollect(other);

        private void TryCollect(Collider other)
        {
            Prize prize = other.GetComponentInParent<Prize>();
            if (prize == null) return;

            // Ignore prizes currently held kinematically by the claw
            if (prize.Rigidbody != null && prize.Rigidbody.isKinematic) return;

            if (Time.time < suppressUntilTime)
            {
                // Suppressed during machine setup/settling - nudge stray toy back into playfield instead of destroying it
                Vector3 safePos = prize.transform.position;
                safePos.x = Mathf.Max(safePos.x, 0.25f);
                safePos.z = Mathf.Max(safePos.z, 0.25f);
                safePos.y = Mathf.Max(safePos.y, 0.55f);
                prize.transform.position = safePos;
                if (prize.Rigidbody != null)
                {
                    prize.Rigidbody.linearVelocity = Vector3.zero;
                    prize.Rigidbody.angularVelocity = Vector3.zero;
                }
                return;
            }

            if (!collectedThisAttempt.Contains(prize))
            {
                collectedThisAttempt.Add(prize);
                Debug.Log($"[ChuteDetector] Collected prize: {prize.name}");
                OnPrizeCollected?.Invoke(prize);
                StartCoroutine(ReturnPrizeAfterDelay(prize, 0.45f));
            }
        }

        private IEnumerator ReturnPrizeAfterDelay(Prize prize, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (prize == null) yield break;

            PrizeSpawner spawner = prize.GetComponentInParent<PrizeSpawner>() ?? FindFirstObjectByType<PrizeSpawner>();
            if (spawner != null)
            {
                spawner.ReturnToPool(prize);
            }
            else
            {
                prize.gameObject.SetActive(false);
            }
        }

        public void ResetAttempt()
        {
            collectedThisAttempt.Clear();
        }
    }
}
