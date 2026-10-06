using UnityEngine;

namespace ClawMachine.Gameplay
{
    /// <summary>
    /// Simulates authentic arcade claw pendulum cable sway.
    /// Trolley acceleration excites 2D angular harmonic oscillation.
    /// Sway offsets the claw hub and grip socket, requiring skill timing to drop at nadir.
    /// </summary>
    public class ClawPendulumSway : MonoBehaviour
    {
        [Header("Pendulum Tuning")]
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float dampingRatio = 0.18f;
        [SerializeField] private float maxAngleDegrees = 16f;
        [SerializeField] private float defaultCableLength = 0.8f;
        [SerializeField] private float swaySensitivity = 1.0f;

        [Header("Hierarchy Transforms")]
        [SerializeField] private Transform trolley;
        [SerializeField] private Transform hoist;
        [SerializeField] private Transform visualClawBody;

        private Vector3 prevTrolleyPos;
        private Vector3 trolleyVelocity;
        private Vector3 prevTrolleyVelocity;
        private Vector3 trolleyAcceleration;

        // Angular state in radians: X = pitch (driven by Z accel), Y = roll (driven by X accel)
        private Vector2 angle;
        private Vector2 angleVelocity;
        private Rigidbody targetRb;

        public Vector3 CurrentSwayOffset { get; private set; }
        public Quaternion CurrentSwayRotation { get; private set; } = Quaternion.identity;
        public float CableLength { get; set; } = 0.8f;

        public void Initialize(Transform trolleyTransform, Transform hoistTransform, Transform visualBody)
        {
            trolley = trolleyTransform;
            hoist = hoistTransform;
            visualClawBody = visualBody;
            if (visualClawBody != null)
            {
                targetRb = visualClawBody.GetComponent<Rigidbody>();
            }

            if (trolley != null)
            {
                prevTrolleyPos = trolley.position;
            }
            CableLength = defaultCableLength;
        }

        public void ResetSway()
        {
            angle = Vector2.zero;
            angleVelocity = Vector2.zero;
            CurrentSwayOffset = Vector3.zero;
            CurrentSwayRotation = Quaternion.identity;
            if (visualClawBody != null)
            {
                visualClawBody.localRotation = Quaternion.identity;
                visualClawBody.localPosition = Vector3.zero;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0.0001f || trolley == null) return;

            // 1. Calculate trolley velocity and acceleration
            Vector3 currentPos = trolley.position;
            Vector3 rawVelocity = (currentPos - prevTrolleyPos) / dt;
            trolleyVelocity = Vector3.Lerp(trolleyVelocity, rawVelocity, Mathf.Clamp01(dt * 20f));
            trolleyAcceleration = (trolleyVelocity - prevTrolleyVelocity) / dt;

            prevTrolleyPos = currentPos;
            prevTrolleyVelocity = trolleyVelocity;

            // Clamp acceleration spikes
            trolleyAcceleration.x = Mathf.Clamp(trolleyAcceleration.x, -25f, 25f);
            trolleyAcceleration.z = Mathf.Clamp(trolleyAcceleration.z, -25f, 25f);

            // 2. Pendulum harmonic physics
            float effLength = Mathf.Max(0.25f, CableLength);
            float omega = Mathf.Sqrt(gravity / effLength);
            float damping = 2f * dampingRatio * omega;

            // Inertial excitation: Opposite to motion!
            // When trolley moves/accelerates +X, claw bob lags in -X.
            // When trolley moves/accelerates +Z, claw bob lags in -Z.
            // Steady velocity drag: keeps claw trailing opposite to travel direction while moving.
            float velocityDragGain = 0.55f;
            float accelGain = 0.65f * swaySensitivity;

            // Bob displacement acceleration:
            // a_bob = -omega^2 * d - damping * v_bob - (accelGain * a_trolley + velocityDragGain * v_trolley)
            float drivingX = (-trolleyAcceleration.x * accelGain) - (trolleyVelocity.x * velocityDragGain);
            float drivingZ = (-trolleyAcceleration.z * accelGain) - (trolleyVelocity.z * velocityDragGain);

            float accelDispX = -(omega * omega * angle.x) - (damping * angleVelocity.x) + drivingX;
            float accelDispZ = -(omega * omega * angle.y) - (damping * angleVelocity.y) + drivingZ;

            angleVelocity.x += accelDispX * dt;
            angleVelocity.y += accelDispZ * dt;

            angle.x += angleVelocity.x * dt;
            angle.y += angleVelocity.y * dt;

            // 3. Clamp maximum horizontal displacement
            float maxDisplacement = effLength * Mathf.Sin(maxAngleDegrees * Mathf.Deg2Rad);
            Vector2 disp2D = new Vector2(angle.x, angle.y);
            if (disp2D.magnitude > maxDisplacement)
            {
                disp2D = disp2D.normalized * maxDisplacement;
                angle.x = disp2D.x;
                angle.y = disp2D.y;
            }

            // 4. Calculate 3D cable direction and rotation
            float dispX = angle.x;
            float dispZ = angle.y;
            float hSqr = effLength * effLength - dispX * dispX - dispZ * dispZ;
            float h = Mathf.Sqrt(Mathf.Max(0.01f, hSqr));

            // Cable direction vector from trolley downwards to claw bob
            Vector3 cableDir = new Vector3(dispX, -h, dispZ).normalized;
            CurrentSwayRotation = Quaternion.FromToRotation(Vector3.down, cableDir);

            // Vertical offset lift as pendulum swings: (h - effLength) is negative
            CurrentSwayOffset = new Vector3(dispX, -(effLength - h), dispZ);

            // 5. Apply to visual body
            if (visualClawBody != null)
            {
                visualClawBody.localRotation = CurrentSwayRotation;
                visualClawBody.localPosition = CurrentSwayOffset;
                if (targetRb != null && targetRb.isKinematic && hoist != null)
                {
                    targetRb.MovePosition(hoist.position + CurrentSwayOffset);
                    targetRb.MoveRotation(hoist.rotation * CurrentSwayRotation);
                }
            }
        }
    }
}
