using System.Collections;
using System.IO;
using UnityEngine;
using ClawMachine.UI;

namespace ClawMachine.DebugTools
{
    public class UIScreenshotHarness : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            bool capture = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-capture-ui-screens")
                {
                    capture = true;
                    break;
                }
            }

            if (capture)
            {
                var go = new GameObject("UIScreenshotHarness");
                go.AddComponent<UIScreenshotHarness>();
                DontDestroyOnLoad(go);
            }
        }

        private IEnumerator Start()
        {
            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Screenshots"));
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            Screen.SetResolution(720, 1280, FullScreenMode.Windowed);
            Debug.Log($"[UIScreenshotHarness] Output directory: {outDir}");

            // 1. Wait for resolution change and layout to settle
            yield return new WaitForSeconds(0.8f);

            // Screenshot 1: Main HUD & Console
            string hudPath = Path.Combine(outDir, "01_hud_console.png");
            CapturePortraitScreenshot(hudPath);
            Debug.Log($"[UIScreenshotHarness] Captured: {hudPath}");
            yield return new WaitForSeconds(0.4f);

            // Screenshot 2: Settings Modal
            var settings = FindFirstObjectByType<SettingsModalUI>(FindObjectsInactive.Include);
            if (settings != null)
            {
                settings.OpenModal();
                yield return new WaitForSeconds(0.4f);
                string settingsPath = Path.Combine(outDir, "02_settings_modal.png");
                CapturePortraitScreenshot(settingsPath);
                Debug.Log($"[UIScreenshotHarness] Captured: {settingsPath}");
                yield return new WaitForSeconds(0.3f);
                settings.CloseModal();
                yield return new WaitForSeconds(0.3f);
            }

            // Screenshot 3: Progression Modal - Upgrades Tab
            var progress = FindFirstObjectByType<ProgressModalUI>(FindObjectsInactive.Include);
            if (progress != null)
            {
                progress.OpenModal();
                progress.SwitchTab(0);
                yield return new WaitForSeconds(0.4f);
                string upgPath = Path.Combine(outDir, "03_progress_upgrades.png");
                CapturePortraitScreenshot(upgPath);
                Debug.Log($"[UIScreenshotHarness] Captured: {upgPath}");
                yield return new WaitForSeconds(0.3f);

                // Screenshot 4: Progression Modal - Rewards Tab
                progress.SwitchTab(1);
                yield return new WaitForSeconds(0.4f);
                string rewPath = Path.Combine(outDir, "04_progress_rewards.png");
                CapturePortraitScreenshot(rewPath);
                Debug.Log($"[UIScreenshotHarness] Captured: {rewPath}");
                yield return new WaitForSeconds(0.3f);
                progress.CloseModal();
                yield return new WaitForSeconds(0.3f);
            }

            // Screenshot 5: Collection Modal
            var collection = FindFirstObjectByType<CollectionUI>(FindObjectsInactive.Include);
            if (collection != null)
            {
                collection.ToggleModal();
                yield return new WaitForSeconds(0.4f);
                string colPath = Path.Combine(outDir, "05_collection_modal.png");
                CapturePortraitScreenshot(colPath);
                Debug.Log($"[UIScreenshotHarness] Captured: {colPath}");
                yield return new WaitForSeconds(0.3f);
                collection.CloseModal();
                yield return new WaitForSeconds(0.3f);
            }

            Debug.Log("[UIScreenshotHarness] Screenshot capture sequence finished successfully!");
            yield return new WaitForSeconds(0.3f);
            Application.Quit(0);
        }

        private void CapturePortraitScreenshot(string path, int width = 1080, int height = 1920)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Camera cam = Camera.main;
            if (cam == null || canvas == null)
            {
                ScreenCapture.CaptureScreenshot(path);
                return;
            }

            RenderMode prevMode = canvas.renderMode;
            Camera prevCam = canvas.worldCamera;
            float prevDist = canvas.planeDistance;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = cam.nearClipPlane + 0.15f;
            Canvas.ForceUpdateCanvases();

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevRT = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(path, tex.EncodeToPNG());

            cam.targetTexture = prevRT;
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            DestroyImmediate(tex);

            canvas.renderMode = prevMode;
            canvas.worldCamera = prevCam;
            canvas.planeDistance = prevDist;
            Canvas.ForceUpdateCanvases();
        }
    }
}
