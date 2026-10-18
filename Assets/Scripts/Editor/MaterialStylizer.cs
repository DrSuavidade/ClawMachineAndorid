#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace ClawMachine.Editor
{
    [InitializeOnLoad]
    public static class MaterialStylizer
    {
        private const string PrefKeyVersion = "ClawMachine_ShaderRemake_v2";

        static MaterialStylizer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!SessionState.GetBool(PrefKeyVersion, false))
                {
                    SessionState.SetBool(PrefKeyVersion, true);
                    UpgradeAllMaterialsToToonOutline();
                }
            };
        }

        [MenuItem("ClawMachine/Stylize All Materials (Toon & Outline)")]
        public static void UpgradeAllMaterialsToToonOutline()
        {
            Shader toonShader = Shader.Find("ClawMachine/ToonOutline");
            Shader glassShader = Shader.Find("ClawMachine/ArcadeGlass");
            if (toonShader == null)
            {
                Debug.LogError("[MaterialStylizer] Could not find shader 'ClawMachine/ToonOutline'!");
                return;
            }

            string[] matGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
            int upgraded = 0;

            foreach (string guid in matGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;

                if (path.IndexOf("Glass", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (glassShader != null)
                    {
                        mat.shader = glassShader;
                        mat.SetColor("_Color", new Color(0.78f, 0.94f, 1.0f, 0.06f));
                        mat.SetColor("_RimColor", new Color(0.55f, 0.90f, 1.0f, 0.42f));
                        mat.SetFloat("_FresnelPower", 3.5f);
                        mat.SetFloat("_SweepIntensity", 0.14f);
                        EditorUtility.SetDirty(mat);
                        upgraded++;
                    }
                    continue;
                }

                if (path.IndexOf("Particle", StringComparison.OrdinalIgnoreCase) >= 0) continue;

                StyleMaterialWithToon(mat, toonShader);
                EditorUtility.SetDirty(mat);
                upgraded++;
            }

            // Also upgrade any in-scene materials (e.g. Claw, Cabinet Glass, Floor)
            Renderer[] sceneRenderers = UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < sceneRenderers.Length; i++)
            {
                Renderer r = sceneRenderers[i];
                if (r == null || r is ParticleSystemRenderer || r is LineRenderer) continue;

                Material m = r.sharedMaterial;
                if (m == null) continue;

                if (r.name.IndexOf("Glass", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    m.name.IndexOf("Glass", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (glassShader != null && m.shader != glassShader)
                    {
                        m.shader = glassShader;
                        m.SetColor("_Color", new Color(0.78f, 0.94f, 1.0f, 0.06f));
                        m.SetColor("_RimColor", new Color(0.55f, 0.90f, 1.0f, 0.42f));
                        EditorUtility.SetDirty(m);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[MaterialStylizer] Upgraded {upgraded} materials to Production Stylized Toon & ArcadeGlass!");
        }

        public static void StyleMaterialWithToon(Material mat, Shader toonShader)
        {
            if (mat == null || toonShader == null) return;

            Color origCol = mat.HasProperty("_Color") ? mat.color : (mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white);
            float origMetal = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
            float origSmooth = mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : (mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.45f);
            Color origEmission = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;

            string n = mat.name;
            bool isMetallic = origMetal > 0.35f
                || ContainsAny(n, "Gold", "Silver", "Metal", "Chrome", "Token", "Visor", "Crown", "SwordBlade", "FloppyShutter", "Shield", "Armor", "Anchor");
            bool isEmissive = origEmission.maxColorComponent > 0.05f
                || ContainsAny(n, "Neon", "Glow", "Magma", "Nebula", "Crystal", "Screen", "Marquee", "Bio", "Toxic", "Pearl", "Potion");
            bool isEnvironment = ContainsAny(n, "Env_", "Floor", "Wall", "Shelf", "Molding");

            mat.shader = toonShader;
            mat.SetColor("_Color", origCol);
            mat.SetColor("_BaseColor", origCol);
            mat.SetColor("_ShadowTint", new Color(0.72f, 0.68f, 0.86f, 1f));
            mat.SetFloat("_CelThreshold", 0.42f);
            mat.SetFloat("_CelSoftness", 0.045f);
            mat.SetFloat("_AmbientBoost", isEnvironment ? 0.50f : 0.46f);

            // Metallic & Specular tuning
            mat.SetFloat("_Metallic", isMetallic ? Mathf.Max(origMetal, 0.82f) : 0.04f);
            mat.SetFloat("_Glossiness", isMetallic ? Mathf.Max(origSmooth, 0.80f) : (isEnvironment ? 0.25f : 0.52f));
            mat.SetFloat("_Smoothness", isMetallic ? Mathf.Max(origSmooth, 0.80f) : (isEnvironment ? 0.25f : 0.52f));
            mat.SetFloat("_SpecThreshold", isMetallic ? 0.88f : 0.93f);
            mat.SetFloat("_SpecIntensity", isMetallic ? 0.75f : (isEnvironment ? 0.15f : 0.42f));

            // Rim Pop (stronger on prizes, subtle on room walls)
            mat.SetColor("_RimColor", Color.white);
            mat.SetFloat("_RimPower", 3.2f);
            mat.SetFloat("_RimIntensity", isEnvironment ? 0.08f : 0.32f);

            // HDR Emission for glowing arcade/sci-fi/magic materials
            if (isEmissive)
            {
                Color hdrEmit = origEmission.maxColorComponent > 0.05f ? origEmission : (origCol * 1.45f);
                mat.SetColor("_EmissionColor", hdrEmit);
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
            }

            // Adaptive Outline (crisp on toys, thinner on background room walls)
            mat.SetColor("_OutlineColor", new Color(0.10f, 0.09f, 0.14f, 1f));
            mat.SetFloat("_OutlineColorTint", 0.55f);
            mat.SetFloat("_OutlineWidth", isEnvironment ? 0.008f : 0.015f);
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (text.IndexOf(keywords[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }
    }
}
#endif
