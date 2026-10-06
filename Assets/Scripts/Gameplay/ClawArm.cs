using UnityEngine;

namespace ClawMachine.Gameplay
{
    public class ClawArm : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Rigidbody armRigidbody;
        [SerializeField] private Transform tipTransform;
        [SerializeField] private float baseAngleY;

        [Header("Angle State")]
        [SerializeField] private float targetAngle = 38f;
        [SerializeField] private float currentAngle = 38f;
        [SerializeField] private float rotateSpeed = 160f;

        // Physical base offset where the 3 prong tips meet perfectly in the center (Tip X = 0)
        public const float PhysicalZeroOffset = -24.5f;
        public const float MinAngle = 2f; // Minimum closed angle (fingers almost touch, never cross)

        public Transform Tip => tipTransform != null ? tipTransform : transform;
        public float CurrentAngle => currentAngle;

        private void Awake()
        {
            if (armRigidbody == null) armRigidbody = GetComponent<Rigidbody>();
            if (armRigidbody != null) armRigidbody.isKinematic = true;
        }

        public void Setup(Rigidbody rb, Transform tip, float angleY, float initialAngle = 38f)
        {
            armRigidbody = rb;
            if (armRigidbody != null) armRigidbody.isKinematic = true;
            tipTransform = tip;
            baseAngleY = angleY;
            targetAngle = Mathf.Max(MinAngle, initialAngle);
            currentAngle = targetAngle;
            UpdateRotation();
        }

        public void SetupHinge(HingeJoint hingeJoint, Rigidbody rb, Transform tip)
        {
            Setup(rb, tip, transform.localEulerAngles.y, 38f);
        }

        public void SetTargetAngle(float target, float springForce = 250f, float damper = 25f)
        {
            // 2° = touching/almost touching in center. Angles are never below 2° (no crossing).
            targetAngle = Mathf.Max(MinAngle, target);
            rotateSpeed = Mathf.Clamp(springForce * 0.75f, 100f, 280f);
        }

        public void SetAngleImmediate(float target)
        {
            targetAngle = Mathf.Max(MinAngle, target);
            currentAngle = targetAngle;
            UpdateRotation();
        }

        private void Update()
        {
            if (Mathf.Abs(currentAngle - targetAngle) > 0.01f)
            {
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
                currentAngle = Mathf.Max(MinAngle, currentAngle);
                UpdateRotation();
            }
        }

        private void UpdateRotation()
        {
            // 2° minimum logic angle prevents fingers from crossing
            float physicalZ = PhysicalZeroOffset + Mathf.Max(MinAngle, currentAngle);
            transform.localRotation = Quaternion.Euler(0f, baseAngleY, 0f) * Quaternion.Euler(0f, 0f, physicalZ);
        }
    }
}
