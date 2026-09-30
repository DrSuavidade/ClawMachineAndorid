using System.Collections.Generic;
using UnityEngine;

namespace ClawMachine.Gameplay
{
    public class ContactSensor : MonoBehaviour
    {
        public struct ContactData
        {
            public Prize prize;
            public Vector3 worldContactPoint;
        }

        private readonly Dictionary<Prize, Vector3> touchedPrizes = new Dictionary<Prize, Vector3>();

        public IReadOnlyDictionary<Prize, Vector3> TouchedPrizes => touchedPrizes;

        public bool IsTouching(Prize prize) => prize != null && touchedPrizes.ContainsKey(prize);

        public Vector3 GetContactPoint(Prize prize)
        {
            if (prize != null && touchedPrizes.TryGetValue(prize, out var pt))
            {
                return pt;
            }
            return transform.position;
        }

        private void OnCollisionEnter(Collision collision)
        {
            RecordCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            RecordCollision(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.TryGetComponent<Prize>(out var prize))
            {
                touchedPrizes.Remove(prize);
            }
        }

        private void RecordCollision(Collision collision)
        {
            if (collision.collider.TryGetComponent<Prize>(out var prize))
            {
                Vector3 contactPt = collision.GetContact(0).point;
                touchedPrizes[prize] = contactPt;
            }
        }

        public void Clear()
        {
            touchedPrizes.Clear();
        }
    }
}
