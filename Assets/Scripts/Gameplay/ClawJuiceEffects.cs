using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Gameplay
{
    public class ClawJuiceEffects : MonoBehaviour
    {
        private static ClawJuiceEffects instance;
        public static ClawJuiceEffects Instance => instance;

        private ParticleSystem confettiPS;
        private ParticleSystem coinShowerPS;
        private ParticleSystem dustPuffPS;
        private ParticleSystem strainSparksPS;
        private readonly List<Material> createdMaterials = new List<Material>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            BuildConfettiParticleSystem();
            BuildCoinShowerParticleSystem();
            BuildDustPuffParticleSystem();
            BuildStrainSparksParticleSystem();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < createdMaterials.Count; i++)
            {
                if (createdMaterials[i] != null) Destroy(createdMaterials[i]);
            }
            createdMaterials.Clear();
            if (instance == this) instance = null;
        }

        public void PlayWinCelebration(Vector3 position)
        {
            Vector3 worldPos = position;
            // Screen-space UI coordinates detection (Canvas ScreenSpaceOverlay uses coordinates in the hundreds)
            if (Mathf.Abs(position.x) > 40f || Mathf.Abs(position.y) > 40f)
            {
                Camera cam = Camera.main ?? FindFirstObjectByType<Camera>();
                if (cam != null)
                {
                    Vector3 vp = cam.ScreenToViewportPoint(position);
                    vp.x = Mathf.Clamp(vp.x, 0.15f, 0.85f);
                    vp.y = Mathf.Clamp(vp.y, 0.20f, 0.80f);
                    vp.z = 2.4f; // in front of camera
                    worldPos = cam.ViewportToWorldPoint(vp);
                }
                else
                {
                    worldPos = new Vector3(0f, 1.8f, -4.5f);
                }
            }

            if (confettiPS != null)
            {
                confettiPS.transform.position = worldPos + Vector3.up * 0.5f;
                confettiPS.Emit(60);
            }

            if (coinShowerPS != null)
            {
                coinShowerPS.transform.position = worldPos + new Vector3(0f, -0.2f, 0f);
                coinShowerPS.Emit(25);
            }
        }

        public void PlayDustPuff(Vector3 position)
        {
            if (dustPuffPS != null)
            {
                dustPuffPS.transform.position = position;
                dustPuffPS.Emit(12);
            }
        }

        public void PlayPrizeNudgeFX(Vector3 position)
        {
            if (dustPuffPS != null)
            {
                dustPuffPS.transform.position = position;
                dustPuffPS.Emit(8);
            }
            if (strainSparksPS != null)
            {
                strainSparksPS.transform.position = position + Vector3.up * 0.1f;
                strainSparksPS.Emit(6);
            }
        }

        public void PlayStrainSparks(Vector3 position)
        {
            if (strainSparksPS != null)
            {
                strainSparksPS.transform.position = position;
                strainSparksPS.Emit(8);
            }
        }

        public void AttachRarityShimmer(GameObject target, PrizeRarity rarity)
        {
            if (target == null || rarity == PrizeRarity.Normal) return;
            if (target.transform.Find("RarityShimmer") != null) return;

            GameObject go = new GameObject("RarityShimmer");
            go.transform.SetParent(target.transform, false);
            go.transform.localPosition = Vector3.zero;

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.playOnAwake = true;
            main.loop = true;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.16f);
            main.gravityModifier = -0.05f;

            Color col = rarity == PrizeRarity.Secret 
                ? new Color(1.0f, 0.88f, 0.25f, 0.85f) 
                : new Color(0.92f, 0.40f, 1.0f, 0.85f);
            main.startColor = col;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            var emission = ps.emission;
            emission.rateOverTime = 3.5f;

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(0.4f, 1f);
            curve.AddKey(1f, 0f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(col, GetOrCreateSparkleTexture());
        }

        private void BuildConfettiParticleSystem()
        {
            GameObject go = new GameObject("ConfettiFX");
            go.transform.parent = transform;
            confettiPS = go.AddComponent<ParticleSystem>();
            confettiPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = confettiPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 1.0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.10f, 0.20f);
            main.gravityModifier = 0.55f;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);

            // Confetti multi-color palette
            var col = confettiPS.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.25f, 0.45f), 0f), new GradientColorKey(new Color(0.2f, 0.85f, 1f), 0.5f), new GradientColorKey(new Color(1f, 0.92f, 0.25f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            // Twirl / flutter rotation
            var rot = confettiPS.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-3.5f, 3.5f);

            // Scale pop-in then gentle flutter
            var sol = confettiPS.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.4f);
            curve.AddKey(0.15f, 1.0f);
            curve.AddKey(0.8f, 0.9f);
            curve.AddKey(1.0f, 0.3f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var shape = confettiPS.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.35f;

            var emission = confettiPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(Color.white, GetOrCreateConfettiTexture());
        }

        private void BuildCoinShowerParticleSystem()
        {
            GameObject go = new GameObject("CoinShowerFX");
            go.transform.parent = transform;
            coinShowerPS = go.AddComponent<ParticleSystem>();
            coinShowerPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = coinShowerPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, 1.6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.2f, 6.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.14f, 0.24f);
            main.gravityModifier = 1.35f;
            main.startColor = Color.white;

            // Spinning coins
            var rot = coinShowerPS.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-6.0f, 6.0f);

            var sol = coinShowerPS.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.6f);
            curve.AddKey(0.12f, 1.0f);
            curve.AddKey(0.85f, 1.0f);
            curve.AddKey(1.0f, 0.6f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var shape = coinShowerPS.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;

            var emission = coinShowerPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(Color.white, GetOrCreateCoinTexture());
        }

        private void BuildDustPuffParticleSystem()
        {
            GameObject go = new GameObject("DustPuffFX");
            go.transform.parent = transform;
            dustPuffPS = go.AddComponent<ParticleSystem>();
            dustPuffPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = dustPuffPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.75f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.0f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f, 0.32f);
            main.gravityModifier = -0.06f; // gently float upward
            main.startColor = new Color(1f, 1f, 1f, 0.45f);

            // Expansion & soft alpha fade
            var sol = dustPuffPS.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.4f);
            sizeCurve.AddKey(0.4f, 1.0f);
            sizeCurve.AddKey(1.0f, 1.4f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            var col = dustPuffPS.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.45f, 0f), new GradientAlphaKey(0.35f, 0.3f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = grad;

            var shape = dustPuffPS.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            var emission = dustPuffPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(new Color(0.92f, 0.94f, 1f, 0.5f), GetOrCreateSoftPuffTexture());
        }

        private void BuildStrainSparksParticleSystem()
        {
            GameObject go = new GameObject("StrainSparksFX");
            go.transform.parent = transform;
            strainSparksPS = go.AddComponent<ParticleSystem>();
            strainSparksPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = strainSparksPS.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.3f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.35f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.6f, 3.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
            main.gravityModifier = 0.85f;
            main.startColor = new Color(1.0f, 0.88f, 0.35f);

            var sol = strainSparksPS.sizeOverLifetime;
            sol.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1.0f);
            curve.AddKey(0.6f, 0.7f);
            curve.AddKey(1.0f, 0.0f);
            sol.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var shape = strainSparksPS.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var emission = strainSparksPS.emission;
            emission.enabled = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(new Color(1f, 0.95f, 0.5f), GetOrCreateSparkleTexture());
        }

        private Material CreateParticleMaterial(Color tint, Texture2D texture)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            Material m = new Material(s);
            m.color = tint;
            if (texture != null)
            {
                m.mainTexture = texture;
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", texture);
            }
            createdMaterials.Add(m);
            return m;
        }

        // =====================================================================
        // PROCEDURAL PARTICLE TEXTURES (FALLBACK & ZERO ALLOC RUNTIME ASSETS)
        // =====================================================================

        private static Texture2D cachedConfettiTex;
        private static Texture2D cachedCoinTex;
        private static Texture2D cachedPuffTex;
        private static Texture2D cachedSparkleTex;

        private static Texture2D GetOrCreateConfettiTexture()
        {
            if (cachedConfettiTex != null) return cachedConfettiTex;
            cachedConfettiTex = Resources.Load<Texture2D>("Particles/Particle_ConfettiRibbon");
            if (cachedConfettiTex != null) return cachedConfettiTex;

            int w = 24, h = 48;
            cachedConfettiTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cachedConfettiTex.wrapMode = TextureWrapMode.Clamp;
            cachedConfettiTex.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool edgeX = x == 0 || x == w - 1;
                    bool edgeY = y == 0 || y == h - 1;
                    if ((x == 0 || x == w - 1) && (y <= 1 || y >= h - 2))
                    {
                        cachedConfettiTex.SetPixel(x, y, Color.clear);
                    }
                    else if (edgeX || edgeY)
                    {
                        cachedConfettiTex.SetPixel(x, y, new Color(1f, 1f, 1f, 0.6f));
                    }
                    else
                    {
                        float gloss = 0.85f + (float)x / w * 0.15f;
                        cachedConfettiTex.SetPixel(x, y, new Color(gloss, gloss, gloss, 1f));
                    }
                }
            }
            cachedConfettiTex.Apply();
            return cachedConfettiTex;
        }

        private static Texture2D GetOrCreateCoinTexture()
        {
            if (cachedCoinTex != null) return cachedCoinTex;
            cachedCoinTex = Resources.Load<Texture2D>("Particles/Particle_Coin");
            if (cachedCoinTex != null) return cachedCoinTex;

            int size = 48;
            cachedCoinTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            cachedCoinTex.wrapMode = TextureWrapMode.Clamp;
            cachedCoinTex.filterMode = FilterMode.Bilinear;
            float c = (size - 1) * 0.5f;
            float r = size * 0.46f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    if (d > r)
                    {
                        cachedCoinTex.SetPixel(x, y, Color.clear);
                    }
                    else if (d > r - 3.5f)
                    {
                        cachedCoinTex.SetPixel(x, y, new Color(0.95f, 0.70f, 0.10f, 1f));
                    }
                    else
                    {
                        float inner = d / (r - 3.5f);
                        Color gold = Color.Lerp(new Color(1f, 0.94f, 0.40f), new Color(1f, 0.78f, 0.15f), inner);
                        cachedCoinTex.SetPixel(x, y, gold);
                    }
                }
            }
            cachedCoinTex.Apply();
            return cachedCoinTex;
        }

        private static Texture2D GetOrCreateSoftPuffTexture()
        {
            if (cachedPuffTex != null) return cachedPuffTex;
            cachedPuffTex = Resources.Load<Texture2D>("Particles/Particle_SoftPuff");
            if (cachedPuffTex != null) return cachedPuffTex;

            int size = 48;
            cachedPuffTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            cachedPuffTex.wrapMode = TextureWrapMode.Clamp;
            cachedPuffTex.filterMode = FilterMode.Bilinear;
            float c = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c;
                    float alpha = Mathf.Clamp01(1f - d);
                    alpha = Mathf.SmoothStep(0f, 1f, alpha);
                    cachedPuffTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
                }
            }
            cachedPuffTex.Apply();
            return cachedPuffTex;
        }

        private static Texture2D GetOrCreateSparkleTexture()
        {
            if (cachedSparkleTex != null) return cachedSparkleTex;
            cachedSparkleTex = Resources.Load<Texture2D>("Particles/Particle_SparkleStar");
            if (cachedSparkleTex != null) return cachedSparkleTex;

            int size = 48;
            cachedSparkleTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            cachedSparkleTex.wrapMode = TextureWrapMode.Clamp;
            cachedSparkleTex.filterMode = FilterMode.Bilinear;
            float c = (size - 1) * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - c) / c;
                    float dy = Mathf.Abs(y - c) / c;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float spikeH = Mathf.Pow(Mathf.Clamp01(1f - dx), 4f) * Mathf.Pow(Mathf.Clamp01(1f - dy * 4f), 1.5f);
                    float spikeV = Mathf.Pow(Mathf.Clamp01(1f - dy), 4f) * Mathf.Pow(Mathf.Clamp01(1f - dx * 4f), 1.5f);
                    float core = Mathf.Pow(Mathf.Clamp01(1f - dist * 2f), 2f);

                    float alpha = Mathf.Clamp01(spikeH + spikeV + core);
                    cachedSparkleTex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            cachedSparkleTex.Apply();
            return cachedSparkleTex;
        }
    }
}
