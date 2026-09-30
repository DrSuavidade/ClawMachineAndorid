using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;

namespace ClawMachine.UI
{
    public class SettingsModalUI : MonoBehaviour
    {
        [Header("Modal & Toggle")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;

        [Header("Toggles")]
        [SerializeField] private Button sfxToggleButton;
        [SerializeField] private Text sfxStatusText;

        [SerializeField] private Button hapticsToggleButton;
        [SerializeField] private Text hapticsStatusText;

        [Header("Reset Data")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Text resetText;

        private bool sfxEnabled = true;
        private bool hapticsEnabled = true;
        private bool confirmReset = false;

        private const string PREF_SFX = "Claw_SFX_Enabled";
        private const string PREF_HAPTICS = "Claw_Haptics_Enabled";

        private void Awake()
        {
            sfxEnabled = PlayerPrefs.GetInt(PREF_SFX, 1) == 1;
            hapticsEnabled = PlayerPrefs.GetInt(PREF_HAPTICS, 1) == 1;

            ApplyAudioSettings();

            if (toggleButton != null) toggleButton.onClick.AddListener(OpenModal);
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);

            if (sfxToggleButton != null) sfxToggleButton.onClick.AddListener(ToggleSFX);
            if (hapticsToggleButton != null) hapticsToggleButton.onClick.AddListener(ToggleHaptics);
            if (resetButton != null) resetButton.onClick.AddListener(HandleResetClicked);

            if (modalPanel != null) modalPanel.SetActive(false);
        }

        public void OpenModal()
        {
            confirmReset = false;
            if (resetText != null) resetText.text = "RESET PROGRESS";
            if (modalPanel != null) modalPanel.SetActive(true);
            UpdateDisplay();
        }

        public void CloseModal()
        {
            confirmReset = false;
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void ToggleSFX()
        {
            sfxEnabled = !sfxEnabled;
            PlayerPrefs.SetInt(PREF_SFX, sfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyAudioSettings();
            UpdateDisplay();
        }

        private void ToggleHaptics()
        {
            hapticsEnabled = !hapticsEnabled;
            PlayerPrefs.SetInt(PREF_HAPTICS, hapticsEnabled ? 1 : 0);
            PlayerPrefs.Save();
            UpdateDisplay();
        }

        private void ApplyAudioSettings()
        {
            AudioListener.volume = sfxEnabled ? 1.0f : 0.0f;
        }

        private void HandleResetClicked()
        {
            if (!confirmReset)
            {
                confirmReset = true;
                if (resetText != null) resetText.text = "CONFIRM RESET?";
                var img = resetButton != null ? resetButton.GetComponent<Image>() : null;
                if (img != null) img.color = new Color(0.95f, 0.15f, 0.15f);
            }
            else
            {
                confirmReset = false;
                if (CollectionManager.Instance != null)
                {
                    CollectionManager.Instance.ResetProgress();
                }
                if (resetText != null) resetText.text = "RESET COMPLETE!";
                var img = resetButton != null ? resetButton.GetComponent<Image>() : null;
                if (img != null) img.color = new Color(0.35f, 0.35f, 0.40f);
            }
        }

        private void UpdateDisplay()
        {
            if (sfxStatusText != null)
            {
                sfxStatusText.text = sfxEnabled ? "SFX: ON" : "SFX: MUTED";
                sfxStatusText.color = sfxEnabled ? new Color(0.3f, 0.9f, 0.45f) : new Color(0.7f, 0.7f, 0.75f);
            }

            if (hapticsStatusText != null)
            {
                hapticsStatusText.text = hapticsEnabled ? "VIBRATION: ON" : "VIBRATION: OFF";
                hapticsStatusText.color = hapticsEnabled ? new Color(0.3f, 0.9f, 0.45f) : new Color(0.7f, 0.7f, 0.75f);
            }
        }
    }
}
