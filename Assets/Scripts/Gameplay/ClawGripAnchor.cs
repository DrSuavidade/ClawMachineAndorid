using UnityEngine;

namespace ClawMachine.Gameplay
{
    public struct GripEvaluation
    {
        public bool hasPrize;
        public Prize candidate;
        public float quality;      // 0.0 to 1.0 (aim accuracy)
        public bool isStable;      // quality >= stableThreshold -> makes it to chute
        public float slipDelay;    // seconds after lift starts before slipping (if unstable)
    }

    public class ClawGripAnchor : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform gripSocket;

        [Header("Sway Tuning")]
        [SerializeField] private float maxSwayAngle = 20f;
        [SerializeField] private float swaySmoothSpeed = 10f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGui = true;

        public Prize HeldPrize { get; private set; }
        public bool HasPrize => HeldPrize != null;
        public GripEvaluation CurrentGrip { get; private set; }
        public float SlipRisk => slipAccumulator;

        public event System.Action OnPrizeSlipped;

        private Transform originalParent;
        private Vector3 lastCarriagePos;
        private Vector3 carriageVelocity;
        private Quaternion targetSwayRotation = Quaternion.identity;
        private float holdTimer;
        private float slipAccumulator;

        private Vector3 startLocalPos;
        private Quaternion startLocalRot;
        private Vector3 targetSocketOffset;
        private float grabLerpTime = 1f;
        private const float GrabTransitionDuration = 0.35f;

        public void Initialize(Transform socket)
        {
            gripSocket = socket != null ? socket : transform;
            lastCarriagePos = transform.position;
        }

        public bool TryAcquire(GripEvaluation eval, Vector3 socketLocalOffset = default)
        {
            if (!eval.hasPrize || eval.candidate == null) return false;

            HeldPrize = eval.candidate;
            CurrentGrip = eval;
            holdTimer = 0f;
            slipAccumulator = 0f;

            Rigidbody rb = HeldPrize.Rigidbody;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;

            // Smooth grab transition: start at captured position, smoothly lift into basket seat
            originalParent = HeldPrize.transform.parent;
            HeldPrize.transform.SetParent(gripSocket, true);
            targetSocketOffset = (socketLocalOffset != Vector3.zero) ? socketLocalOffset : new Vector3(0f, -0.38f, 0f);
            startLocalPos = HeldPrize.transform.localPosition;
            startLocalRot = HeldPrize.transform.localRotation;
            grabLerpTime = 0f;

            // Ignore collisions between prize and claw assembly so kinematic hold doesn't glitch joints
            Collider[] prizeCols = HeldPrize.GetComponentsInChildren<Collider>();
            Collider[] clawCols = transform.root.GetComponentsInChildren<Collider>();
            for (int p = 0; p < prizeCols.Length; p++)
            {
                for (int c = 0; c < clawCols.Length; c++)
                {
                    if (prizeCols[p] != null && clawCols[c] != null && prizeCols[p] != clawCols[c])
                    {
                        Physics.IgnoreCollision(prizeCols[p], clawCols[c], true);
                    }
                }
            }

            Debug.Log($"[ClawGripAnchor] Secured {HeldPrize.name} in basket | Quality: {eval.quality:P0} | Stable: {eval.isStable}");
            return true;
        }

        public void UpdateMotion(float deltaTime)
        {
            if (gripSocket == null) return;

            Vector3 currentPos = transform.position;
            if (deltaTime > 0f)
            {
                carriageVelocity = (currentPos - lastCarriagePos) / deltaTime;
            }
            lastCarriagePos = currentPos;

            if (HeldPrize == null) return;

            holdTimer += deltaTime;

            // Speed & jerk based slip risk when moving
            float horizSpeed = new Vector2(carriageVelocity.x, carriageVelocity.z).magnitude;
            if (horizSpeed > 1.1f)
            {
                float riskRate = (horizSpeed - 0.9f) * (1.8f - CurrentGrip.quality);
                slipAccumulator += riskRate * deltaTime;
                if (slipAccumulator >= 1.0f)
                {
                    Debug.Log($"[ClawGripAnchor] Jolted too hard ({horizSpeed:F1}m/s)! {HeldPrize.name} slipped!");
                    Release();
                    OnPrizeSlipped?.Invoke();
                    return;
                }
            }
            else
            {
                slipAccumulator = Mathf.Max(0f, slipAccumulator - deltaTime * 0.4f);
            }

            if (grabLerpTime < GrabTransitionDuration)
            {
                grabLerpTime += deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, grabLerpTime / GrabTransitionDuration);
                HeldPrize.transform.localPosition = Vector3.Lerp(startLocalPos, targetSocketOffset, t);
                HeldPrize.transform.localRotation = Quaternion.Slerp(startLocalRot, Quaternion.identity, t);
            }
            else
            {
                // Sway in direction opposite to carriage movement
                Vector3 localVel = gripSocket.InverseTransformDirection(carriageVelocity);
                float pitch = Mathf.Clamp(-localVel.z * 5f, -maxSwayAngle, maxSwayAngle);
                float roll = Mathf.Clamp(localVel.x * 5f, -maxSwayAngle, maxSwayAngle);

                targetSwayRotation = Quaternion.Euler(pitch, 0f, roll);
                HeldPrize.transform.localPosition = targetSocketOffset;
                HeldPrize.transform.localRotation = Quaternion.Slerp(HeldPrize.transform.localRotation, targetSwayRotation, deltaTime * swaySmoothSpeed);
            }
        }

        public void Release()
        {
            if (HeldPrize == null) return;

            HeldPrize.transform.SetParent(originalParent, true);

            Rigidbody rb = HeldPrize.Rigidbody;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Restore collisions
            Collider[] prizeCols = HeldPrize.GetComponentsInChildren<Collider>();
            Collider[] clawCols = transform.root.GetComponentsInChildren<Collider>();
            for (int p = 0; p < prizeCols.Length; p++)
            {
                for (int c = 0; c < clawCols.Length; c++)
                {
                    if (prizeCols[p] != null && clawCols[c] != null && prizeCols[p] != clawCols[c])
                    {
                        Physics.IgnoreCollision(prizeCols[p], clawCols[c], false);
                    }
                }
            }

            // Gentle release: zero tumble spin, minimal horizontal carry momentum, gentle downward drop
            Vector3 horizVel = Vector3.ClampMagnitude(new Vector3(carriageVelocity.x, 0f, carriageVelocity.z) * 0.20f, 0.4f);
            rb.linearVelocity = horizVel + (Vector3.down * 0.25f);
            rb.angularVelocity = Vector3.zero;

            Debug.Log($"[ClawGripAnchor] Dropped {HeldPrize.name}");
            HeldPrize = null;
            CurrentGrip = default;
            grabLerpTime = 1f;
        }

        private void OnGUI()
        {
            if (!showDebugGui || HeldPrize == null) return;

            GUI.Box(new Rect(10, 10, 260, 95), "<b>CLAW GRIP</b>");
            GUI.Label(new Rect(20, 32, 240, 22), $"Held: <b>{HeldPrize.name}</b>");
            GUI.Label(new Rect(20, 52, 240, 22), $"Aim Quality: {CurrentGrip.quality:P0}");
            GUI.Label(new Rect(20, 72, 240, 22), $"Slip Risk: {slipAccumulator:P0}");
        }
    }
}
