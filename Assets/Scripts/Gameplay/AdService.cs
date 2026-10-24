using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Core.Services;
using ClawMachine.UI;

namespace ClawMachine.Gameplay
{
    /// <summary>
    /// Handles Rewarded Video Ads and the "Out of Coins / Watch Ad" prompt.
    /// Ready for Unity LevelPlay (Unity Ads) or Google AdMob SDK integration.
    /// </summary>
    public class AdService : MonoBehaviour, IAdService
    {
        private static AdService instance;
        public static AdService Instance => instance;

        [Header("Reward Tuning")]
        [SerializeField] private int minCoinReward = 50;
        [SerializeField] private int maxCoinReward = 100;
        [SerializeField] private float simulatedAdDuration = 3.0f;

        private GameObject promptModal;
        private GameObject adPlaybackOverlay;
        private Text adCountdownText;
        private Text promptBodyText;
        private Text watchBtnText;
        private bool isShowingAd;

        public bool IsAdReady => !isShowingAd;

        private void GetScaledRewardRange(out int min, out int max)
        {
            int entry = 5;
            if (ServiceLocator.TryGet<ICollectionService>(out var col) && col.CurrentMachine != null)
            {
                entry = Mathf.Max(5, col.CurrentMachine.entryCost);
            }
            min = Mathf.Max(minCoinReward, entry * 4);
            max = Mathf.Max(maxCoinReward, entry * 8);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            ServiceLocator.Register<IAdService>(this);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                ServiceLocator.Unregister<IAdService>();
                instance = null;
            }
        }

        public void ShowOutOfCoinsPrompt()
        {
            EnsureUI();
            if (promptModal != null)
            {
                GetScaledRewardRange(out int min, out int max);
                if (promptBodyText != null)
                {
                    promptBodyText.text = $"Watch a short video to instantly receive {min}–{max} FREE COINS!";
                }
                if (watchBtnText != null)
                {
                    watchBtnText.text = $"WATCH AD (+{min}-{max} COINS)";
                }
                promptModal.SetActive(true);
                Transform card = promptModal.transform.Find("DialogCard") ?? promptModal.transform;
                ClawMachine.UI.UIStyleHelper.AnimatePopIn(this, card);
                ServiceLocator.Get<IAudioService>()?.PlayModalOpen();
            }
        }

        public void ShowRewardedAd(Action<int> onRewarded, Action onFailed = null)
        {
            if (isShowingAd)
            {
                onFailed?.Invoke();
                return;
            }

            if (promptModal != null) promptModal.SetActive(false);

            // NOTE: When integrating Unity Ads (Advertisement.Show) or AdMob (rewardedAd.Show),
            // replace this coroutine with the SDK's Show call and invoke AwardAdCoins in OnUnityAdsShowComplete.
            StartCoroutine(SimulatedRewardedAdRoutine(onRewarded));
        }

        private IEnumerator SimulatedRewardedAdRoutine(Action<int> onRewarded)
        {
            isShowingAd = true;
            EnsureUI();

            if (adPlaybackOverlay != null) adPlaybackOverlay.SetActive(true);

            float remaining = simulatedAdDuration;
            while (remaining > 0f)
            {
                if (adCountdownText != null)
                {
                    adCountdownText.text = $"SPONSORED VIDEO AD\n\nReward in {Mathf.CeilToInt(remaining)}s...";
                }
                remaining -= Time.unscaledDeltaTime;
                yield return null;
            }

            if (adPlaybackOverlay != null) adPlaybackOverlay.SetActive(false);
            isShowingAd = false;

            GetScaledRewardRange(out int min, out int max);
            int reward = UnityEngine.Random.Range(min, max + 1);
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.AwardCoins(reward);
            }
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                audio.PlayCoinReward();
            }
            ClawJuiceEffects.Instance?.PlayWinCelebration(Vector3.up * 1.5f);
            UIStyleHelper.SpawnFloatingCoinReward(this, reward, new Vector2(0f, 100f));

            Debug.Log($"[AdService] Rewarded ad completed! Awarded +{reward} coins.");
            onRewarded?.Invoke(reward);
        }

        private void EnsureUI()
        {
            if (promptModal != null && adPlaybackOverlay != null) return;

            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform parent = canvas.transform.Find("ModalLayer") ?? canvas.transform;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            if (promptModal == null)
            {
                promptModal = new GameObject("WatchAdModal", typeof(RectTransform), typeof(Image));
                promptModal.transform.SetParent(parent, false);
                RectTransform rt = promptModal.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                promptModal.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.06f, 0.88f);

                GameObject card = new GameObject("DialogCard", typeof(RectTransform), typeof(Image));
                card.transform.SetParent(promptModal.transform, false);
                RectTransform cardRt = card.GetComponent<RectTransform>();
                cardRt.sizeDelta = new Vector2(680f, 400f);
                card.GetComponent<Image>().color = new Color(0.14f, 0.16f, 0.24f, 1f);

                CreateText(card.transform, "Title", "NEED MORE COINS?", font, 36, FontStyle.Bold,
                    new Color(1f, 0.85f, 0.2f), new Vector2(0f, 120f), new Vector2(620f, 60f));

                promptBodyText = CreateText(card.transform, "Body", $"Watch a short video to instantly receive {minCoinReward}–{maxCoinReward} FREE COINS!",
                    font, 26, FontStyle.Normal, Color.white, new Vector2(0f, 20f), new Vector2(580f, 110f));

                Button cancelBtn = CreateButton(card.transform, "Btn_Close", "NO THANKS", font,
                    new Color(0.28f, 0.32f, 0.42f), new Vector2(-155f, -120f), new Vector2(260f, 80f));
                cancelBtn.onClick.AddListener(() => promptModal.SetActive(false));

                Button watchBtn = CreateButton(card.transform, "Btn_WatchAd", $"WATCH AD (+{minCoinReward}-{maxCoinReward} COINS)", font,
                    new Color(0.18f, 0.72f, 0.36f), new Vector2(155f, -120f), new Vector2(310f, 80f));
                watchBtnText = watchBtn.GetComponentInChildren<Text>();
                watchBtn.onClick.AddListener(() => ShowRewardedAd(null));

                promptModal.SetActive(false);
            }

            if (adPlaybackOverlay == null)
            {
                adPlaybackOverlay = new GameObject("AdPlaybackOverlay", typeof(RectTransform), typeof(Image));
                adPlaybackOverlay.transform.SetParent(canvas.transform, false);
                RectTransform rt = adPlaybackOverlay.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                adPlaybackOverlay.GetComponent<Image>().color = new Color(0.04f, 0.05f, 0.08f, 0.98f);

                adCountdownText = CreateText(adPlaybackOverlay.transform, "CountdownText", "SPONSORED VIDEO AD", font, 38, FontStyle.Bold,
                    Color.white, Vector2.zero, new Vector2(800f, 240f));

                adPlaybackOverlay.SetActive(false);
            }
        }

        private static Text CreateText(Transform parent, string name, string content, Font font, int size, FontStyle style, Color color, Vector2 pos, Vector2 rectSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = rectSize;
            Text t = go.GetComponent<Text>();
            t.font = font;
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            return t;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font, Color bg, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = bg;
            CreateText(go.transform, "Text", label, font, 24, FontStyle.Bold, Color.white, Vector2.zero, size);
            return go.GetComponent<Button>();
        }
    }
}
