#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Gameplay;
using ClawMachine.UI;

namespace ClawMachine.Editor
{
    public static class PrefabExporter
    {
        [MenuItem("ClawMachine/Export Core Prefabs")]
        public static void ExportPrefabs()
        {
            EnsurePrefabDirectories();

            // 1. Locate scene objects in current open scene

            // 2. Locate scene objects
            GameObject cabinet = GameObject.Find("Cabinet");
            GameObject clawRig = GameObject.Find("ClawRig");
            GameObject canvas = GameObject.Find("UI Canvas") ?? GameObject.Find("Canvas_ArcadeConsole");

            if (cabinet != null)
            {
                string path = "Assets/Prefabs/Cabinet/Cabinet_Standard.prefab";
                PrefabUtility.SaveAsPrefabAsset(cabinet, path);
                Debug.Log($"[PrefabExporter] Saved: {path}");
            }

            if (clawRig != null)
            {
                string path = "Assets/Prefabs/Claw/ClawAssembly.prefab";
                PrefabUtility.SaveAsPrefabAsset(clawRig, path);
                Debug.Log($"[PrefabExporter] Saved: {path}");
            }

            if (canvas != null)
            {
                string path = "Assets/Prefabs/UI/ArcadeConsole_UI.prefab";
                PrefabUtility.SaveAsPrefabAsset(canvas, path);
                Debug.Log($"[PrefabExporter] Saved: {path}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PrefabExporter] Core Prefabs successfully exported!");
        }

        private static void EnsurePrefabDirectories()
        {
            if (!Directory.Exists("Assets/Prefabs/Cabinet")) Directory.CreateDirectory("Assets/Prefabs/Cabinet");
            if (!Directory.Exists("Assets/Prefabs/Claw")) Directory.CreateDirectory("Assets/Prefabs/Claw");
            if (!Directory.Exists("Assets/Prefabs/UI")) Directory.CreateDirectory("Assets/Prefabs/UI");
            AssetDatabase.Refresh();
        }
    }
}
#endif
