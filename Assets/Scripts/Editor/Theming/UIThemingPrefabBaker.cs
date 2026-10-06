#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.UI.Theming;

namespace ClawMachine.Editor
{
    public static class UIThemingPrefabBaker
    {
        private const string PREFAB_PATH = "Assets/Prefabs/UI/ArcadeConsole_UI.prefab";

        [InitializeOnLoadMethod]
        private static void AutoBakeOnce()
        {
            if (!EditorPrefs.GetBool("ClawMachine_ThemingBaked_v1", false))
            {
                EditorPrefs.SetBool("ClawMachine_ThemingBaked_v1", true);
                EditorApplication.delayCall += BakeThemingToPrefab;
            }
        }

        [MenuItem("ClawMachine/Theme/Bake Themed UI Elements To Prefab")]
        public static void BakeThemingToPrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PREFAB_PATH);
            if (prefabRoot == null)
            {
                Debug.LogError($"[UIThemingPrefabBaker] Could not load prefab at {PREFAB_PATH}");
                return;
            }

            int count = 0;
            var images = prefabRoot.GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                string n = img.gameObject.name;
                ThemeTargetType? targetType = null;

                if (n == "MarqueeRim") targetType = ThemeTargetType.FrameBorder;
                else if (n == "ConsoleRim") targetType = ThemeTargetType.Secondary;
                else if (n == "CarouselStrip") targetType = ThemeTargetType.Secondary;
                else if (n == "Rivet") targetType = ThemeTargetType.RivetBulb;
                else if (n == "DialogCard") targetType = ThemeTargetType.FrameBorder;
                else if (n == "Header" && img.transform.parent != null && img.transform.parent.name.EndsWith("Modal"))
                {
                    targetType = ThemeTargetType.HeaderBanner;
                }
                else if (n == "Btn_PrevMachine" || n == "Btn_NextMachine" || n == "Btn_ConfirmUnlock" || n == "Btn_Awesome" || n == "Btn_SellDuplicates")
                {
                    targetType = ThemeTargetType.ButtonBackground;
                }
                else if (n == "UnlockModal" || n == "CollectionModal" || n == "ProgressModal" || n == "SettingsModal" || n == "CelebrationModal")
                {
                    targetType = ThemeTargetType.Surface;
                }

                if (targetType.HasValue)
                {
                    var themed = img.gameObject.GetComponent<ThemedUIElement>();
                    if (themed == null)
                    {
                        themed = img.gameObject.AddComponent<ThemedUIElement>();
                    }
                    themed.TargetType = targetType.Value;
                    count++;
                }
            }

            if (prefabRoot.GetComponent<UIThemeManager>() == null)
            {
                prefabRoot.AddComponent<UIThemeManager>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PREFAB_PATH);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UIThemingPrefabBaker] Successfully baked {count} ThemedUIElements into {PREFAB_PATH}!");
        }
    }
}
#endif
