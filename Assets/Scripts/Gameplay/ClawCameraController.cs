using UnityEngine;

namespace ClawMachine.Gameplay
{
    /// <summary>
    /// Controls dynamic cinematic framing, zoom, and focus based on ClawState.
    /// Builds tension during descent/grab and frames chute delivery during wins.
    /// </summary>
    public class ClawCameraController : MonoBehaviour
    {
        [Header("Default Framing")]
        [SerializeField] private Vector3 defaultPosition = new Vector3(0f, 5.0f, -7.0f);
        [SerializeField] private Vector3 defaultRotationEuler = new Vector3(28f, 0f, 0f);
        [SerializeField] private float defaultFov = 45f;

        [Header("Close-Up Grab Framing")]
        [SerializeField] private float grabFov = 34f;
        [SerializeField] private float grabHeight = 5.0f;
        [SerializeField] private float grabZDistance = -5.5f;

        [Header("Chute Drop Framing")]
        [SerializeField] private Vector3 chuteCameraOffset = new Vector3(-0.45f, 4.3f, -5.8f);

        [Header("Smoothing")]
        [SerializeField] private float positionSmoothSpeed = 4.5f;
        [SerializeField] private float rotationSmoothSpeed = 4.5f;
        [SerializeField] private float fovSmoothSpeed = 4.0f;

        [Header("References")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private ClawController clawController;
        [SerializeField] private Transform clawTarget;
        [SerializeField] private Transform chuteTarget;

        public Camera TargetCamera => targetCamera;

        private float basePitch = 28f;

        public void SetThemePreset(float fov, float pitchOffset = 0f)
        {
            defaultFov = fov;
            defaultRotationEuler.x = basePitch + pitchOffset;
            if (clawController == null || !IsGrabZoomState(clawController.CurrentState))
            {
                targetPos = defaultPosition;
                targetFovVal = defaultFov;
                targetRot = Quaternion.Euler(defaultRotationEuler);
            }
        }

        private Vector3 targetPos;
        private Quaternion targetRot;
        private float targetFovVal;

        // Camera Shake State
        private Vector3 shakeOffset;
        private float shakeTime;
        private float shakeMag;

        public void TriggerShake(float duration = 0.2f, float magnitude = 0.04f)
        {
            shakeTime = duration;
            shakeMag = magnitude;
        }

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>() ?? Camera.main;
            }
            if (clawController == null)
            {
                clawController = FindFirstObjectByType<ClawController>();
            }
            if (clawTarget == null && clawController != null)
            {
                clawTarget = clawController.GripSocket != null ? clawController.GripSocket : clawController.transform;
            }

            // Capture exact scene starting position & rotation as baseline default
            defaultPosition = transform.position;
            defaultRotationEuler = transform.eulerAngles;
            basePitch = defaultRotationEuler.x;
            if (targetCamera != null)
            {
                defaultFov = targetCamera.fieldOfView;
            }

            targetPos = defaultPosition;
            targetRot = Quaternion.Euler(defaultRotationEuler);
            targetFovVal = defaultFov;
        }

        private void OnEnable()
        {
            if (clawController != null)
            {
                clawController.OnStateChanged -= HandleClawStateChanged;
                clawController.OnStateChanged += HandleClawStateChanged;
            }
        }

        private void OnDisable()
        {
            if (clawController != null)
            {
                clawController.OnStateChanged -= HandleClawStateChanged;
            }
        }

        public void Initialize(ClawController claw, Transform clawSocket, Transform chute)
        {
            if (clawController != null)
            {
                clawController.OnStateChanged -= HandleClawStateChanged;
            }

            clawController = claw;
            clawTarget = clawSocket;
            chuteTarget = chute;

            if (clawController != null)
            {
                clawController.OnStateChanged += HandleClawStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (clawController != null)
            {
                clawController.OnStateChanged -= HandleClawStateChanged;
            }
        }

        private static bool IsGrabZoomState(ClawState state)
        {
            return state == ClawState.Descending
                || state == ClawState.Closing
                || state == ClawState.EvaluatingGrip
                || state == ClawState.Lifting;
        }

        private Coroutine hitStopRoutine;

        public void TriggerHitStop(float duration = 0.06f)
        {
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(CoHitStop(duration));
        }

        private System.Collections.IEnumerator CoHitStop(float duration)
        {
            float prevTimeScale = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = prevTimeScale;
            hitStopRoutine = null;
        }

        private void HandleClawStateChanged(ClawState state)
        {
            if (state == ClawState.Closing)
            {
                TriggerHitStop(0.06f);
            }

            if (IsGrabZoomState(state))
            {
                UpdateGrabFollowTarget();
                targetFovVal = grabFov;
            }
            else if (state == ClawState.Carrying)
            {
                UpdateCarryFollowTarget();
            }
            else
            {
                targetPos = defaultPosition;
                targetRot = Quaternion.Euler(defaultRotationEuler);
                targetFovVal = defaultFov;
            }
        }

        private void UpdateGrabFollowTarget()
        {
            float clawX = clawTarget != null ? clawTarget.position.x : 0f;
            float clawZ = clawTarget != null ? clawTarget.position.z : 0f;
            float clawY = clawTarget != null ? clawTarget.position.y : 2.0f;

            // Follow claw downward and horizontally to frame the prize being grabbed
            float dynamicY = Mathf.Clamp(clawY + 2.2f, 3.2f, grabHeight);
            targetPos = new Vector3(clawX * 0.45f, dynamicY, grabZDistance + clawZ * 0.25f);
            targetRot = Quaternion.Euler(27f, clawX * 3.5f, 0f);
        }

        private void UpdateCarryFollowTarget()
        {
            float clawX = clawTarget != null ? clawTarget.position.x : 0f;
            float clawZ = clawTarget != null ? clawTarget.position.z : 0f;
            targetPos = new Vector3(clawX * 0.35f, 4.6f, -6.0f + clawZ * 0.20f);
            targetRot = Quaternion.Euler(26f, clawX * 2.8f, 0f);
            targetFovVal = 38f;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            if (clawController != null)
            {
                if (IsGrabZoomState(clawController.CurrentState))
                {
                    UpdateGrabFollowTarget();
                    targetFovVal = grabFov;
                }
                else if (clawController.CurrentState == ClawState.Carrying)
                {
                    UpdateCarryFollowTarget();
                }
                else
                {
                    targetPos = defaultPosition;
                    targetRot = Quaternion.Euler(defaultRotationEuler);
                    targetFovVal = defaultFov;
                }
            }

            float dt = Time.deltaTime;
            // Smoothly move towards target framing
            transform.position = Vector3.Lerp(transform.position, targetPos, dt * positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * rotationSmoothSpeed);
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFovVal, dt * fovSmoothSpeed);

            // Handle Camera Shake additively without drift
            if (shakeTime > 0f)
            {
                shakeTime -= dt;
                Vector3 randOffset = Random.insideUnitSphere * shakeMag;
                randOffset.z = 0f;
                transform.position += randOffset;
            }
        }
    }
}
