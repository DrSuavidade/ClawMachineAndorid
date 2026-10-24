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

        [Header("Audio Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Text masterValueText;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Text musicValueText;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Text sfxValueText;

        [Header("Toggles & Buttons")]
        [SerializeField] private Button hapticsToggleButton;
        [SerializeField] private Text hapticsStatusText;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject resetConfirmPopup;

        // Legacy scene yaml compatibility
        [SerializeField, HideInInspector] private Button sfxToggleButton;
        [SerializeField, HideInInspector] private Text sfxStatusText;
        [SerializeField, HideInInspector] private Text resetText;

        private void Awake()
        {
            if (toggleButton != null) toggleButton.onClick.AddListener(OpenModal);
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);

            WireOrBuildSettings();

            if (modalPanel != null) modalPanel.SetActive(false);
            if (resetConfirmPopup != null) resetConfirmPopup.SetActive(false);
        }

        private void WireOrBuildSettings()
        {
            if (modalPanel == null) return;

            // Ensure standard modal shell with top-right close button
            RectTransform contentTr = UIStyleHelper.EnsureModalShell(modalPanel, "SETTINGS", CloseModal, 600f, 540f);

            // Hide old legacy Header and CloseButton from scene
            Transform oldHeader = modalPanel.transform.Find("Header");
            if (oldHeader != null) oldHeader.gameObject.SetActive(false);
            if (closeButton != null) closeButton.gameObject.SetActive(false);

            Transform listContainerTr = modalPanel.transform.Find("ListContainer");
            if (listContainerTr == null)
            {
                var lc = new GameObject("ListContainer", typeof(RectTransform));
                lc.transform.SetParent(contentTr, false);
                listContainerTr = lc.transform;
            }
            else
            {
                listContainerTr.SetParent(contentTr, false);
            }

            RectTransform lcRt = listContainerTr.GetComponent<RectTransform>();
            lcRt.anchorMin = Vector2.zero;
            lcRt.anchorMax = Vector2.one;
            lcRt.offsetMin = Vector2.zero;
            lcRt.offsetMax = Vector2.zero;

            // Remove legacy unconfigured empty rows from old scene yaml
            for (int i = listContainerTr.childCount - 1; i >= 0; i--)
            {
                Transform child = listContainerTr.GetChild(i);
                if (child.name == "Row_0" || child.name == "Row_1" || child.name == "Row_2")
                {
                    Destroy(child.gameObject);
                }
            }

            BuildFallbackLayout(listContainerTr);
        }

        private void BuildFallbackLayout(Transform listContainerTr)
        {
            VerticalLayoutGroup vlg = listContainerTr.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = listContainerTr.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 16f;
            vlg.padding = new RectOffset(16, 16, 12, 12);

            Font font = UIStyleHelper.MainFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            float curMaster = PlayerPrefs.GetFloat("Claw_Master_Volume", 1.0f);
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) curMaster = audio.MasterVolume;
            masterSlider = CreateVolumeRow(listContainerTr, "Row_MasterVolume", "MASTER VOLUME", font, curMaster, OnMasterVolumeChanged, out masterValueText);

            float curMusic = PlayerPrefs.GetFloat("Claw_Music_Volume", 0.75f);
            if (audio != null) curMusic = audio.MusicVolume;
            musicSlider = CreateVolumeRow(listContainerTr, "Row_MusicVolume", "GLOBAL MUSIC", font, curMusic, OnMusicVolumeChanged, out musicValueText);

            float curSfx = PlayerPrefs.GetFloat("Claw_SFX_Volume", 1.0f);
            if (audio != null) curSfx = audio.SFXVolume;
            sfxSlider = CreateVolumeRow(listContainerTr, "Row_SFXVolume", "SOUND EFFECTS", font, curSfx, OnSFXVolumeChanged, out sfxValueText);

            hapticsToggleButton = CreateActionRowButton(listContainerTr, "Row_Haptics", "VIBRATION: ON", font, new Color(0.18f, 0.22f, 0.32f, 1f), ToggleHaptics, out hapticsStatusText);

            Text dummy;
            resetButton = CreateActionRowButton(listContainerTr, "Row_ResetProgress", "RESET PROGRESS", font, new Color(0.75f, 0.18f, 0.20f, 1f), OpenResetConfirmPopup, out dummy);
        }

        private Slider CreateVolumeRow(Transform parent, string rowName, string label, Font font, float initialVal, System.Action<float> onChange, out Text valueLabel)
        {
            GameObject row = new GameObject(rowName, typeof(RectTransform), typeof(Image));
            row.transform.SetParent(parent, false);

            RectTransform rowRt = row.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(0, 68f);

            Image rowBg = row.GetComponent<Image>();
            rowBg.sprite = UIStyleHelper.RoundedCardSprite;
            rowBg.type = Image.Type.Sliced;
            rowBg.color = new Color(0.12f, 0.14f, 0.20f, 0.95f);

            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
            titleObj.transform.SetParent(row.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 0f);
            titleRt.anchorMax = new Vector2(0.46f, 1f);
            titleRt.offsetMin = new Vector2(18f, 0f);
            titleRt.offsetMax = Vector2.zero;

            Text titleTxt = titleObj.GetComponent<Text>();
            titleTxt.font = font;
            titleTxt.text = label;
            titleTxt.fontSize = 22;
            titleTxt.fontStyle = FontStyle.Bold;
            titleTxt.color = new Color(0.92f, 0.94f, 0.98f, 1f);
            titleTxt.alignment = TextAnchor.MiddleLeft;

            GameObject valObj = new GameObject("ValueText", typeof(RectTransform), typeof(Text));
            valObj.transform.SetParent(row.transform, false);
            RectTransform valRt = valObj.GetComponent<RectTransform>();
            valRt.anchorMin = new Vector2(0.84f, 0f);
            valRt.anchorMax = new Vector2(1f, 1f);
            valRt.offsetMin = Vector2.zero;
            valRt.offsetMax = new Vector2(-12f, 0f);

            Text valTxt = valObj.GetComponent<Text>();
            valTxt.font = font;
            valTxt.text = Mathf.RoundToInt(initialVal * 100f) + "%";
            valTxt.fontSize = 20;
            valTxt.fontStyle = FontStyle.Bold;
            valTxt.color = new Color(0.35f, 0.90f, 0.50f, 1f);
            valTxt.alignment = TextAnchor.MiddleRight;
            valueLabel = valTxt;

            Slider slider = UIStyleHelper.CreateStyledSlider(row.transform, "Slider", 0f, 1f, initialVal, onChange);
            RectTransform sRt = slider.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.46f, 0.5f);
            sRt.anchorMax = new Vector2(0.84f, 0.5f);
            sRt.sizeDelta = new Vector2(0, 24f);

            return slider;
        }

        private Button CreateActionRowButton(Transform parent, string rowName, string label, Font font, Color bgColor, UnityEngine.Events.UnityAction onClick, out Text statusText)
        {
            GameObject btnObj = new GameObject(rowName, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);

            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(0, 62f);

            Image img = btnObj.GetComponent<Image>();
            img.sprite = UIStyleHelper.PillButtonGlossSprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            Text txt = textObj.GetComponent<Text>();
            txt.font = font;
            txt.text = label;
            txt.fontSize = 24;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleCenter;
            UIStyleHelper.AddDropShadow(txt, new Color(0f, 0f, 0f, 0.75f), new Vector2(1f, -1.5f));
            statusText = txt;

            Button btn = btnObj.GetComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(onClick);
            UIStyleHelper.StyleButton(btn);
            img.color = bgColor;

            return btn;
        }

        public void OpenModal()
        {
            if (resetConfirmPopup != null) resetConfirmPopup.SetActive(false);
            if (modalPanel != null)
            {
                modalPanel.SetActive(true);
                Transform card = modalPanel.transform.Find("DialogCard") ?? modalPanel.transform;
                UIStyleHelper.AnimatePopIn(this, card);
            }
            ServiceLocator.Get<IAudioService>()?.PlayModalOpen();
            UpdateDisplay();
        }

        public void CloseModal()
        {
            if (resetConfirmPopup != null) resetConfirmPopup.SetActive(false);
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnMasterVolumeChanged(float val)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.MasterVolume = val;
            }
            else
            {
                PlayerPrefs.SetFloat("Claw_Master_Volume", val);
                PlayerPrefs.Save();
            }
            if (masterValueText != null) masterValueText.text = Mathf.RoundToInt(val * 100f) + "%";
        }

        private void OnMusicVolumeChanged(float val)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.MusicVolume = val;
            }
            else
            {
                PlayerPrefs.SetFloat("Claw_Music_Volume", val);
                PlayerPrefs.Save();
            }
            if (musicValueText != null) musicValueText.text = Mathf.RoundToInt(val * 100f) + "%";
        }

        private void OnSFXVolumeChanged(float val)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.SFXVolume = val;
            }
            else
            {
                PlayerPrefs.SetFloat("Claw_SFX_Volume", val);
                PlayerPrefs.Save();
            }
            if (sfxValueText != null) sfxValueText.text = Mathf.RoundToInt(val * 100f) + "%";
        }

        private void ToggleHaptics()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.IsHapticsEnabled = !audioService.IsHapticsEnabled;
                audioService.PlayUIClick();
            }
            else
            {
                bool current = PlayerPrefs.GetInt("Claw_Haptics_Enabled", 1) == 1;
                PlayerPrefs.SetInt("Claw_Haptics_Enabled", current ? 0 : 1);
                PlayerPrefs.Save();
            }
            UpdateDisplay();
        }

        private void OpenResetConfirmPopup()
        {
            EnsureResetConfirmPopup();
            if (resetConfirmPopup != null)
            {
                resetConfirmPopup.SetActive(true);
            }
        }

        private void ConfirmResetProgress()
        {
            if (resetConfirmPopup != null) resetConfirmPopup.SetActive(false);
            if (ServiceLocator.TryGet<ICollectionService>(out var collectionService))
            {
                collectionService.ResetProgress();
            }
            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                audioService.PlayErrorBuzz();
            }
            CloseModal();
        }

        private void EnsureResetConfirmPopup()
        {
            if (resetConfirmPopup != null || modalPanel == null) return;

            Transform existing = modalPanel.transform.Find("ResetConfirmPopup");
            if (existing != null)
            {
                resetConfirmPopup = existing.gameObject;
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            resetConfirmPopup = new GameObject("ResetConfirmPopup", typeof(RectTransform), typeof(Image));
            resetConfirmPopup.transform.SetParent(modalPanel.transform, false);
            RectTransform overlayRt = resetConfirmPopup.GetComponent<RectTransform>();
            overlayRt.anchorMin = Vector2.zero;
            overlayRt.anchorMax = Vector2.one;
            overlayRt.offsetMin = Vector2.zero;
            overlayRt.offsetMax = Vector2.zero;
            resetConfirmPopup.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.88f);

            GameObject card = new GameObject("DialogCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(resetConfirmPopup.transform, false);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(680f, 380f);
            card.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.22f, 1f);

            CreateLabel(card.transform, "Title", "RESET ALL PROGRESS?", font, 34, FontStyle.Bold,
                new Color(1f, 0.35f, 0.35f), new Vector2(0f, 110f), new Vector2(620f, 60f));

            CreateLabel(card.transform, "Body", "This will permanently delete all coins, unlocked machines, upgrades, and collected prizes.\n\nThis action cannot be undone!",
                font, 24, FontStyle.Normal, new Color(0.88f, 0.90f, 0.95f), new Vector2(0f, 10f), new Vector2(600f, 130f));

            Button cancelBtn = CreatePopupButton(card.transform, "Btn_Cancel", "CANCEL", font,
                new Color(0.28f, 0.32f, 0.42f), new Vector2(-150f, -115f), new Vector2(260f, 76f));
            cancelBtn.onClick.AddListener(() => resetConfirmPopup.SetActive(false));

            Button confirmBtn = CreatePopupButton(card.transform, "Btn_ConfirmReset", "YES, RESET", font,
                new Color(0.88f, 0.18f, 0.18f), new Vector2(150f, -115f), new Vector2(260f, 76f));
            confirmBtn.onClick.AddListener(ConfirmResetProgress);
        }

        private static Text CreateLabel(Transform parent, string name, string content, Font font, int fontSize, FontStyle style, Color color, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Text txt = go.GetComponent<Text>();
            txt.font = font;
            txt.text = content;
            txt.fontSize = fontSize;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            return txt;
        }

        private static Button CreatePopupButton(Transform parent, string name, string label, Font font, Color bgColor, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = bgColor;
            CreateLabel(go.transform, "Text", label, font, 26, FontStyle.Bold, Color.white, Vector2.zero, size);
            return go.GetComponent<Button>();
        }

        private void UpdateDisplay()
        {
            float mVol = 1f;
            float muVol = 0.75f;
            float sVol = 1f;
            bool hapticsOn = true;

            if (ServiceLocator.TryGet<IAudioService>(out var audioService))
            {
                mVol = audioService.MasterVolume;
                muVol = audioService.MusicVolume;
                sVol = audioService.SFXVolume;
                hapticsOn = audioService.IsHapticsEnabled;
            }
            else
            {
                mVol = PlayerPrefs.GetFloat("Claw_Master_Volume", 1f);
                muVol = PlayerPrefs.GetFloat("Claw_Music_Volume", 0.75f);
                sVol = PlayerPrefs.GetFloat("Claw_SFX_Volume", 1f);
                hapticsOn = PlayerPrefs.GetInt("Claw_Haptics_Enabled", 1) == 1;
            }

            if (masterSlider != null) masterSlider.value = mVol;
            if (masterValueText != null) masterValueText.text = Mathf.RoundToInt(mVol * 100f) + "%";

            if (musicSlider != null) musicSlider.value = muVol;
            if (musicValueText != null) musicValueText.text = Mathf.RoundToInt(muVol * 100f) + "%";

            if (sfxSlider != null) sfxSlider.value = sVol;
            if (sfxValueText != null) sfxValueText.text = Mathf.RoundToInt(sVol * 100f) + "%";

            if (hapticsStatusText != null)
            {
                hapticsStatusText.text = hapticsOn ? "VIBRATION: ON" : "VIBRATION: OFF";
                hapticsStatusText.color = hapticsOn ? new Color(0.3f, 0.9f, 0.45f) : new Color(0.7f, 0.7f, 0.75f);
            }
        }
    }
}
