using UnityEngine;

namespace ClawMachine.Data
{
    [CreateAssetMenu(fileName = "ClawConfig_Default", menuName = "ClawMachine/Claw Configuration")]
    public class ClawConfiguration : ScriptableObject
    {
        [Header("Rail Movement")]
        public float moveSpeed = 2.5f;
        public float moveDamping = 8f;
        public Vector2 xBounds = new Vector2(-1.5f, 1.5f);
        public Vector2 zBounds = new Vector2(-1.5f, 1.5f);

        [Header("Vertical Action")]
        public float dropSpeed = 1.8f;
        public float liftSpeed = 1.5f;
        public float returnSpeed = 2.0f;
        public float dropMinY = 1.35f;
        public float homeY = 3.6f;

        [Header("Claw Mechanics")]
        public float openAngle = 38f;
        public float closedAngle = -28f;
        public float armRotateSpeed = 65f;
        public float armMaxTorque = 15f;

        [Header("Grip Solver (Subtle Assistance)")]
        [Tooltip("Minimum arm contacts needed before grip stabilization applies")]
        public int minContactsForAssist = 2;
        [Tooltip("Extra vertical holding force multiplier during lift")]
        public float gripSupportStrength = 2.5f;
        [Tooltip("Max weight this claw can comfortably lift without heavy slip")]
        public float maxGripCapacity = 2.5f;

        [Header("Targeting & Alignment")]
        [Tooltip("Max horizontal distance between reticle shadow center and toy center to successfully grasp")]
        public float grabToleranceRadius = 0.22f;
        [Tooltip("Base slip chance multiplier when moving fast while carrying")]
        public float slipSensitivity = 1.0f;

        [Header("State Timing")]
        [Tooltip("Seconds to wait for arms to close before evaluating grip")]
        public float closingDuration = 1.1f;
        [Tooltip("Seconds to wait for arms to open during release")]
        public float releasingDuration = 0.8f;
        [Tooltip("Seconds to pause after releasing before returning to Aiming")]
        public float resolveDuration = 1.0f;

        [Header("Descent Targeting")]
        [Tooltip("Capsule scan radius for finding prizes under reticle")]
        public float descentScanRadius = 0.60f;
        [Tooltip("Minimum hoist Y during descent")]
        public float descentMinY = 0.88f;
        [Tooltip("Maximum hoist Y during descent")]
        public float descentMaxY = 2.2f;
    }
}
