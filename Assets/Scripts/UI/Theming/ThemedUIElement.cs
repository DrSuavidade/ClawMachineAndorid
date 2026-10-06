using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Data;

namespace ClawMachine.UI.Theming
{
    public enum ThemeTargetType
    {
        Primary,
        Secondary,
        Accent,
        Surface,
        HeaderText,
        BodyText,
        FrameBorder,
        ButtonBackground,
        RivetBulb
    }

    [DisallowMultipleComponent]
    public class ThemedUIElement : MonoBehaviour
    {
        [Header("Theme Binding")]
        [SerializeField] private ThemeTargetType targetType = ThemeTargetType.Primary;
        [SerializeField] private bool overrideAlpha = false;
        [SerializeField] [Range(0f, 1f)] private float customAlpha = 1f;

        private Graphic graphicComponent;
        private Image imageComponent;
        private Text textComponent;

        public ThemeTargetType TargetType
        {
            get => targetType;
            set
            {
                targetType = value;
                ApplyCurrentTheme();
            }
        }

        private void Awake()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            CacheComponents();
            UIThemeManager.OnThemeChanged += HandleThemeChanged;
            ApplyCurrentTheme();
        }

        private void OnDisable()
        {
            UIThemeManager.OnThemeChanged -= HandleThemeChanged;
        }

        private void CacheComponents()
        {
            if (graphicComponent == null) graphicComponent = GetComponent<Graphic>();
            if (imageComponent == null) imageComponent = GetComponent<Image>();
            if (textComponent == null) textComponent = GetComponent<Text>();
        }

        private void HandleThemeChanged(CabinetTheme theme)
        {
            ApplyTheme(theme);
        }

        public void ApplyCurrentTheme()
        {
            if (UIThemeManager.Instance != null && UIThemeManager.Instance.CurrentTheme != null)
            {
                ApplyTheme(UIThemeManager.Instance.CurrentTheme);
            }
        }

        public void ApplyTheme(CabinetTheme theme)
        {
            if (theme == null) return;
            CacheComponents();

            Color targetColor = Color.white;
            bool applyColor = true;

            switch (targetType)
            {
                case ThemeTargetType.Primary:
                    targetColor = theme.primaryColor;
                    break;
                case ThemeTargetType.Secondary:
                    targetColor = theme.secondaryColor;
                    break;
                case ThemeTargetType.Accent:
                    targetColor = theme.accentColor;
                    break;
                case ThemeTargetType.Surface:
                    targetColor = theme.surfaceColor;
                    break;
                case ThemeTargetType.HeaderText:
                    targetColor = theme.textColorHeader;
                    break;
                case ThemeTargetType.BodyText:
                    targetColor = theme.textColorBody;
                    break;
                case ThemeTargetType.FrameBorder:
                    if (imageComponent != null && theme.frameBorder != null)
                    {
                        imageComponent.sprite = theme.frameBorder;
                    }
                    targetColor = theme.primaryColor;
                    break;
                case ThemeTargetType.ButtonBackground:
                    if (imageComponent != null && theme.buttonBackground != null)
                    {
                        imageComponent.sprite = theme.buttonBackground;
                    }
                    targetColor = theme.accentColor;
                    break;
                case ThemeTargetType.RivetBulb:
                    if (imageComponent != null && theme.rivetBulb != null)
                    {
                        imageComponent.sprite = theme.rivetBulb;
                    }
                    targetColor = theme.accentColor;
                    break;
            }

            if (applyColor && graphicComponent != null)
            {
                if (overrideAlpha)
                {
                    targetColor.a = customAlpha;
                }
                graphicComponent.color = targetColor;
            }
        }
    }
}
