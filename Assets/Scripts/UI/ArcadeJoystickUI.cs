using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Gameplay;

namespace ClawMachine.UI
{
    public class ArcadeJoystickUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Target Controller")]
        [SerializeField] private MachineController machineController;

        [Header("Skeuomorphic Components")]
        [SerializeField] private RectTransform knobTransform;
        [SerializeField] private Image outlineRing;
        [SerializeField] private Image ballKnobImage;

        [Header("Tuning")]
        [SerializeField] private float maxDeflectionRadius = 38f;
        [SerializeField] private float dragSensitivity = 0.022f;
        [SerializeField] private float returnSpeed = 16f;

        private bool isArmed = false;
        private bool isScreenDragging = false;
        private bool isDirectDragging = false;
        private Vector2 screenAnchorPos;
        private Vector2 currentInputVector;

        public bool IsArmed => isArmed;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindFirstObjectByType<MachineController>();
            }

            if (outlineRing != null)
            {
                outlineRing.gameObject.SetActive(false);
            }

            if (knobTransform != null)
            {
                knobTransform.anchoredPosition = Vector2.zero;
            }
        }

        private void OnEnable()
        {
            if (knobTransform != null)
            {
                knobTransform.anchoredPosition = Vector2.zero;
            }
        }

        private void Update()
        {
            // Keyboard visual feedback - use direct key checks to avoid phantom controller/joystick axis drift
            float h = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
            float v = 0f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
            Vector2 keyVec = new Vector2(h, v);

            if (keyVec.sqrMagnitude > 0.01f && !isDirectDragging && !isScreenDragging)
            {
                currentInputVector = keyVec.normalized;
                if (knobTransform != null)
                {
                    knobTransform.anchoredPosition = Vector2.Lerp(knobTransform.anchoredPosition, currentInputVector * maxDeflectionRadius, Time.deltaTime * returnSpeed);
                }
                return;
            }

            // Floating screen drag mode when armed
            if (isArmed && !isDirectDragging)
            {
                HandleScreenTouchMode();
            }

            // Return to center when not actively dragging
            if (!isDirectDragging && !isScreenDragging && knobTransform != null)
            {
                knobTransform.anchoredPosition = Vector2.Lerp(knobTransform.anchoredPosition, Vector2.zero, Time.deltaTime * returnSpeed);
            }

            // Outline pulse animation when armed
            if (isArmed && outlineRing != null)
            {
                float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 6f);
                outlineRing.color = new Color(0.2f, 0.95f, 1f, pulse);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Toggle armed state
            isArmed = !isArmed;
            if (outlineRing != null)
            {
                outlineRing.gameObject.SetActive(isArmed);
            }

            isScreenDragging = false;
            currentInputVector = Vector2.zero;
            if (knobTransform != null)
            {
                knobTransform.anchoredPosition = Vector2.zero;
            }

            Debug.Log($"[ArcadeJoystickUI] Joystick Armed: {isArmed}");
        }

        public void SetThemeColor(Color themeColor)
        {
            if (ballKnobImage != null)
            {
                ballKnobImage.color = themeColor;
            }
        }

        // Direct drag on joystick
        public void OnBeginDrag(PointerEventData eventData)
        {
            isDirectDragging = true;
            if (knobTransform != null) knobTransform.localScale = Vector3.one * 1.08f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
            {
                Vector2 clamped = Vector2.ClampMagnitude(localPos, maxDeflectionRadius);
                if (knobTransform != null) knobTransform.anchoredPosition = clamped;

                currentInputVector = clamped / maxDeflectionRadius;
                if (machineController != null)
                {
                    machineController.SetDragInput(currentInputVector * (dragSensitivity * 45f));
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDirectDragging = false;
            currentInputVector = Vector2.zero;
            if (knobTransform != null) knobTransform.localScale = Vector3.one;
        }

        // Floating screen center mode: click anywhere on screen to establish center
        private void HandleScreenTouchMode()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Check if pointer is over another interactive UI element (like Drop or Carousel or Joystick itself)
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    var hit = eventDataRaycast();
                    if (hit != null && (hit.name.Contains("Btn_") || hit.name.Contains("DropButton") || hit.name.Contains("ArcadePushButton") || hit.name.Contains("BallKnob") || hit.name.Contains("ArcadeJoystick")))
                    {
                        return; // Allow button and joystick clicks without triggering screen drag
                    }
                }

                screenAnchorPos = Input.mousePosition;
                isScreenDragging = true;
            }
            else if (Input.GetMouseButton(0) && isScreenDragging)
            {
                Vector2 currentPos = Input.mousePosition;
                Vector2 screenDelta = currentPos - screenAnchorPos;

                float maxScreenRadius = 120f;
                Vector2 clampedScreen = Vector2.ClampMagnitude(screenDelta, maxScreenRadius);
                Vector2 norm = clampedScreen / maxScreenRadius;

                // Deflect stick visual
                if (knobTransform != null)
                {
                    knobTransform.anchoredPosition = norm * maxDeflectionRadius;
                }

                // Send motion delta
                if (machineController != null && norm.sqrMagnitude > 0.001f)
                {
                    machineController.SetDragInput(norm * (dragSensitivity * 55f));
                }
            }
            else if (Input.GetMouseButtonUp(0) && isScreenDragging)
            {
                isScreenDragging = false;
                currentInputVector = Vector2.zero;
            }
        }

        private GameObject eventDataRaycast()
        {
            PointerEventData ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }
    }
}
