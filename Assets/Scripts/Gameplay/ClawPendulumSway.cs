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

        public Vector3 CurrentSwayOffset { get; private set; }
        public Quaternion CurrentSwayRotation { get; private set; } = Quaternion.identity;
        public float CableLength { get; set; } = 0.8f;

        public void Initialize(Transform trolleyTransform, Transform hoistTransform, Transform visualBody)
        {
            trolley = trolleyTransform;
            hoist = hoistTransform;
            visualClawBody = visualBody;

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
            trolleyVelocity = (currentPos - prevTrolleyPos) / dt;
            trolleyAcceleration = (trolleyVelocity - prevTrolleyVelocity) / dt;

            prevTrolleyPos = currentPos;
            prevTrolleyVelocity = trolleyVelocity;

            // Clamp acceleration spikes from discrete teleports
            trolleyAcceleration.x = Mathf.Clamp(trolleyAcceleration.x, -25f, 25f);
            trolleyAcceleration.z = Mathf.Clamp(trolleyAcceleration.z, -25f, 25f);

            // 2. Harmonic pendulum physics
            float effLength = Mathf.Max(0.25f, CableLength);
            float omega = Mathf.Sqrt(gravity / effLength);
            float damping = 2f * dampingRatio * omega;

            // Acceleration excitation: forward trolley accel forces claw backward
            float drivingX = (-trolleyAcceleration.z / effLength) * swaySensitivity;
            float drivingZ = (trolleyAcceleration.x / effLength) * swaySensitivity;

            // Angular acceleration: theta'' = -damping * theta' - omega^2 * theta + driving
            float alphaX = -damping * angleVelocity.x - (omega * omega * angle.x) + drivingX;
            float alphaZ = -damping * angleVelocity.y - (omega * omega * angle.y) + drivingZ;

            angleVelocity.x += alphaX * dt;
            angleVelocity.y += alphaZ * dt;

            angle.x += angleVelocity.x * dt;
            angle.y += angleVelocity.y * dt;

            // Clamp max swing angle
            float maxRad = maxAngleDegrees * Mathf.Deg2Rad;
            angle.x = Mathf.Clamp(angle.x, -maxRad, maxRad);
            angle.y = Mathf.Clamp(angle.y, -maxRad, maxRad);

            // 3. Compute offset and rotation
            float degX = angle.x * Mathf.Rad2Deg;
            float degZ = angle.y * Mathf.Rad2Deg;
            CurrentSwayRotation = Quaternion.Euler(degX, 0f, degZ);

            // Displacement relative to vertical cable:
            // roll around Z moves in X; pitch around X moves in Z
            float offsetX = Mathf.Sin(angle.y) * effLength;
            float offsetZ = -Mathf.Sin(angle.x) * effLength;
            CurrentSwayOffset = new Vector3(offsetX, 0f, offsetZ);

            // 4. Apply to visual body
            if (visualClawBody != null)
            {
                visualClawBody.localRotation = CurrentSwayRotation;
                visualClawBody.localPosition = CurrentSwayOffset;
            }
        }
    }
}
