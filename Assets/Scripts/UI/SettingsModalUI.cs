using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

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

        private bool confirmReset = false;

        private void Awake()
        {
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
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.IsSFXEnabled = !audioService.IsSFXEnabled;
            }
            else
            {
                bool current = PlayerPrefs.GetInt("Claw_SFX_Enabled", 1) == 1;
                PlayerPrefs.SetInt("Claw_SFX_Enabled", current ? 0 : 1);
                PlayerPrefs.Save();
                AudioListener.volume = current ? 0f : 1f;
            }
            UpdateDisplay();
        }

        private void ToggleHaptics()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.IsHapticsEnabled = !audioService.IsHapticsEnabled;
            }
            else
            {
                bool current = PlayerPrefs.GetInt("Claw_Haptics_Enabled", 1) == 1;
                PlayerPrefs.SetInt("Claw_Haptics_Enabled", current ? 0 : 1);
                PlayerPrefs.Save();
            }
            UpdateDisplay();
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
                if (ServiceLocator.TryGet<ICollectionService>(out var collectionService))
                {
                    collectionService.ResetProgress();
                }
                else if (CollectionManager.Instance != null)
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
            bool sfxOn = true;
            bool hapticsOn = true;

            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                sfxOn = audioService.IsSFXEnabled;
                hapticsOn = audioService.IsHapticsEnabled;
            }
            else
            {
                sfxOn = PlayerPrefs.GetInt("Claw_SFX_Enabled", 1) == 1;
                hapticsOn = PlayerPrefs.GetInt("Claw_Haptics_Enabled", 1) == 1;
            }

            if (sfxStatusText != null)
            {
                sfxStatusText.text = sfxOn ? "SFX: ON" : "SFX: MUTED";
                sfxStatusText.color = sfxOn ? new Color(0.3f, 0.9f, 0.45f) : new Color(0.7f, 0.7f, 0.75f);
            }

            if (hapticsStatusText != null)
            {
                hapticsStatusText.text = hapticsOn ? "VIBRATION: ON" : "VIBRATION: OFF";
                hapticsStatusText.color = hapticsOn ? new Color(0.3f, 0.9f, 0.45f) : new Color(0.7f, 0.7f, 0.75f);
            }
        }
    }
}
