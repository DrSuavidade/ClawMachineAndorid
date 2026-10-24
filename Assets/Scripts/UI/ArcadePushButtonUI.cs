using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

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
        [SerializeField] private Color normalColor = new Color(0.95f, 0.24f, 0.24f);
        [SerializeField] private Color pressedColor = new Color(0.65f, 0.10f, 0.12f);
        [SerializeField] private Color disabledColor = new Color(0.32f, 0.32f, 0.36f);
        [SerializeField] private float plungeOffset = 10f;

        private Vector2 initialPlungerPos;
        private bool isInteractable = true;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindFirstObjectByType<MachineController>();
            }

            Image baseImg = GetComponent<Image>();
            if (baseImg != null)
            {
                baseImg.sprite = UIStyleHelper.ArcadeButtonBezelSprite;
                baseImg.color = Color.white;
            }

            if (plungerImage != null)
            {
                plungerImage.sprite = UIStyleHelper.ArcadeButtonCapSprite;
            }

            if (plungerTransform != null)
            {
                plungerTransform.anchoredPosition = Vector2.zero;
                initialPlungerPos = Vector2.zero;
            }

            if (labelText != null)
            {
                labelText.text = "DROP";
                labelText.fontSize = 24;
                labelText.fontStyle = FontStyle.Bold;
                labelText.color = Color.white;
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.gameObject.SetActive(true);
                UIStyleHelper.AddDropShadow(labelText, new Color(0f, 0f, 0f, 0.8f), new Vector2(1.5f, -2f));
            }
        }

        private void OnEnable()
        {
            if (machineController != null)
            {
                machineController.OnStateChanged += HandleStateChanged;
            }
            Theming.UIThemeManager.OnThemeChanged += HandleThemeChanged;
            if (Theming.UIThemeManager.Instance != null && Theming.UIThemeManager.Instance.CurrentTheme != null)
            {
                HandleThemeChanged(Theming.UIThemeManager.Instance.CurrentTheme);
            }
        }

        private void OnDisable()
        {
            if (machineController != null)
            {
                machineController.OnStateChanged -= HandleStateChanged;
            }
            Theming.UIThemeManager.OnThemeChanged -= HandleThemeChanged;
        }

        private void Update()
        {
            // Subtle arcade internal illumination glow when ready to drop/grab
            if (isInteractable && plungerImage != null && !ArcadeConsoleUI.IsAnyModalOpen())
            {
                float pulse = 0.94f + Mathf.Sin(Time.time * 3.5f) * 0.08f;
                plungerImage.color = new Color(normalColor.r * pulse, normalColor.g * pulse, normalColor.b * pulse, 1f);
            }
        }

        private void HandleThemeChanged(Data.CabinetTheme theme)
        {
            if (theme == null) return;
            normalColor = theme.accentColor;
            pressedColor = theme.accentColor * 0.72f;
            pressedColor.a = 1f;
            if (isInteractable && plungerImage != null)
            {
                plungerImage.color = normalColor;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable || ArcadeConsoleUI.IsAnyModalOpen()) return;

            if (plungerTransform != null)
            {
                plungerTransform.anchoredPosition = initialPlungerPos + new Vector2(0f, -plungeOffset);
                plungerTransform.localScale = new Vector3(0.95f, 0.95f, 1f);
            }
            if (plungerImage != null)
            {
                plungerImage.color = pressedColor;
            }

            ServiceLocator.Get<IAudioService>()?.PlayUIClick();
            ServiceLocator.Get<IAudioService>()?.TriggerHapticLight();
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
            if (!isInteractable || ArcadeConsoleUI.IsAnyModalOpen()) return;

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
        }

        public void SetThemeColor(Color themeColor)
        {
            normalColor = themeColor;
            pressedColor = themeColor * 0.72f;
            if (plungerImage != null && isInteractable)
            {
                plungerImage.color = normalColor;
            }
        }
    }
}
