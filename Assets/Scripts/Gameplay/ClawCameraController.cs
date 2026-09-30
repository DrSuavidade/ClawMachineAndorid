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
        [SerializeField] private float grabHeight = 4.1f;
        [SerializeField] private float grabZDistance = -5.5f;

        [Header("Chute Drop Framing")]
        [SerializeField] private float chuteFov = 38f;
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

        private Vector3 targetPos;
        private Quaternion targetRot;
        private float targetFovVal;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>() ?? Camera.main;
            }
            targetPos = defaultPosition;
            targetRot = Quaternion.Euler(defaultRotationEuler);
            targetFovVal = defaultFov;
        }

        public void Initialize(ClawController claw, Transform clawSocket, Transform chute)
        {
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

        private void HandleClawStateChanged(ClawState state)
        {
            switch (state)
            {
                case ClawState.Aiming:
                    targetPos = defaultPosition;
                    targetRot = Quaternion.Euler(defaultRotationEuler);
                    targetFovVal = defaultFov;
                    break;

                case ClawState.Descending:
                case ClawState.Closing:
                case ClawState.EvaluatingGrip:
                    // Zoom in on target prize zone under claw
                    float clawX = clawTarget != null ? clawTarget.position.x : 0f;
                    float clawZ = clawTarget != null ? clawTarget.position.z : 0f;
                    targetPos = new Vector3(clawX * 0.40f, grabHeight, grabZDistance + clawZ * 0.25f);
                    targetRot = Quaternion.Euler(27f, clawX * 3.5f, 0f);
                    targetFovVal = grabFov;
                    break;

                case ClawState.Lifting:
                case ClawState.Carrying:
                    // Follow claw transit with medium framing
                    float liftX = clawTarget != null ? clawTarget.position.x : 0f;
                    float liftY = clawTarget != null ? Mathf.Max(3.8f, clawTarget.position.y + 1.2f) : 4.8f;
                    targetPos = new Vector3(liftX * 0.30f, liftY, -6.3f);
                    targetRot = Quaternion.Euler(26f, liftX * 2.5f, 0f);
                    targetFovVal = 40f;
                    break;

                case ClawState.Returning:
                case ClawState.Releasing:
                    // Focus on chute drop zone
                    targetPos = chuteCameraOffset;
                    targetRot = Quaternion.Euler(25f, -6f, 0f);
                    targetFovVal = chuteFov;
                    break;

                case ClawState.Resolving:
                    // Return back to default
                    targetPos = defaultPosition;
                    targetRot = Quaternion.Euler(defaultRotationEuler);
                    targetFovVal = defaultFov;
                    break;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;

            float dt = Time.deltaTime;
            // Smoothly move towards target framing
            transform.position = Vector3.Lerp(transform.position, targetPos, dt * positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * rotationSmoothSpeed);
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFovVal, dt * fovSmoothSpeed);
        }
    }
}
