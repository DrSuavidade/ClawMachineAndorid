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

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable) return;

            Debug.Log("[ArcadePushButtonUI] Plunger pressed -> DROP triggered!");
            if (machineController != null)
            {
                machineController.TriggerDrop();
            }
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
