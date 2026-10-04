#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ClawMachine.Editor
{
    public static class MaterialStylizer
    {
        [MenuItem("ClawMachine/Stylize All Materials (Toon & Outline)")]
        public static void UpgradeAllMaterialsToToonOutline()
        {
            Shader toonShader = Shader.Find("ClawMachine/ToonOutline");
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
                if (path.Contains("CabinetGlass")) continue; // Keep glass transparent!

                Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    Color origCol = mat.HasProperty("_Color") ? mat.color : (mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white);
                    mat.shader = toonShader;
                    mat.SetColor("_Color", origCol);
                    mat.SetColor("_OutlineColor", new Color(0.10f, 0.10f, 0.14f, 1f));
                    mat.SetFloat("_OutlineWidth", 0.020f);
                    mat.SetFloat("_CelThreshold", 0.40f);
                    mat.SetFloat("_CelSoftness", 0.035f);
                    mat.SetFloat("_SpecThreshold", 0.94f);
                    mat.SetFloat("_AmbientBoost", 0.42f);
                    EditorUtility.SetDirty(mat);
                    upgraded++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MaterialStylizer] Successfully upgraded {upgraded} materials to Stylized Toon Outline!");
        }
    }
}
#endif
