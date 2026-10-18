#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClawMachine.Editor
{
    [InitializeOnLoad]
    public static class UIAndParticleAssetGenerator
    {
        private const string ParticleDir = "Assets/Textures/Particles";
        private const string UIDir = "Assets/Textures/UI";
        private const string ResParticleDir = "Assets/Resources/Particles";
        private const string ResUIDir = "Assets/Resources/UI";
        private const string VersionPrefKey = "ClawMachine_UI_Particle_Gen_v2";

        static UIAndParticleAssetGenerator()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool(VersionPrefKey, false))
                {
                    SessionState.SetBool(VersionPrefKey, true);
                    GenerateAllAssets();
                }
            };
        }

        [MenuItem("ClawMachine/Generate UI and Particle Textures")]
        public static void GenerateAllAssets()
        {
            if (!Directory.Exists(ParticleDir)) Directory.CreateDirectory(ParticleDir);
            if (!Directory.Exists(UIDir)) Directory.CreateDirectory(UIDir);
            if (!Directory.Exists(ResParticleDir)) Directory.CreateDirectory(ResParticleDir);
            if (!Directory.Exists(ResUIDir)) Directory.CreateDirectory(ResUIDir);

            // 1. Particle Textures
            GenerateSparkleStar($"{ParticleDir}/Particle_SparkleStar.png", 64);
            GenerateSoftPuff($"{ParticleDir}/Particle_SoftPuff.png", 64);
            GenerateCoinParticle($"{ParticleDir}/Particle_Coin.png", 64);
            GenerateConfettiRibbon($"{ParticleDir}/Particle_ConfettiRibbon.png", 32, 64);
            GenerateGlowRing($"{ParticleDir}/Particle_GlowRing.png", 64);

            // Mirror to Resources/Particles
            CopyAndImportFile($"{ParticleDir}/Particle_SparkleStar.png", $"{ResParticleDir}/Particle_SparkleStar.png", Vector4.zero);
            CopyAndImportFile($"{ParticleDir}/Particle_SoftPuff.png", $"{ResParticleDir}/Particle_SoftPuff.png", Vector4.zero);
            CopyAndImportFile($"{ParticleDir}/Particle_Coin.png", $"{ResParticleDir}/Particle_Coin.png", Vector4.zero);
            CopyAndImportFile($"{ParticleDir}/Particle_ConfettiRibbon.png", $"{ResParticleDir}/Particle_ConfettiRibbon.png", Vector4.zero);
            CopyAndImportFile($"{ParticleDir}/Particle_GlowRing.png", $"{ResParticleDir}/Particle_GlowRing.png", Vector4.zero);

            // 2. UI Sprites (9-slice borders)
            Vector4 cardBorder = new Vector4(36, 36, 36, 36);
            Vector4 buttonBorder = new Vector4(32, 18, 32, 18);
            Vector4 badgeBorder = new Vector4(24, 14, 24, 14);
            Vector4 progressBorder = new Vector4(16, 10, 16, 10);

            GenerateRoundedCard($"{UIDir}/UI_RoundedCard.png", 128, 128, 28, cardBorder);
            GeneratePillButton($"{UIDir}/UI_PillButton_Gloss.png", 128, 64, 20, false, buttonBorder);
            GeneratePillButton($"{UIDir}/UI_PillButton_Pressed.png", 128, 64, 20, true, buttonBorder);
            GenerateBadgePill($"{UIDir}/UI_BadgePill.png", 96, 48, 18, badgeBorder);
            GenerateProgressTrack($"{UIDir}/UI_ProgressBar_Bg.png", 64, 32, progressBorder);
            GenerateProgressFill($"{UIDir}/UI_ProgressBar_Fill.png", 64, 32, progressBorder);
            GenerateCoinIcon($"{UIDir}/UI_Icon_Coin.png", 64);

            // Mirror to Resources/UI
            CopyAndImportFile($"{UIDir}/UI_RoundedCard.png", $"{ResUIDir}/UI_RoundedCard.png", cardBorder);
            CopyAndImportFile($"{UIDir}/UI_PillButton_Gloss.png", $"{ResUIDir}/UI_PillButton_Gloss.png", buttonBorder);
            CopyAndImportFile($"{UIDir}/UI_PillButton_Pressed.png", $"{ResUIDir}/UI_PillButton_Pressed.png", buttonBorder);
            CopyAndImportFile($"{UIDir}/UI_BadgePill.png", $"{ResUIDir}/UI_BadgePill.png", badgeBorder);
            CopyAndImportFile($"{UIDir}/UI_ProgressBar_Bg.png", $"{ResUIDir}/UI_ProgressBar_Bg.png", progressBorder);
            CopyAndImportFile($"{UIDir}/UI_ProgressBar_Fill.png", $"{ResUIDir}/UI_ProgressBar_Fill.png", progressBorder);
            CopyAndImportFile($"{UIDir}/UI_Icon_Coin.png", $"{ResUIDir}/UI_Icon_Coin.png", Vector4.zero);

            AssignSpritesToCabinetThemes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[UIAndParticleAssetGenerator] Successfully generated and configured high-definition UI and particle sprites!");
        }

        private static void AssignSpritesToCabinetThemes()
        {
            Sprite cardSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UIDir}/UI_RoundedCard.png");
            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UIDir}/UI_PillButton_Gloss.png");
            Sprite badgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{UIDir}/UI_BadgePill.png");

            string[] guids = AssetDatabase.FindAssets("t:CabinetTheme");
            for (int i = 0; i < guids.Length; i++)
            {
                string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                var theme = AssetDatabase.LoadAssetAtPath<ClawMachine.Data.CabinetTheme>(p);
                if (theme != null)
                {
                    bool changed = false;
                    if (theme.frameBorder == null) { theme.frameBorder = cardSprite; changed = true; }
                    if (theme.buttonBackground == null) { theme.buttonBackground = buttonSprite; changed = true; }
                    if (theme.headerBanner == null) { theme.headerBanner = cardSprite; changed = true; }
                    if (theme.rivetBulb == null) { theme.rivetBulb = badgeSprite; changed = true; }
                    if (changed) EditorUtility.SetDirty(theme);
                }
            }
        }

        private static void CopyAndImportFile(string src, string dst, Vector4 border)
        {
            if (File.Exists(src))
            {
                File.Copy(src, dst, true);
                AssetDatabase.ImportAsset(dst, ImportAssetOptions.ForceUpdate);
                TextureImporter importer = AssetImporter.GetAtPath(dst) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spriteBorder = border;
                    importer.alphaIsTransparency = true;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.mipmapEnabled = false;
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
            }
        }

        // =====================================================================
        // PARTICLE SPRITE GENERATORS
        // =====================================================================

        private static void GenerateSparkleStar(string path, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) / center;
                    float dy = Mathf.Abs(y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Cross spikes
                    float spikeH = Mathf.Pow(Mathf.Clamp01(1f - dx), 4.5f) * Mathf.Pow(Mathf.Clamp01(1f - dy * 4.5f), 1.5f);
                    float spikeV = Mathf.Pow(Mathf.Clamp01(1f - dy), 4.5f) * Mathf.Pow(Mathf.Clamp01(1f - dx * 4.5f), 1.5f);
                    // Core circular glow
                    float core = Mathf.Clamp01(1f - dist * 2.2f);
                    core = core * core;

                    float alpha = Mathf.Clamp01(spikeH + spikeV + core);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            SaveAndConfigureSprite(tex, path, Vector4.zero);
        }

        private static void GenerateSoftPuff(string path, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Smooth Gaussian-like decay
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    alpha = alpha * alpha;

                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            SaveAndConfigureSprite(tex, path, Vector4.zero);
        }

        private static void GenerateCoinParticle(string path, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > 1.0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Outer dark border stroke
                    if (dist > 0.90f)
                    {
                        float a = Mathf.Clamp01((1.0f - dist) / 0.10f);
                        tex.SetPixel(x, y, new Color(0.65f, 0.42f, 0.05f, a));
                    }
                    // Raised outer coin rim
                    else if (dist > 0.72f)
                    {
                        float rimShine = 0.5f + 0.5f * (dy * 0.6f + dx * 0.4f);
                        Color rimCol = Color.Lerp(new Color(1.0f, 0.82f, 0.15f), new Color(1.0f, 0.96f, 0.65f), rimShine);
                        tex.SetPixel(x, y, rimCol);
                    }
                    // Inner groove
                    else if (dist > 0.66f)
                    {
                        tex.SetPixel(x, y, new Color(0.78f, 0.55f, 0.10f, 1f));
                    }
                    // Inner coin face + embossed star
                    else
                    {
                        float faceShine = 0.5f + 0.5f * (-dy * 0.5f + dx * 0.3f);
                        Color faceCol = Color.Lerp(new Color(0.95f, 0.74f, 0.10f), new Color(1.0f, 0.88f, 0.35f), faceShine);

                        // Embossed center star
                        float sdx = Mathf.Abs(dx);
                        float sdy = Mathf.Abs(dy);
                        if ((sdx < 0.22f && sdy < 0.07f) || (sdy < 0.22f && sdx < 0.07f) || (sdx < 0.14f && sdy < 0.14f))
                        {
                            faceCol = Color.Lerp(faceCol, Color.white, 0.65f);
                        }
                        tex.SetPixel(x, y, faceCol);
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, Vector4.zero);
        }

        private static void GenerateConfettiRibbon(string path, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / (width - 1);
                    float edgeX = Mathf.Min(nx, 1f - nx) * 2f;
                    float edgeY = Mathf.Min(ny, 1f - ny) * 2f;
                    float edge = Mathf.Clamp01(Mathf.Min(edgeX * 4f, edgeY * 4f));

                    // Gentle satin highlight down middle
                    float shine = 1f - Mathf.Abs(nx - 0.5f) * 1.2f;
                    Color c = Color.Lerp(new Color(0.85f, 0.85f, 0.85f), Color.white, shine);
                    c.a = edge;
                    tex.SetPixel(x, y, c);
                }
            }
            SaveAndConfigureSprite(tex, path, Vector4.zero);
        }

        private static void GenerateGlowRing(string path, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    // Ring band centered at dist = 0.70 with thickness 0.18
                    float ringDist = Mathf.Abs(dist - 0.70f);
                    float alpha = Mathf.Clamp01(1f - (ringDist / 0.18f));
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            SaveAndConfigureSprite(tex, path, Vector4.zero);
        }

        // =====================================================================
        // UI SPRITE GENERATORS (With 9-Slice Borders)
        // =====================================================================

        private static void GenerateRoundedCard(string path, int width, int height, float radius, Vector4 border)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float d = DistanceToRoundedBox(x, y, width, height, radius);

                    if (d > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - d);

                        // Subtle dark gradient background + 2px top bevel highlight
                        float grad = Mathf.Lerp(0.92f, 1.08f, ny);
                        Color baseCol = new Color(0.13f * grad, 0.15f * grad, 0.20f * grad, alpha);

                        // Inner border stroke (1.8px width)
                        if (d > -2.2f && d <= 0.5f)
                        {
                            float borderAlpha = Mathf.Clamp01(1f - Mathf.Abs(d + 1.0f) / 1.2f);
                            Color strokeCol = Color.Lerp(new Color(0.35f, 0.40f, 0.52f, alpha), new Color(0.60f, 0.70f, 0.88f, alpha), ny);
                            baseCol = Color.Lerp(baseCol, strokeCol, borderAlpha * 0.85f);
                        }

                        tex.SetPixel(x, y, baseCol);
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, border);
        }

        private static void GeneratePillButton(string path, int width, int height, float radius, bool pressed, Vector4 border)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float d = DistanceToRoundedBox(x, y, width, height, radius);

                    if (d > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - d);

                        // Base gradient
                        float grad = pressed ? Mathf.Lerp(0.75f, 0.95f, ny) : Mathf.Lerp(0.85f, 1.15f, ny);
                        Color col = new Color(grad, grad, grad, alpha);

                        // Top half glossy sheen (unpressed)
                        if (!pressed && ny > 0.48f && d < -2.0f)
                        {
                            float sheen = Mathf.Clamp01((ny - 0.48f) / 0.52f) * 0.35f;
                            col += new Color(sheen, sheen, sheen, 0f);
                        }

                        // Embossed border stroke
                        if (d > -2.5f)
                        {
                            Color stroke = pressed ? new Color(0.4f, 0.4f, 0.4f, alpha) : (ny > 0.5f ? new Color(1.2f, 1.2f, 1.2f, alpha) : new Color(0.5f, 0.5f, 0.5f, alpha));
                            col = Color.Lerp(col, stroke, 0.45f);
                        }

                        tex.SetPixel(x, y, col);
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, border);
        }

        private static void GenerateBadgePill(string path, int width, int height, float radius, Vector4 border)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float d = DistanceToRoundedBox(x, y, width, height, radius);

                    if (d > 0.5f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - d);
                        float grad = Mathf.Lerp(0.88f, 1.12f, ny);
                        Color col = new Color(0.18f * grad, 0.22f * grad, 0.30f * grad, alpha);

                        // Outer glowing rim
                        if (d > -2.0f)
                        {
                            Color rim = Color.Lerp(new Color(0.40f, 0.75f, 1.0f, alpha), new Color(0.70f, 0.90f, 1.0f, alpha), ny);
                            col = Color.Lerp(col, rim, 0.70f);
                        }

                        tex.SetPixel(x, y, col);
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, border);
        }

        private static void GenerateProgressTrack(string path, int width, int height, Vector4 border)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float radius = height * 0.45f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float d = DistanceToRoundedBox(x, y, width, height, radius);
                    if (d > 0.5f) tex.SetPixel(x, y, Color.clear);
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - d);
                        Color track = new Color(0.08f, 0.09f, 0.13f, alpha * 0.95f);
                        if (d > -1.5f) track = Color.Lerp(track, new Color(0.25f, 0.28f, 0.38f, alpha), 0.5f);
                        tex.SetPixel(x, y, track);
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, border);
        }

        private static void GenerateProgressFill(string path, int width, int height, Vector4 border)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float radius = height * 0.42f;

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float d = DistanceToRoundedBox(x, y, width, height, radius);
                    if (d > 0.5f) tex.SetPixel(x, y, Color.clear);
                    else
                    {
                        float alpha = Mathf.Clamp01(0.5f - d);
                        Color fill = Color.Lerp(new Color(0.25f, 0.75f, 1.0f, alpha), new Color(0.55f, 0.95f, 1.0f, alpha), ny);
                        if (ny > 0.55f && d < -1.5f)
                        {
                            fill += new Color(0.25f, 0.25f, 0.25f, 0f);
                        }
                        tex.SetPixel(x, y, fill);
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, border);
        }

        private static void GenerateCoinIcon(string path, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    if (dist > 1.0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float a = Mathf.Clamp01((1.0f - dist) / 0.05f);
                    if (dist > 0.88f)
                    {
                        tex.SetPixel(x, y, new Color(0.55f, 0.35f, 0.05f, a));
                    }
                    else if (dist > 0.72f)
                    {
                        float shine = 0.5f + 0.5f * (dy * 0.7f + dx * 0.3f);
                        Color rim = Color.Lerp(new Color(1.0f, 0.82f, 0.15f), new Color(1.0f, 0.98f, 0.70f), shine);
                        tex.SetPixel(x, y, new Color(rim.r, rim.g, rim.b, a));
                    }
                    else if (dist > 0.65f)
                    {
                        tex.SetPixel(x, y, new Color(0.72f, 0.48f, 0.08f, a));
                    }
                    else
                    {
                        float faceShine = 0.5f + 0.5f * (-dy * 0.5f + dx * 0.4f);
                        Color face = Color.Lerp(new Color(0.96f, 0.76f, 0.12f), new Color(1.0f, 0.90f, 0.45f), faceShine);
                        // Embossed coin symbol
                        if (Mathf.Abs(dx) < 0.16f && Mathf.Abs(dy) < 0.28f)
                        {
                            face = Color.Lerp(face, Color.white, 0.7f);
                        }
                        tex.SetPixel(x, y, new Color(face.r, face.g, face.b, a));
                    }
                }
            }
            SaveAndConfigureSprite(tex, path, Vector4.zero);
        }

        private static float DistanceToRoundedBox(float px, float py, float width, float height, float radius)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float cx = Mathf.Abs(px - halfW) - (halfW - radius);
            float cy = Mathf.Abs(py - halfH) - (halfH - radius);

            float outside = Mathf.Sqrt(Mathf.Max(cx, 0f) * Mathf.Max(cx, 0f) + Mathf.Max(cy, 0f) * Mathf.Max(cy, 0f));
            float inside = Mathf.Min(Mathf.Max(cx, cy), 0f);
            return outside + inside - radius;
        }

        private static void SaveAndConfigureSprite(Texture2D tex, string path, Vector4 border)
        {
            byte[] pngData = tex.EncodeToPNG();
            File.WriteAllBytes(path, pngData);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spriteBorder = border;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
