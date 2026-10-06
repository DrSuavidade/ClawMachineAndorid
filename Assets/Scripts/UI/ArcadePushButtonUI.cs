using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Gameplay;

namespace ClawMachine.UI
{
    public class ArcadePushButtonUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [Header("Target Controller")]
        [SerializeField] private MachineController machineController;

        [Header("Skeuomorphic Visuals")]
        [SerializeField] private RectTransform plungerTransform;
        [SerializeField] private Image plungerImage;
        [SerializeField] private Text labelText;

        [Header("Colors & Animation")]
        [SerializeField] private Color normalColor = new Color(0.92f, 0.22f, 0.22f);
        [SerializeField] private Color pressedColor = new Color(0.68f, 0.12f, 0.12f);
        [SerializeField] private Color disabledColor = new Color(0.35f, 0.35f, 0.38f);
        [SerializeField] private float plungeOffset = 7f;

        private Vector2 initialPlungerPos;
        private bool isInteractable = true;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindFirstObjectByType<MachineController>();
            }

            if (plungerTransform != null)
            {
                plungerTransform.anchoredPosition = Vector2.zero;
                initialPlungerPos = Vector2.zero;
            }
        }

        private void OnEnable()
        {
            if (machineController != null)
            {
                machineController.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (machineController != null)
            {
                machineController.OnStateChanged -= HandleStateChanged;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable) return;
            if (plungerTransform != null)
            {
                plungerTransform.anchoredPosition = initialPlungerPos + new Vector2(0f, -plungeOffset);
                plungerTransform.localScale = Vector3.one * 0.95f;
            }
            if (plungerImage != null)
            {
                plungerImage.color = pressedColor;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (plungerTransform != null)
            {
                plungerTransform.anchoredPosition = initialPlungerPos;
                plungerTransform.localScale = Vector3.one;
            }
            if (plungerImage != null)
            {
                plungerImage.color = isInteractable ? normalColor : disabledColor;
            }
        }

        private Coroutine punchRoutine;

        public void SetThemeColor(Color themeColor)
        {
            normalColor = themeColor;
            pressedColor = themeColor * 0.72f;
            if (plungerImage != null && isInteractable)
            {
                plungerImage.color = normalColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;

            if (punchRoutine != null) StopCoroutine(punchRoutine);
            punchRoutine = StartCoroutine(PunchRoutine());

            if (machineController != null)
            {
                machineController.TriggerDrop();
            }
        }

        private System.Collections.IEnumerator PunchRoutine()
        {
            if (plungerTransform == null) yield break;
            float elapsed = 0f;
            float duration = 0.22f;
            Vector3 startScale = Vector3.one * 0.92f;
            Vector3 peakScale = Vector3.one * 1.08f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                plungerTransform.localScale = (t < 0.45f)
                    ? Vector3.Lerp(startScale, peakScale, t / 0.45f)
                    : Vector3.Lerp(peakScale, Vector3.one, (t - 0.45f) / 0.55f);
                yield return null;
            }
            plungerTransform.localScale = Vector3.one;
            punchRoutine = null;
        }

        private void HandleStateChanged(ClawState state)
        {
            isInteractable = (state == ClawState.Aiming || state == ClawState.Carrying);

            if (plungerImage != null)
            {
                plungerImage.color = isInteractable ? normalColor : disabledColor;
            }

            if (labelText != null)
            {
                if (state == ClawState.Carrying)
                {
                    labelText.text = "DROP PRIZE";
                }
                else if (state == ClawState.Aiming)
                {
                    labelText.text = "DROP";
                }
                else
                {
                    labelText.text = "...";
                }
            }
        }
    }
}
