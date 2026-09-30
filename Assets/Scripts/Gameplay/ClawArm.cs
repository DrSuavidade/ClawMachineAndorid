using UnityEngine;

namespace ClawMachine.Gameplay
{
    public class ClawArm : MonoBehaviour
    {
        [Header("Hinge Spring Setup")]
        [SerializeField] private HingeJoint hinge;
        [SerializeField] private Rigidbody armRigidbody;
        [SerializeField] private Transform tipTransform;

        public Transform Tip => tipTransform != null ? tipTransform : transform;
        public float CurrentAngle => hinge != null ? hinge.angle : 0f;

        private void Awake()
        {
            if (hinge == null) hinge = GetComponent<HingeJoint>();
            if (armRigidbody == null) armRigidbody = GetComponent<Rigidbody>();
        }

        public void SetupHinge(HingeJoint hingeJoint, Rigidbody rb, Transform tip)
        {
            hinge = hingeJoint;
            armRigidbody = rb;
            tipTransform = tip;
        }

        public void SetTargetAngle(float targetAngle, float springForce = 250f, float damper = 25f)
        {
            if (hinge == null) return;

            JointSpring spr = hinge.spring;
            spr.targetPosition = targetAngle;
            spr.spring = springForce;
            spr.damper = damper;
            hinge.spring = spr;
            hinge.useSpring = true;
        }

        public void SetAngleImmediate(float targetAngle)
        {
            SetTargetAngle(targetAngle, 300f, 30f);
        }
    }
}
