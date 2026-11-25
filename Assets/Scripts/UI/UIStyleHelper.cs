using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClawMachine.UI
{
    [DefaultExecutionOrder(-40)]
    public class UIStyleHelper : MonoBehaviour
    {
        private static Sprite cachedCardSprite;
        private static Sprite cachedPillButtonSprite;
        private static Sprite cachedPillPressedSprite;
        private static Sprite cachedBadgeSprite;
        private static Sprite cachedProgressBgSprite;
        private static Sprite cachedProgressFillSprite;
        private static Sprite cachedCoinIconSprite;
        private static Sprite cachedButtonBezelSprite;
        private static Sprite cachedButtonCapSprite;
        private static Sprite cachedJoystickBaseSprite;
        private static Sprite cachedJoystickBallSprite;
        private static Sprite cachedJoystickShaftSprite;
        private static Sprite cachedKnobShadowSprite;
        private static Sprite cachedDisplayScreenSprite;
        private static Sprite cachedShelfPedestalSprite;
        private static Sprite cachedBrassPlacardSprite;
        private static Font cachedFont;

        private static Sprite cachedIconTrophy;
        private static Sprite cachedIconCoin;
        private static Sprite cachedIconLightning;
        private static Sprite cachedIconGear;
        private static Sprite cachedIconArrowLeft;
        private static Sprite cachedIconArrowRight;
        private static Sprite cachedIconCloseX;
        private static Sprite cachedIconCheckmark;
        private static Sprite cachedIconSpeed;
        private static Sprite cachedIconMagnet;
        private static Sprite cachedIconTarget;

        public static Font MainFont
        {
            get
            {
                if (cachedFont != null) return cachedFont;
                cachedFont = Resources.Load<Font>("Fonts/LilitaOne");
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (cachedFont == null) cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return cachedFont;
            }
        }

        public static Sprite RoundedCardSprite => cachedCardSprite ??= LoadOrGenerateCardSprite();
        public static Sprite PillButtonGlossSprite => cachedPillButtonSprite ??= LoadOrGeneratePillButtonSprite(false);
        public static Sprite PillButtonPressedSprite => cachedPillPressedSprite ??= LoadOrGeneratePillButtonSprite(true);
        public static Sprite BadgePillSprite => cachedBadgeSprite ??= LoadOrGenerateBadgeSprite();
        public static Sprite ProgressBarBgSprite => cachedProgressBgSprite ??= LoadOrGenerateProgressBgSprite();
        public static Sprite ProgressBarFillSprite => cachedProgressFillSprite ??= LoadOrGenerateProgressFillSprite();
        public static Sprite CoinIconSprite => cachedCoinIconSprite ??= LoadOrGenerateCoinIconSprite();
        public static Sprite ArcadeButtonBezelSprite => cachedButtonBezelSprite ??= LoadOrGenerateButtonBezelSprite();
        public static Sprite ArcadeButtonCapSprite => cachedButtonCapSprite ??= LoadOrGenerateButtonCapSprite();
        public static Sprite ArcadeJoystickBaseSprite => cachedJoystickBaseSprite ??= LoadOrGenerateJoystickBaseSprite();
        public static Sprite ArcadeJoystickBallSprite => cachedJoystickBallSprite ??= LoadOrGenerateJoystickBallSprite();
        public static Sprite ArcadeJoystickShaftSprite => cachedJoystickShaftSprite ??= LoadOrGenerateJoystickShaftSprite();
        public static Sprite ArcadeKnobShadowSprite => cachedKnobShadowSprite ??= LoadOrGenerateKnobShadowSprite();
        public static Sprite ArcadeDisplayScreenSprite => cachedDisplayScreenSprite ??= LoadOrGenerateDisplayScreenSprite();
        public static Sprite ShelfPedestalSprite => cachedShelfPedestalSprite ??= LoadOrGenerateShelfPedestalSprite();
        public static Sprite BrassPlacardSprite => cachedBrassPlacardSprite ??= LoadOrGenerateBrassPlacardSprite();

        private static Sprite LoadIconSprite(string path)
        {
            var sp = Resources.Load<Sprite>(path);
            if (sp != null) return sp;
            var tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            return null;
        }

        public static Sprite IconTrophy => cachedIconTrophy ??= LoadIconSprite("UI/Icons/Icon_Trophy");
        public static Sprite IconCoin => cachedIconCoin ??= LoadIconSprite("UI/Icons/Icon_Coin_3D") ?? CoinIconSprite;
        public static Sprite IconLightning => cachedIconLightning ??= LoadIconSprite("UI/Icons/Icon_Lightning");
        public static Sprite IconGear => cachedIconGear ??= LoadIconSprite("UI/Icons/Icon_Gear");
        public static Sprite IconArrowLeft => cachedIconArrowLeft ??= LoadIconSprite("UI/Icons/Icon_Arrow_Left");
        public static Sprite IconArrowRight => cachedIconArrowRight ??= LoadIconSprite("UI/Icons/Icon_Arrow_Right");
        public static Sprite IconCloseX => cachedIconCloseX ??= LoadIconSprite("UI/Icons/Icon_Close_X");
        public static Sprite IconCheckmark => cachedIconCheckmark ??= LoadIconSprite("UI/Icons/Icon_Checkmark");
        public static Sprite IconUpgradeSpeed => cachedIconSpeed ??= LoadIconSprite("UI/Icons/Icon_Upgrade_Speed");
        public static Sprite IconUpgradeMagnet => cachedIconMagnet ??= LoadIconSprite("UI/Icons/Icon_Upgrade_Magnet");
        public static Sprite IconUpgradeTarget => cachedIconTarget ??= LoadIconSprite("UI/Icons/Icon_Upgrade_Target");

        public static Image EnsureChildIcon(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 pos)
        {
            if (parent == null || sprite == null) return null;
            Transform existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = new GameObject(name, typeof(RectTransform), typeof(Image));
                go.transform.SetParent(parent, false);
            }
            else
            {
                go = existing.gameObject;
            }

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        private void Awake()
        {
            ApplyStyleToHierarchy(gameObject);
        }

        public static void ApplyAll(GameObject root)
        {
            if (root == null) return;
            ApplyStyleToHierarchy(root);
        }

        public static void ApplyStyleToHierarchy(GameObject root)
        {
            if (root == null) return;

            // 1. Style all buttons
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                StyleButton(buttons[i]);
            }

            // 2. Style all images matching card, modal, or badge patterns
            Image[] images = root.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image img = images[i];
                if (img == null) continue;
                string name = img.gameObject.name;

                if (name == "DialogCard" || name == "Card" || name.EndsWith("Card") || name.EndsWith("Modal") || name == "Panel_Content" || name.EndsWith("Box"))
                {
                    StyleCard(img);
                }
                else if (name.StartsWith("Badge_") || name == "RarityBadge" || name.EndsWith("Badge"))
                {
                    StyleBadge(img);
                }
                else if (name.StartsWith("Pip_") || name == "RedDot" || name.EndsWith("Pip"))
                {
                    StyleNotificationPip(img);
                }
                else if (name == "ProgressBar_Bg" || name == "Track")
                {
                    img.sprite = ProgressBarBgSprite;
                    img.type = Image.Type.Sliced;
                }
                else if (name == "ProgressBar_Fill" || name == "Fill")
                {
                    img.sprite = ProgressBarFillSprite;
                    img.type = Image.Type.Sliced;
                }
            }

            // 3. Set Lilita One font and drop shadow for clean readability
            Font f = MainFont;
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                Text txt = texts[i];
                if (txt == null) continue;
                if (f != null) txt.font = f;
                // Legacy Text renders NOTHING when lines don't fit vertically; never hide labels.
                txt.verticalOverflow = VerticalWrapMode.Overflow;
                string n = txt.gameObject.name;
                if (n.Contains("Title") || n.Contains("Header") || n == "PrizeName" || n.Contains("Count") || n.Contains("Balance") || n.Contains("Machine"))
                {
                    AddDropShadow(txt, new Color(0f, 0f, 0f, 0.75f), new Vector2(1.5f, -2f));
                }
                else
                {
                    AddDropShadow(txt, new Color(0f, 0f, 0f, 0.6f), new Vector2(1f, -1.5f));
                }
            }
        }

        private static readonly HashSet<int> wiredButtonIds = new HashSet<int>();

        public static void StyleButton(Button button)
        {
            if (button == null) return;

            int id = button.GetInstanceID();
            if (wiredButtonIds.Add(id))
            {
                button.onClick.AddListener(() =>
                {
                    if (Core.Services.ServiceLocator.TryGet<Core.Services.IAudioService>(out var audio))
                    {
                        audio.PlayUIClick();
                    }
                });
            }

            Image img = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (img != null)
            {
                // Only replace if default Unity white/square sprite or null
                if (img.sprite == null || img.sprite.name.StartsWith("UISprite") || img.sprite.name.Contains("Background"))
                {
                    img.sprite = PillButtonGlossSprite;
                    img.type = Image.Type.Sliced;
                }

                button.targetGraphic = img;
                button.transition = Selectable.Transition.SpriteSwap;
                SpriteState ss = button.spriteState;
                ss.pressedSprite = PillButtonPressedSprite;
                ss.highlightedSprite = img.sprite;
                button.spriteState = ss;
            }

            // Drop shadow on button label
            Text btnText = button.GetComponentInChildren<Text>(true);
            if (btnText != null)
            {
                if (MainFont != null) btnText.font = MainFont;
                AddDropShadow(btnText, new Color(0f, 0f, 0f, 0.70f), new Vector2(1f, -1.5f));
            }
        }

        public static void StyleCard(Image img)
        {
            if (img == null) return;
            if (img.sprite == null || img.sprite.name.StartsWith("UISprite") || img.sprite.name.Contains("Background"))
            {
                img.sprite = RoundedCardSprite;
                img.type = Image.Type.Sliced;
            }
        }

        public static void StyleBadge(Image img)
        {
            if (img == null) return;
            if (img.sprite == null || img.sprite.name.StartsWith("UISprite") || img.sprite.name.Contains("Background"))
            {
                img.sprite = BadgePillSprite;
                img.type = Image.Type.Sliced;
            }
        }

        public static void StyleNotificationPip(Image img)
        {
            if (img == null) return;
            img.sprite = BadgePillSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.92f, 0.16f, 0.16f, 1f);
            RectTransform rt = img.rectTransform;
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(22f, 22f);
            }
        }

        public static void AddDropShadow(Text text, Color shadowColor, Vector2 distance)
        {
            if (text == null) return;
            // Outline derives from Shadow, so skip it when looking for the plain drop shadow.
            Shadow s = null;
            foreach (Shadow c in text.GetComponents<Shadow>())
            {
                if (!(c is Outline)) { s = c; break; }
            }
            if (s == null) s = text.gameObject.AddComponent<Shadow>();
            s.effectColor = shadowColor;
            s.effectDistance = distance;
            s.useGraphicAlpha = true;

            // Chunky dark outline (mobile-arcade look).
            Outline o = text.GetComponent<Outline>();
            if (o == null) o = text.gameObject.AddComponent<Outline>();
            o.effectColor = new Color(0.04f, 0.06f, 0.11f, 0.9f);
            o.effectDistance = new Vector2(2f, 2f);
            o.useGraphicAlpha = true;
        }

        public static RectTransform EnsureModalShell(GameObject modalRoot, string title, System.Action onClose, float cardWidth = 660f, float cardHeight = 880f)
        {
            if (modalRoot == null) return null;

            RectTransform rootRt = modalRoot.GetComponent<RectTransform>();
            if (rootRt != null)
            {
                rootRt.anchorMin = Vector2.zero;
                rootRt.anchorMax = Vector2.one;
                rootRt.offsetMin = Vector2.zero;
                rootRt.offsetMax = Vector2.zero;
            }

            Image rootImg = modalRoot.GetComponent<Image>();
            if (rootImg != null)
            {
                rootImg.sprite = null;
                rootImg.color = new Color(0.04f, 0.05f, 0.08f, 0.85f);
                rootImg.raycastTarget = true;
            }

            Button rootBtn = modalRoot.GetComponent<Button>();
            if (rootBtn == null) rootBtn = modalRoot.AddComponent<Button>();
            rootBtn.transition = Selectable.Transition.None;
            rootBtn.onClick.RemoveAllListeners();
            if (onClose != null) rootBtn.onClick.AddListener(() => onClose());

            Transform cardTr = modalRoot.transform.Find("DialogCard");
            GameObject cardObj;
            if (cardTr == null)
            {
                cardObj = new GameObject("DialogCard", typeof(RectTransform), typeof(Image));
                cardObj.transform.SetParent(modalRoot.transform, false);
                cardTr = cardObj.transform;
            }
            else
            {
                cardObj = cardTr.gameObject;
            }

            cardTr.SetAsLastSibling();

            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.pivot = new Vector2(0.5f, 0.5f);
            cardRt.anchoredPosition = Vector2.zero;
            cardRt.sizeDelta = new Vector2(cardWidth, cardHeight);

            Image cardImg = cardObj.GetComponent<Image>();
            if (cardImg != null)
            {
                cardImg.sprite = RoundedCardSprite;
                cardImg.type = Image.Type.Sliced;
                cardImg.color = Color.white;
                cardImg.raycastTarget = true;
            }

            Transform headerTr = cardTr.Find("ModalHeader");
            if (headerTr == null)
            {
                GameObject headerGo = new GameObject("ModalHeader", typeof(RectTransform), typeof(Image));
                headerGo.transform.SetParent(cardTr, false);
                headerTr = headerGo.transform;

                Image hImg = headerGo.GetComponent<Image>();
                hImg.sprite = BadgePillSprite;
                hImg.type = Image.Type.Sliced;
                hImg.color = new Color(0.10f, 0.12f, 0.18f, 0.95f);
                hImg.raycastTarget = false;

                GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(Text));
                titleGo.transform.SetParent(headerTr, false);
                RectTransform tRt = titleGo.GetComponent<RectTransform>();
                tRt.anchorMin = Vector2.zero;
                tRt.anchorMax = Vector2.one;
                tRt.offsetMin = new Vector2(16f, 0f);
                tRt.offsetMax = new Vector2(-16f, 0f);

                Text titleTxt = titleGo.GetComponent<Text>();
                titleTxt.font = MainFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                titleTxt.text = title;
                titleTxt.fontSize = 22;
                titleTxt.fontStyle = FontStyle.Bold;
                titleTxt.alignment = TextAnchor.MiddleCenter;
                titleTxt.color = Color.white;
                AddDropShadow(titleTxt, new Color(0f, 0f, 0f, 0.7f), new Vector2(1f, -1.5f));
            }
            else
            {
                Text t = headerTr.GetComponentInChildren<Text>();
                if (t != null)
                {
                    if (MainFont != null) t.font = MainFont;
                    t.text = title;
                }
            }

            RectTransform hRt = headerTr.GetComponent<RectTransform>();
            if (hRt != null)
            {
                hRt.anchorMin = new Vector2(0f, 1f);
                hRt.anchorMax = new Vector2(1f, 1f);
                hRt.pivot = new Vector2(0.5f, 1f);
                hRt.anchoredPosition = new Vector2(0f, -10f);
                hRt.sizeDelta = new Vector2(-28f, 56f);
            }

            Transform closeTr = cardTr.Find("ModalCloseBtn");
            if (closeTr == null)
            {
                GameObject closeGo = new GameObject("ModalCloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                closeGo.transform.SetParent(cardTr, false);
                closeTr = closeGo.transform;

                Image cImg = closeGo.GetComponent<Image>();
                cImg.sprite = BadgePillSprite;
                cImg.type = Image.Type.Sliced;
                cImg.color = new Color(0.24f, 0.28f, 0.40f, 1f);

                Image xImg = EnsureChildIcon(closeTr, "Icon_X", IconCloseX, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20f, 20f), Vector2.zero);
                if (xImg == null)
                {
                    GameObject xTxtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
                    xTxtGo.transform.SetParent(closeTr, false);
                    RectTransform xRt = xTxtGo.GetComponent<RectTransform>();
                    xRt.anchorMin = Vector2.zero;
                    xRt.anchorMax = Vector2.one;
                    xRt.offsetMin = Vector2.zero;
                    xRt.offsetMax = Vector2.zero;

                    Text xTxt = xTxtGo.GetComponent<Text>();
                    xTxt.font = MainFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    xTxt.text = "✕";
                    xTxt.fontSize = 20;
                    xTxt.fontStyle = FontStyle.Bold;
                    xTxt.alignment = TextAnchor.MiddleCenter;
                    xTxt.color = Color.white;
                }

                Button cBtn = closeGo.GetComponent<Button>();
                if (onClose != null) cBtn.onClick.AddListener(() => onClose());
                StyleButton(cBtn);
            }

            RectTransform cRt = closeTr.GetComponent<RectTransform>();
            if (cRt != null)
            {
                cRt.anchorMin = new Vector2(1f, 1f);
                cRt.anchorMax = new Vector2(1f, 1f);
                cRt.pivot = new Vector2(0.5f, 0.5f);
                cRt.anchoredPosition = new Vector2(-36f, -38f);
                cRt.sizeDelta = new Vector2(40f, 40f);
            }

            Transform contentTr = cardTr.Find("ContentContainer");
            GameObject contentGo;
            if (contentTr == null)
            {
                contentGo = new GameObject("ContentContainer", typeof(RectTransform));
                contentGo.transform.SetParent(cardTr, false);
                contentTr = contentGo.transform;
            }
            else
            {
                contentGo = contentTr.gameObject;
            }

            RectTransform contentRt = contentGo.GetComponent<RectTransform>();
            contentRt.anchorMin = Vector2.zero;
            contentRt.anchorMax = Vector2.one;
            contentRt.offsetMin = new Vector2(16f, 16f);
            contentRt.offsetMax = new Vector2(-16f, -74f);

            return contentRt;
        }

        // =====================================================================
        // UI JUICE & TWEEN ANIMATIONS
        // =====================================================================

        public static Coroutine AnimatePopIn(MonoBehaviour runner, Transform target, System.Action onComplete = null)
        {
            if (runner == null || target == null) return null;
            return runner.StartCoroutine(CoPopIn(target, onComplete));
        }

        private static System.Collections.IEnumerator CoPopIn(Transform target, System.Action onComplete)
        {
            if (target == null) yield break;
            float elapsed = 0f;
            float duration = 0.22f;
            Vector3 startScale = Vector3.one * 0.72f;
            Vector3 peakScale = Vector3.one * 1.05f;

            target.localScale = startScale;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (target == null) yield break;
                target.localScale = (t < 0.5f)
                    ? Vector3.Lerp(startScale, peakScale, t / 0.5f)
                    : Vector3.Lerp(peakScale, Vector3.one, (t - 0.5f) / 0.5f);
                yield return null;
            }
            if (target != null) target.localScale = Vector3.one;
            onComplete?.Invoke();
        }

        public static Coroutine AnimateScalePunch(MonoBehaviour runner, Transform target, float punchScale = 1.18f, float duration = 0.22f)
        {
            if (runner == null || target == null) return null;
            return runner.StartCoroutine(CoScalePunch(target, punchScale, duration));
        }

        private static System.Collections.IEnumerator CoScalePunch(Transform target, float punchScale, float duration)
        {
            if (target == null) yield break;
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one;
            Vector3 peak = baseScale * punchScale;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (target == null) yield break;
                target.localScale = (t < 0.4f)
                    ? Vector3.Lerp(baseScale, peak, t / 0.4f)
                    : Vector3.Lerp(peak, baseScale, (t - 0.4f) / 0.6f);
                yield return null;
            }
            if (target != null) target.localScale = baseScale;
        }

        public static Coroutine AnimateNumberRoll(MonoBehaviour runner, Text targetText, int from, int to, float duration = 0.35f, string prefix = "", string suffix = " COINS")
        {
            if (runner == null || targetText == null) return null;
            return runner.StartCoroutine(CoNumberRoll(targetText, from, to, duration, prefix, suffix));
        }

        private static System.Collections.IEnumerator CoNumberRoll(Text targetText, int from, int to, float duration, string prefix, string suffix)
        {
            if (targetText == null) yield break;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (targetText == null) yield break;
                int val = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
                targetText.text = $"{prefix}{val:N0}{suffix}";
                yield return null;
            }
            if (targetText != null) targetText.text = $"{prefix}{to:N0}{suffix}";
        }

        public static void SpawnFloatingCoinReward(MonoBehaviour runner, int amount, Vector2? screenPosition = null, Transform originTransform = null)
        {
            if (amount <= 0) return;
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            Transform parent = canvas.transform.Find("SafeAreaContainer") ?? canvas.transform;

            GameObject go = new GameObject("FloatingCoinReward", typeof(RectTransform), typeof(CanvasGroup));
            go.transform.SetParent(parent, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            Vector2 startPos = screenPosition ?? (originTransform != null ? (Vector2)parent.InverseTransformPoint(originTransform.position) : new Vector2(0f, 60f));
            rt.anchoredPosition = startPos;
            rt.sizeDelta = new Vector2(240f, 50f);

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(go.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.anchoredPosition = new Vector2(25f, 0f);
            iconRt.sizeDelta = new Vector2(36f, 36f);
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = CoinIconSprite;
            iconImg.raycastTarget = false;

            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            txtObj.transform.SetParent(go.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0f, 0f);
            txtRt.anchorMax = new Vector2(1f, 1f);
            txtRt.offsetMin = new Vector2(50f, 0f);
            txtRt.offsetMax = Vector2.zero;
            Text txt = txtObj.GetComponent<Text>();
            txt.font = MainFont;
            txt.fontSize = 28;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(1.0f, 0.88f, 0.20f, 1f);
            txt.alignment = TextAnchor.MiddleLeft;
            txt.text = $"+{amount:N0}";
            txt.raycastTarget = false;

            Shadow shadow = txtObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.04f, 0.05f, 0.08f, 0.9f);
            shadow.effectDistance = new Vector2(2f, -2f);

            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            if (runner != null)
            {
                runner.StartCoroutine(CoFloatingReward(go, rt, cg, startPos));
            }
            else
            {
                var helper = Object.FindFirstObjectByType<UIStyleHelper>();
                if (helper != null) helper.StartCoroutine(CoFloatingReward(go, rt, cg, startPos));
                else Object.Destroy(go, 1.2f);
            }
        }

        private static System.Collections.IEnumerator CoFloatingReward(GameObject go, RectTransform rt, CanvasGroup cg, Vector2 startPos)
        {
            float duration = 1.1f;
            float elapsed = 0f;
            Vector3 startScale = Vector3.one * 0.6f;
            Vector3 peakScale = Vector3.one * 1.15f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (go == null) yield break;

                if (t < 0.25f)
                {
                    rt.localScale = Vector3.Lerp(startScale, peakScale, t / 0.25f);
                }
                else if (t < 0.45f)
                {
                    rt.localScale = Vector3.Lerp(peakScale, Vector3.one, (t - 0.25f) / 0.20f);
                }
                else
                {
                    rt.localScale = Vector3.one;
                }

                float floatDist = Mathf.SmoothStep(0f, 95f, t);
                rt.anchoredPosition = startPos + new Vector2(0f, floatDist);

                if (t > 0.65f)
                {
                    cg.alpha = 1f - (t - 0.65f) / 0.35f;
                }
                else
                {
                    cg.alpha = 1f;
                }

                yield return null;
            }

            if (go != null) Object.Destroy(go);
        }

        // =====================================================================
        // PROCEDURAL RUNTIME SPRITE LOADERS / FALLBACKS
        // =====================================================================

        private static Sprite LoadOrGenerateCardSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_RoundedCard");
            if (s != null) return s;

            int w = 128, h = 128, r = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color borderCol = new Color(0.95f, 0.95f, 0.98f, 1f);
            Color fillCol = new Color(0.90f, 0.92f, 0.96f, 0.95f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, r);
                    if (d > 0f)
                    {
                        float alpha = Mathf.Clamp01(1f - d);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * 0.15f));
                    }
                    else if (d > -3.5f)
                    {
                        tex.SetPixel(x, y, borderCol);
                    }
                    else
                    {
                        float v = Mathf.Lerp(0.85f, 1.0f, (float)y / h);
                        tex.SetPixel(x, y, fillCol * v);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(32, 32, 32, 32));
        }

        private static Sprite LoadOrGeneratePillButtonSprite(bool pressed)
        {
            string path = pressed ? "UI/UI_PillButton_Pressed" : "UI/UI_PillButton_Gloss";
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null) return s;

            int w = 128, h = 64, r = 20;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, r);
                    if (d > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (d > -2.5f)
                    {
                        // Outer rim
                        tex.SetPixel(x, y, pressed ? new Color(0.5f, 0.5f, 0.55f, 0.9f) : new Color(1f, 1f, 1f, 0.95f));
                    }
                    else
                    {
                        float ny = (float)y / h;
                        float gloss = pressed ? (0.6f + ny * 0.2f) : (ny > 0.5f ? 1.0f : 0.82f);
                        tex.SetPixel(x, y, new Color(gloss, gloss, gloss, 1f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(30, 18, 30, 18));
        }

        private static Sprite LoadOrGenerateBadgeSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_BadgePill");
            if (s != null) return s;

            int w = 96, h = 48, r = 18;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, r);
                    if (d > 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (d > -2.5f)
                    {
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.85f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(0.18f, 0.22f, 0.30f, 0.92f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(24, 14, 24, 14));
        }

        private static Sprite LoadOrGenerateProgressBgSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_ProgressBar_Bg");
            if (s != null) return s;

            int w = 64, h = 32, r = 10;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, r);
                    if (d > 0f) tex.SetPixel(x, y, Color.clear);
                    else if (d > -2f) tex.SetPixel(x, y, new Color(0.1f, 0.1f, 0.15f, 0.8f));
                    else tex.SetPixel(x, y, new Color(0.12f, 0.15f, 0.22f, 0.95f));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16, 10, 16, 10));
        }

        private static Sprite LoadOrGenerateProgressFillSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_ProgressBar_Fill");
            if (s != null) return s;

            int w = 64, h = 32, r = 10;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, r);
                    if (d > 0f) tex.SetPixel(x, y, Color.clear);
                    else
                    {
                        float gloss = ((float)y / h) > 0.5f ? 1.0f : 0.82f;
                        tex.SetPixel(x, y, new Color(gloss, gloss, gloss, 1.0f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16, 10, 16, 10));
        }

        private static Sprite LoadOrGenerateCoinIconSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Icon_Coin");
            if (s != null) return s;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float r = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist > r)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist > r - 3.5f)
                    {
                        tex.SetPixel(x, y, new Color(0.85f, 0.65f, 0.10f, 1f));
                    }
                    else
                    {
                        float inner = dist / (r - 3.5f);
                        Color c = Color.Lerp(new Color(1f, 0.92f, 0.35f), new Color(1f, 0.78f, 0.12f), inner);
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float DistanceToBox(float px, float py, float width, float height, float radius)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float cx = Mathf.Abs(px - halfW) - (halfW - radius);
            float cy = Mathf.Abs(py - halfH) - (halfH - radius);
            float outside = Mathf.Sqrt(Mathf.Max(cx, 0f) * Mathf.Max(cx, 0f) + Mathf.Max(cy, 0f) * Mathf.Max(cy, 0f));
            float inside = Mathf.Min(Mathf.Max(cx, cy), 0f);
            return outside + inside - radius;
        }

        private static Sprite LoadOrGenerateButtonBezelSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Button_Bezel");
            if (s != null) return s;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float rOuter = size * 0.48f;
            float rInner = size * 0.38f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist > rOuter)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (dist >= rInner)
                    {
                        // Bezel metallic ring
                        float t = (dist - rInner) / (rOuter - rInner);
                        float shade = Mathf.Lerp(0.18f, 0.42f, t) + (y > center ? 0.08f : -0.06f);
                        tex.SetPixel(x, y, new Color(shade, shade * 1.02f, shade * 1.05f, 1f));
                    }
                    else
                    {
                        // Well recess shadow
                        float depth = dist / rInner;
                        float shadow = Mathf.Lerp(0.08f, 0.14f, depth);
                        tex.SetPixel(x, y, new Color(shadow, shadow, shadow, 1f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite LoadOrGenerateButtonCapSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Button_Cap");
            if (s != null) return s;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float r = size * 0.46f;
            Vector2 lightOffset = new Vector2(center - 12f, center + 14f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, new Vector2(center, center));
                    if (dist > r)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float rim = dist / r;
                        float specDist = Vector2.Distance(pos, lightOffset) / (r * 0.85f);
                        float spec = Mathf.Clamp01(1f - specDist);
                        spec = Mathf.Pow(spec, 3.5f) * 0.55f;

                        // Vibrant glossy arcade red dome
                        Color baseColor = Color.Lerp(new Color(1f, 0.28f, 0.25f, 1f), new Color(0.68f, 0.08f, 0.10f, 1f), rim);
                        Color finalColor = Color.Lerp(baseColor, Color.white, spec);
                        if (rim > 0.90f) finalColor = Color.Lerp(finalColor, new Color(0.35f, 0.02f, 0.04f, 1f), (rim - 0.90f) / 0.10f);
                        tex.SetPixel(x, y, finalColor);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite LoadOrGenerateJoystickBaseSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Joystick_Base");
            if (s != null) return s;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float rOuter = size * 0.47f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist > rOuter)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        // Black arcade dust washer with circular grooving
                        float norm = dist / rOuter;
                        float rings = Mathf.Sin(norm * 30f) * 0.03f;
                        float v = Mathf.Lerp(0.12f, 0.06f, norm) + rings;
                        if (norm > 0.88f) v = (y > center ? 0.42f : 0.26f); // bright metal rim so base reads on dark deck
                        tex.SetPixel(x, y, new Color(v, v, v + 0.01f, 0.94f));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite LoadOrGenerateJoystickBallSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Joystick_Ball");
            if (s != null) return s;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;
            float r = size * 0.46f;
            Vector2 specPos = new Vector2(center - 10f, center + 14f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, new Vector2(center, center));
                    if (dist > r)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float rim = dist / r;
                        float specDist = Vector2.Distance(pos, specPos) / (r * 0.75f);
                        float spec = Mathf.Pow(Mathf.Clamp01(1f - specDist), 4.0f) * 0.65f;

                        // Vibrant candy-red / ruby ball top
                        Color col = Color.Lerp(new Color(0.96f, 0.22f, 0.28f, 1f), new Color(0.48f, 0.05f, 0.12f, 1f), rim);
                        col = Color.Lerp(col, Color.white, spec);
                        tex.SetPixel(x, y, col);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite LoadOrGenerateShelfPedestalSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Shelf_Pedestal");
            if (s != null) return s;

            int w = 128;
            int h = 128;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float radius = 14f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, radius);
                    if (d > 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float ny = (float)y / h;
                        Color c;
                        if (ny < 0.16f)
                        {
                            // Wooden / brass shelf ledge base
                            float ledgeT = ny / 0.16f;
                            c = Color.Lerp(new Color(0.38f, 0.24f, 0.12f, 1f), new Color(0.58f, 0.38f, 0.20f, 1f), ledgeT);
                        }
                        else if (ny < 0.20f)
                        {
                            // Ledge highlight lip
                            c = new Color(0.82f, 0.62f, 0.32f, 1f);
                        }
                        else
                        {
                            // Inset alcove shadow (dark arcade cabinet shelf alcove)
                            float insetY = (ny - 0.20f) / 0.80f;
                            c = Color.Lerp(new Color(0.10f, 0.11f, 0.16f, 0.95f), new Color(0.05f, 0.06f, 0.09f, 0.98f), insetY);
                        }
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16, 26, 16, 16));
        }

        private static Sprite LoadOrGenerateBrassPlacardSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Brass_Placard");
            if (s != null) return s;

            int w = 96;
            int h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float radius = 6f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float d = DistanceToBox(x, y, w, h, radius);
                    if (d > 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float ny = (float)y / h;
                        // Metallic brass gradient with bright border
                        Color c = Color.Lerp(new Color(0.68f, 0.52f, 0.20f, 1f), new Color(0.92f, 0.78f, 0.38f, 1f), ny);
                        if (d > -2.2f)
                        {
                            c = new Color(1f, 0.92f, 0.55f, 1f); // bevel highlight
                        }
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
        }

        public static Slider CreateStyledSlider(Transform parent, string name, float min, float max, float cur, System.Action<float> onChange)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(parent, false);

            RectTransform rt = sliderObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(240, 28);

            // Background
            GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            Image bgImg = bgObj.GetComponent<Image>();
            bgImg.sprite = ProgressBarBgSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.12f, 0.14f, 0.20f, 0.95f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.GetComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0, 0.2f);
            faRt.anchorMax = new Vector2(1, 0.8f);
            faRt.sizeDelta = new Vector2(-16, 0);

            // Fill
            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(fillArea.transform, false);
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.sprite = ProgressBarFillSprite;
            fillImg.type = Image.Type.Sliced;
            fillImg.color = new Color(0.20f, 0.82f, 0.45f, 1f);

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRt = handleArea.GetComponent<RectTransform>();
            haRt.anchorMin = Vector2.zero;
            haRt.anchorMax = Vector2.one;
            haRt.sizeDelta = new Vector2(-20, 0);

            // Handle
            GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObj.transform.SetParent(handleArea.transform, false);
            RectTransform handleRt = handleObj.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(26, 26);
            Image handleImg = handleObj.GetComponent<Image>();
            handleImg.sprite = CoinIconSprite;
            handleImg.color = Color.white;

            Slider slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = cur;

            if (onChange != null)
            {
                slider.onValueChanged.AddListener(val => onChange(val));
            }

            return slider;
        }

        private static Sprite LoadOrGenerateJoystickShaftSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Joystick_Shaft");
            if (s != null) return s;

            int w = 32;
            int h = 128;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (float)x / (w - 1);
                    // Chrome cylinder gradient
                    float spec = Mathf.Exp(-Mathf.Pow((nx - 0.32f) * 7.5f, 2f));
                    float baseMetallic = Mathf.Lerp(0.35f, 0.78f, Mathf.Sin(nx * Mathf.PI));
                    float v = Mathf.Clamp01(baseMetallic + spec * 0.45f);
                    if (nx < 0.08f || nx > 0.92f) v *= 0.45f; // rim shadow
                    tex.SetPixel(x, y, new Color(v, v * 1.02f, v * 1.05f, 1f));
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 100f);
        }

        private static Sprite LoadOrGenerateKnobShadowSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Knob_Shadow");
            if (s != null) return s;

            int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / (size * 0.46f);
                    float dy = (y - center) / (size * 0.32f); // slight oval
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > 1f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Pow(1f - d, 1.8f) * 0.55f;
                        tex.SetPixel(x, y, new Color(0.02f, 0.02f, 0.04f, alpha));
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite LoadOrGenerateDisplayScreenSprite()
        {
            Sprite s = Resources.Load<Sprite>("UI/UI_Arcade_Display_Screen");
            if (s != null) return s;

            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            float r = 16f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = DistanceToBox(x, y, size, size, r);
                    if (d > 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else if (d > -3.5f)
                    {
                        // Outer bezel border
                        float b = Mathf.Lerp(0.18f, 0.38f, (float)y / size);
                        tex.SetPixel(x, y, new Color(b, b * 1.05f, b * 1.15f, 1f));
                    }
                    else if (d > -5.5f)
                    {
                        // Inner bezel bevel line
                        tex.SetPixel(x, y, new Color(0.03f, 0.04f, 0.07f, 1f));
                    }
                    else
                    {
                        // Recessed dark blue arcade LCD screen
                        float ny = (float)y / size;
                        Color screenCol = Color.Lerp(new Color(0.03f, 0.06f, 0.12f, 0.98f), new Color(0.05f, 0.09f, 0.16f, 0.98f), ny);
                        tex.SetPixel(x, y, screenCol);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(18, 18, 18, 18));
        }
    }
}
