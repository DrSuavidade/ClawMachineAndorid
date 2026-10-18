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
        private static Sprite cachedShelfPedestalSprite;
        private static Sprite cachedBrassPlacardSprite;

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
        public static Sprite ShelfPedestalSprite => cachedShelfPedestalSprite ??= LoadOrGenerateShelfPedestalSprite();
        public static Sprite BrassPlacardSprite => cachedBrassPlacardSprite ??= LoadOrGenerateBrassPlacardSprite();

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

            // 3. Add drop shadow to titles and headers for readability
            Text[] texts = root.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                Text txt = texts[i];
                if (txt == null) continue;
                string n = txt.gameObject.name;
                if (n.Contains("Title") || n.Contains("Header") || n == "PrizeName" || n.Contains("Count") || n.Contains("Balance"))
                {
                    AddDropShadow(txt, new Color(0f, 0f, 0f, 0.65f), new Vector2(1.5f, -2f));
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
                AddDropShadow(btnText, new Color(0f, 0f, 0f, 0.55f), new Vector2(1f, -1.5f));
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

        public static void AddDropShadow(Text text, Color shadowColor, Vector2 distance)
        {
            if (text == null) return;
            Shadow s = text.GetComponent<Shadow>();
            if (s == null) s = text.gameObject.AddComponent<Shadow>();
            s.effectColor = shadowColor;
            s.effectDistance = distance;
            s.useGraphicAlpha = true;
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
    }
}
