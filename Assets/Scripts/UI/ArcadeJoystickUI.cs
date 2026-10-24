using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class ArcadeJoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Target Controller")]
        [SerializeField] private MachineController machineController;

        [Header("Skeuomorphic Components")]
        [SerializeField] private RectTransform knobTransform;
        [SerializeField] private Image outlineRing;
        [SerializeField] private Image ballKnobImage;
        [SerializeField] private RectTransform shaftTransform;
        [SerializeField] private Image shaftImage;
        [SerializeField] private RectTransform shadowTransform;
        [SerializeField] private Image shadowImage;

        [Header("Tuning")]
        [SerializeField] private float maxDeflectionRadius = 52f;
        [SerializeField] private float dragSensitivity = 0.024f;

        private const float SpringK = 380f;
        private const float Damping = 24f;

        private bool isArmed = false;
        private bool isScreenDragging = false;
        private bool isDirectDragging = false;
        private Vector2 screenAnchorPos;

        private Vector2 currentPos = Vector2.zero;
        private Vector2 springVelocity = Vector2.zero;
        private Vector2 currentInputVector = Vector2.zero;

        public bool IsArmed => isArmed;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindFirstObjectByType<MachineController>();
            }

            Image baseImg = GetComponent<Image>();
            if (baseImg != null)
            {
                baseImg.sprite = UIStyleHelper.ArcadeJoystickBaseSprite;
                baseImg.color = Color.white;
            }

            if (ballKnobImage != null)
            {
                ballKnobImage.sprite = UIStyleHelper.ArcadeJoystickBallSprite;
                ballKnobImage.color = Color.white;
            }

            EnsureMechanicalParts();

            if (outlineRing != null)
            {
                outlineRing.gameObject.SetActive(false);
            }

            ResetToCenter();
        }

        private void EnsureMechanicalParts()
        {
            if (shadowTransform == null)
            {
                Transform existing = transform.Find("KnobShadow");
                if (existing != null)
                {
                    shadowTransform = existing.GetComponent<RectTransform>();
                    shadowImage = existing.GetComponent<Image>();
                }
            }
            if (shadowImage != null)
            {
                shadowImage.sprite = UIStyleHelper.ArcadeKnobShadowSprite;
                shadowImage.color = Color.white;
                shadowImage.raycastTarget = false;
            }

            if (shaftTransform == null)
            {
                Transform existing = transform.Find("JoystickShaft");
                if (existing != null)
                {
                    shaftTransform = existing.GetComponent<RectTransform>();
                    shaftImage = existing.GetComponent<Image>();
                }
            }
            if (shaftImage != null)
            {
                shaftImage.sprite = UIStyleHelper.ArcadeJoystickShaftSprite;
                shaftImage.color = Color.white;
                shaftImage.raycastTarget = false;
            }

            Transform spec = (knobTransform != null ? knobTransform.Find("SpecularDot") : null) ?? transform.Find("BallKnob/SpecularDot");
            if (spec != null) spec.gameObject.SetActive(false);

            Transform collar = transform.Find("CollarBase");
            if (collar != null) collar.gameObject.SetActive(false);

            if (shadowTransform != null) shadowTransform.SetSiblingIndex(1);
            if (shaftTransform != null) shaftTransform.SetSiblingIndex(2);
            if (knobTransform != null) knobTransform.SetAsLastSibling();
        }

        private void OnEnable()
        {
            ResetToCenter();
        }

        private void ResetToCenter()
        {
            currentPos = Vector2.zero;
            springVelocity = Vector2.zero;
            currentInputVector = Vector2.zero;
            isDirectDragging = false;
            isScreenDragging = false;
            if (knobTransform != null) knobTransform.anchoredPosition = Vector2.zero;
            UpdateMechanicalVisuals(Vector2.zero);
        }

        private void Update()
        {
            if (ArcadeConsoleUI.IsAnyModalOpen())
            {
                if (isDirectDragging || isScreenDragging)
                {
                    isDirectDragging = false;
                    isScreenDragging = false;
                }
                ResetToCenter();
                return;
            }

            // Keyboard input (WASD / Arrows)
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
                currentPos = Vector2.Lerp(currentPos, currentInputVector * maxDeflectionRadius, Time.deltaTime * 18f);
                springVelocity = Vector2.zero;

                if (knobTransform != null) knobTransform.anchoredPosition = currentPos;
                UpdateMechanicalVisuals(currentPos);

                if (machineController != null)
                {
                    machineController.SetDragInput(currentInputVector * (dragSensitivity * 45f));
                }
                return;
            }

            // Floating screen touch drag mode when armed
            if (isArmed && !isDirectDragging)
            {
                HandleScreenTouchMode();
            }
            else if (!isArmed && !isDirectDragging)
            {
                HandleUnarmedDoubleTapNudge();
            }

            // Outline pulse animation when armed
            if (outlineRing != null)
            {
                if (isArmed)
                {
                    if (!outlineRing.gameObject.activeSelf) outlineRing.gameObject.SetActive(true);
                    float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * 6f);
                    outlineRing.color = new Color(0.2f, 0.95f, 1f, pulse);
                }
                else if (outlineRing.gameObject.activeSelf)
                {
                    outlineRing.gameObject.SetActive(false);
                }
            }

            // Direct dragging continuous input
            if (isDirectDragging)
            {
                if (currentInputVector.sqrMagnitude > 0.001f && machineController != null)
                {
                    machineController.SetDragInput(currentInputVector * (dragSensitivity * 45f));
                }
                return;
            }

            // If screen dragging, visual is driven by HandleScreenTouchMode
            if (isScreenDragging) return;

            // Mechanical Spring Harmonic Snap-Back
            if (currentPos.sqrMagnitude > 0.01f)
            {
                float dt = Mathf.Min(Time.deltaTime, 0.033f);
                Vector2 springForce = -currentPos * SpringK;
                Vector2 dampingForce = -springVelocity * Damping;
                springVelocity += (springForce + dampingForce) * dt;
                currentPos += springVelocity * dt;

                if (currentPos.magnitude < 0.6f && springVelocity.magnitude < 1.5f)
                {
                    currentPos = Vector2.zero;
                    springVelocity = Vector2.zero;
                    currentInputVector = Vector2.zero;
                }

                if (knobTransform != null) knobTransform.anchoredPosition = currentPos;
                UpdateMechanicalVisuals(currentPos);
            }
        }

        private void UpdateMechanicalVisuals(Vector2 deflection)
        {
            float len = deflection.magnitude;

            if (shaftTransform != null)
            {
                if (len > 3f)
                {
                    if (!shaftTransform.gameObject.activeSelf) shaftTransform.gameObject.SetActive(true);
                    shaftTransform.anchoredPosition = Vector2.zero;
                    shaftTransform.sizeDelta = new Vector2(22f, len);
                    float angle = Mathf.Atan2(deflection.y, deflection.x) * Mathf.Rad2Deg - 90f;
                    shaftTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                }
                else
                {
                    if (shaftTransform.gameObject.activeSelf) shaftTransform.gameObject.SetActive(false);
                }
            }

            if (shadowTransform != null)
            {
                shadowTransform.anchoredPosition = deflection * 0.42f + new Vector2(4f, -7f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Toggle armed state when clicking joystick
            if (eventData.dragging) return;

            isArmed = !isArmed;
            isScreenDragging = false;
            currentInputVector = Vector2.zero;

            var audioSvc = ServiceLocator.Get<IAudioService>();
            if (audioSvc != null)
            {
                audioSvc.PlayUIClick();
                audioSvc.TriggerHapticLight();
            }

            Debug.Log($"[ArcadeJoystickUI] Joystick Armed: {isArmed}");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (ArcadeConsoleUI.IsAnyModalOpen()) return;
            isDirectDragging = true;
            springVelocity = Vector2.zero;

            ServiceLocator.Get<IAudioService>()?.TriggerHapticLight();
            UpdatePointerDeflection(eventData);
            if (knobTransform != null) knobTransform.localScale = Vector3.one * 1.06f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDirectDragging || ArcadeConsoleUI.IsAnyModalOpen()) return;
            UpdatePointerDeflection(eventData);
        }

        private void UpdatePointerDeflection(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPos))
            {
                currentPos = Vector2.ClampMagnitude(localPos, maxDeflectionRadius);
                if (knobTransform != null) knobTransform.anchoredPosition = currentPos;

                currentInputVector = currentPos / maxDeflectionRadius;
                UpdateMechanicalVisuals(currentPos);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isDirectDragging) return;
            isDirectDragging = false;
            currentInputVector = Vector2.zero;
            if (knobTransform != null)
            {
                if (currentPos.sqrMagnitude > 25f)
                {
                    UIStyleHelper.AnimateScalePunch(this, knobTransform, 0.88f, 0.20f);
                }
                else
                {
                    knobTransform.localScale = Vector3.one;
                }
            }
            ServiceLocator.Get<IAudioService>()?.TriggerHapticLight();
        }

        private void HandleScreenTouchMode()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    var hit = RaycastUIUnderPointer();
                    if (hit != null && (hit.name.Contains("Btn_") || hit.name.Contains("DropButton") || 
                                       hit.name.Contains("ArcadePushButton") || hit.name.Contains("BallKnob") || 
                                       hit.name.Contains("ArcadeJoystick") || hit.name.Contains("Modal")))
                    {
                        return;
                    }
                }

                screenAnchorPos = Input.mousePosition;
                isScreenDragging = true;
            }
            else if (Input.GetMouseButton(0) && isScreenDragging)
            {
                Vector2 currentScreen = Input.mousePosition;
                Vector2 screenDelta = currentScreen - screenAnchorPos;

                float maxScreenRadius = 120f;
                Vector2 clampedScreen = Vector2.ClampMagnitude(screenDelta, maxScreenRadius);
                Vector2 norm = clampedScreen / maxScreenRadius;

                currentPos = norm * maxDeflectionRadius;
                if (knobTransform != null) knobTransform.anchoredPosition = currentPos;
                UpdateMechanicalVisuals(currentPos);

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

        private float lastTapTime = -1f;
        private Vector2 lastTapPos;

        private void HandleUnarmedDoubleTapNudge()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    var hit = RaycastUIUnderPointer();
                    if (hit != null && (hit.name.Contains("Btn_") || hit.name.Contains("DropButton") || 
                                       hit.name.Contains("ArcadePushButton") || hit.name.Contains("BallKnob") || 
                                       hit.name.Contains("ArcadeJoystick") || hit.name.Contains("Modal") ||
                                       hit.name.Contains("Card_")))
                    {
                        return;
                    }
                }

                float now = Time.unscaledTime;
                Vector2 currentTapPos = Input.mousePosition;
                if (now - lastTapTime < 0.32f && Vector2.Distance(currentTapPos, lastTapPos) < 70f)
                {
                    if (machineController != null)
                    {
                        machineController.TriggerMachineNudge(currentTapPos);
                    }
                    lastTapTime = -1f;
                }
                else
                {
                    lastTapTime = now;
                    lastTapPos = currentTapPos;
                }
            }
        }

        private GameObject RaycastUIUnderPointer()
        {
            PointerEventData ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results.Count > 0 ? results[0].gameObject : null;
        }

        public void SetThemeColor(Color themeColor)
        {
            if (ballKnobImage != null)
            {
                ballKnobImage.color = themeColor;
            }
        }
    }
}
