using System;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class PrizeWinModalUI : MonoBehaviour
    {
        public static PrizeWinModalUI Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text prizeNameText;
        [SerializeField] private Text rarityBadgeText;
        [SerializeField] private Image rarityBadgeBg;
        [SerializeField] private Text rewardDetailText;
        [SerializeField] private Button claimButton;

        private Coroutine autoDismissRoutine;
        private Coroutine bounceRoutine;
        private Coroutine dropRoutine;
        private bool isDropAnimationDone = false;

        // 3D Offscreen Preview Rig (Renders 3D model into RenderTexture for pure 2D Canvas display)
        private RenderTexture previewRenderTexture;
        private Camera previewCamera;
        private GameObject previewRigRoot;
        private GameObject currentPreviewModel;
        private GameObject currentPreviewRoot;
        private RawImage previewRawImage;

        private void Awake()
        {
            Instance = this;
            AutoFindRefsIfNull();
            if (claimButton != null)
            {
                claimButton.onClick.RemoveAllListeners();
                claimButton.onClick.AddListener(Dismiss);
            }
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
        }

        private void OnDestroy()
        {
            if (previewRenderTexture != null)
            {
                previewRenderTexture.Release();
                Destroy(previewRenderTexture);
            }
            if (previewRigRoot != null)
            {
                Destroy(previewRigRoot);
            }
        }

        public void ShowWin(Prize prize)
        {
            if (prize == null) return;
            AutoFindRefsIfNull();
            if (modalPanel == null) return;

            string pName = (prize.Definition != null && !string.IsNullOrEmpty(prize.Definition.displayName)) 
                ? prize.Definition.displayName 
                : prize.name;
            string pId = (prize.Definition != null && !string.IsNullOrEmpty(prize.Definition.id)) 
                ? prize.Definition.id 
                : prize.name;
            PrizeRarity rarity = prize.Rarity;
            
            bool isNew = true;
            int dupVal = prize.Definition != null ? prize.Definition.duplicateCoinValue : 20;

            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                isNew = coll.GetPrizeCount(pId) <= 1;
            }

            if (prizeNameText != null) prizeNameText.text = pName.ToUpper();
            if (rarityBadgeText != null) rarityBadgeText.text = rarity.ToString().ToUpper();
            
            Color rarityColor = rarity switch
            {
                PrizeRarity.Secret => new Color(1.0f, 0.82f, 0.15f),
                PrizeRarity.Rare => new Color(0.85f, 0.35f, 0.95f),
                _ => new Color(0.25f, 0.85f, 1.0f)
            };

            if (rarityBadgeBg != null) rarityBadgeBg.color = rarityColor;
            if (rarityBadgeText != null) rarityBadgeText.color = Color.white;

            if (rewardDetailText != null)
            {
                rewardDetailText.text = isNew 
                    ? "★ NEW DISCOVERY ADDED TO COLLECTION! ★" 
                    : $"DUPLICATE PRIZE SOLD (+{dupVal} 🪙)";
            }

            modalPanel.SetActive(true);

            // 1. Audio & VFX Celebration Juice
            var audio = ServiceLocator.Get<IAudioService>();
            if (audio != null)
            {
                audio.PlayWin();
                audio.TriggerHapticSuccess();
            }
            ClawJuiceEffects.Instance?.PlayWinCelebration(Vector3.up * 1.5f);

            // 2. Punch Scale Bounce Animation
            if (bounceRoutine != null) StopCoroutine(bounceRoutine);
            bounceRoutine = StartCoroutine(BounceRoutine());

            if (previewRigRoot != null) previewRigRoot.SetActive(true);

            // 3. 3D Animated Prize Chute Drop via RenderTexture
            SpawnPreviewModel(prize.Definition != null ? prize.Definition.prefab : null);
        }

        private void Update()
        {
            if (currentPreviewRoot != null && isDropAnimationDone)
            {
                currentPreviewRoot.transform.Rotate(Vector3.up, 38f * Time.deltaTime, Space.World);
            }

#if UNITY_EDITOR
            // Debug hotkey: Press 'P' in Play Mode to test the 3D rotating preview modal anytime
            if (Input.GetKeyDown(KeyCode.P))
            {
                var prize = FindFirstObjectByType<Prize>();
                if (prize != null)
                {
                    ShowWin(prize);
                }
                else
                {
                    Debug.Log("[PrizeWinModalUI] Press P: No prize found in scene to preview.");
                }
            }
#endif
        }

        private void SetupPreviewRig()
        {
            if (previewRigRoot != null) return;

            // Isolated position far below scene
            Vector3 rigPos = new Vector3(0f, -250f, 0f);
            previewRigRoot = new GameObject("PrizePreviewRig");
            previewRigRoot.transform.position = rigPos;

            previewRenderTexture = new RenderTexture(384, 384, 16, RenderTextureFormat.ARGB32)
            {
                name = "RT_PrizeWinPreview"
            };

            GameObject camObj = new GameObject("PreviewCamera");
            camObj.transform.parent = previewRigRoot.transform;
            camObj.transform.localPosition = new Vector3(0f, 0.1f, -1.8f);
            camObj.transform.localRotation = Quaternion.identity;

            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.04f, 0.06f, 0.10f, 0f); // Transparent
            previewCamera.fieldOfView = 32f;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 10f;
            previewCamera.targetTexture = previewRenderTexture;
            previewCamera.enabled = false;

            // Local Point lights with range 3.5m so they NEVER illuminate the main scene at Y=0!
            GameObject lightObj = new GameObject("PreviewLight");
            lightObj.transform.parent = previewRigRoot.transform;
            lightObj.transform.localPosition = new Vector3(0.7f, 1.2f, -1.2f);
            Light pLight = lightObj.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.range = 3.5f;
            pLight.color = new Color(1f, 0.98f, 0.92f);
            pLight.intensity = 2.5f;

            // Rim / fill point light
            GameObject rimLightObj = new GameObject("PreviewRimLight");
            rimLightObj.transform.parent = previewRigRoot.transform;
            rimLightObj.transform.localPosition = new Vector3(-0.7f, -0.5f, -0.9f);
            Light rLight = rimLightObj.AddComponent<Light>();
            rLight.type = LightType.Point;
            rLight.range = 3.5f;
            rLight.color = new Color(0.40f, 0.80f, 1.0f);
            rLight.intensity = 1.5f;
        }

        private void SpawnPreviewModel(GameObject prefab)
        {
            if (currentPreviewModel != null)
            {
                Destroy(currentPreviewModel);
                currentPreviewModel = null;
            }
            if (currentPreviewRoot != null)
            {
                Destroy(currentPreviewRoot);
                currentPreviewRoot = null;
            }

            if (prefab == null) return;
            SetupPreviewRig();

            currentPreviewModel = Instantiate(prefab, previewRigRoot.transform);

            // 1. Immediately freeze and strip physics so model never falls due to gravity
            var rbs = currentPreviewModel.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rbs.Length; i++)
            {
                rbs[i].isKinematic = true;
                rbs[i].useGravity = false;
                rbs[i].linearVelocity = Vector3.zero;
                rbs[i].angularVelocity = Vector3.zero;
                Destroy(rbs[i]);
            }
            var cols = currentPreviewModel.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].enabled = false;
                Destroy(cols[i]);
            }

            // 2. Center visual mesh bounds and normalize scale
            Renderer[] renderers = currentPreviewModel.GetComponentsInChildren<Renderer>(true);
            Vector3 centerOffset = Vector3.zero;
            float scaleFactor = 1f;
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                centerOffset = currentPreviewModel.transform.position - b.center;
                float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                float targetDim = 0.62f;
                scaleFactor = (maxDim > 0.01f) ? (targetDim / maxDim) : 1f;
            }

            currentPreviewRoot = new GameObject("PrizePivot");
            currentPreviewRoot.transform.SetParent(previewRigRoot.transform, false);

            currentPreviewModel.transform.SetParent(currentPreviewRoot.transform, false);
            currentPreviewModel.transform.localScale = Vector3.one * scaleFactor;
            currentPreviewModel.transform.localPosition = centerOffset * scaleFactor;
            currentPreviewModel.transform.localRotation = Quaternion.identity;

            EnsurePreviewRawImage();
            if (previewRawImage != null)
            {
                previewRawImage.texture = previewRenderTexture;
                previewRawImage.gameObject.SetActive(true);
            }

            if (previewCamera != null) previewCamera.enabled = true;

            // 3. Animate chute drop & reveal
            if (dropRoutine != null) StopCoroutine(dropRoutine);
            dropRoutine = StartCoroutine(AnimateDropRoutine());
        }

        private System.Collections.IEnumerator AnimateDropRoutine()
        {
            if (currentPreviewRoot == null) yield break;

            isDropAnimationDone = false;
            Vector3 dropStart = new Vector3(0f, 1.45f, 0f);
            Vector3 dropTarget = Vector3.zero;

            currentPreviewRoot.transform.localPosition = dropStart;
            currentPreviewRoot.transform.localRotation = Quaternion.Euler(18f, 0f, -8f);

            float elapsed = 0f;
            float duration = 0.38f;

            // Accelerating fall through chute door
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easeIn = t * t;
                currentPreviewRoot.transform.localPosition = Vector3.Lerp(dropStart, dropTarget, easeIn);
                currentPreviewRoot.transform.localRotation = Quaternion.Euler(Mathf.Lerp(18f, 10f, t), Mathf.Lerp(0f, 35f, t), Mathf.Lerp(-8f, 0f, t));
                yield return null;
            }

            currentPreviewRoot.transform.localPosition = dropTarget;

            // Soft landing chime/thud
            ServiceLocator.Get<IAudioService>()?.PlayDropFloor();

            // Elastic bounce & squish on chute floor
            float bounceElapsed = 0f;
            float bounceDuration = 0.24f;
            Vector3 baseScale = currentPreviewRoot.transform.localScale;

            while (bounceElapsed < bounceDuration)
            {
                bounceElapsed += Time.deltaTime;
                float bt = Mathf.Clamp01(bounceElapsed / bounceDuration);
                float bounceY = Mathf.Sin(bt * Mathf.PI) * 0.10f * (1f - bt);
                float squashX = 1f + Mathf.Sin(bt * Mathf.PI) * 0.08f * (1f - bt);
                float squashY = 1f - Mathf.Sin(bt * Mathf.PI) * 0.08f * (1f - bt);

                currentPreviewRoot.transform.localPosition = dropTarget + new Vector3(0f, bounceY, 0f);
                currentPreviewRoot.transform.localScale = new Vector3(baseScale.x * squashX, baseScale.y * squashY, baseScale.z * squashX);
                yield return null;
            }

            currentPreviewRoot.transform.localPosition = dropTarget;
            currentPreviewRoot.transform.localScale = baseScale;
            isDropAnimationDone = true;
            dropRoutine = null;
        }

        private void EnsurePreviewRawImage()
        {
            if (previewRawImage != null) return;
            if (modalPanel == null) return;

            Transform aperture = modalPanel.transform.Find("DialogCard/ShowcaseAperture");
            if (aperture == null) aperture = modalPanel.transform.Find("ShowcaseAperture");
            if (aperture == null) aperture = modalPanel.transform;

            GameObject rawObj = new GameObject("RawImage_PrizePreview", typeof(RectTransform));
            rawObj.transform.SetParent(aperture, false);
            RectTransform rt = rawObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            previewRawImage = rawObj.AddComponent<RawImage>();
            previewRawImage.raycastTarget = false;
        }

        private System.Collections.IEnumerator BounceRoutine()
        {
            if (modalPanel == null) yield break;
            float elapsed = 0f;
            float duration = 0.28f;
            Vector3 startScale = Vector3.one * 0.70f;
            Vector3 peakScale = Vector3.one * 1.06f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                modalPanel.transform.localScale = (t < 0.45f)
                    ? Vector3.Lerp(startScale, peakScale, t / 0.45f)
                    : Vector3.Lerp(peakScale, Vector3.one, (t - 0.45f) / 0.55f);
                yield return null;
            }
            modalPanel.transform.localScale = Vector3.one;
            bounceRoutine = null;
        }

        private System.Collections.IEnumerator AutoDismissAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Dismiss();
        }

        public void Dismiss()
        {
            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
                dropRoutine = null;
            }
            if (autoDismissRoutine != null)
            {
                StopCoroutine(autoDismissRoutine);
                autoDismissRoutine = null;
            }
            if (previewCamera != null)
            {
                previewCamera.enabled = false;
            }
            if (currentPreviewModel != null)
            {
                Destroy(currentPreviewModel);
                currentPreviewModel = null;
            }
            if (currentPreviewRoot != null)
            {
                Destroy(currentPreviewRoot);
                currentPreviewRoot = null;
            }
            if (previewRawImage != null)
            {
                previewRawImage.gameObject.SetActive(false);
            }
            if (previewRigRoot != null)
            {
                previewRigRoot.SetActive(false);
            }
            if (modalPanel != null) modalPanel.SetActive(false);

            // Re-apply machine atmosphere/lighting to ensure tint remains 100% intact
            FindFirstObjectByType<MachineController>()?.RefreshAtmosphere();
        }

        private void AutoFindRefsIfNull()
        {
            if (modalPanel == null)
            {
                if (name == "PrizeWinModal")
                {
                    modalPanel = gameObject;
                }
                else
                {
                    var found = transform.Find("PrizeWinModal");
                    if (found == null)
                    {
                        var allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                        for (int i = 0; i < allTransforms.Length; i++)
                        {
                            if (allTransforms[i].name == "PrizeWinModal" && allTransforms[i].hideFlags == HideFlags.None)
                            {
                                found = allTransforms[i];
                                break;
                            }
                        }
                    }
                    if (found != null) modalPanel = found.gameObject;
                }
            }

            if (modalPanel != null)
            {
                if (titleText == null) titleText = modalPanel.transform.Find("DialogCard/Title")?.GetComponent<Text>() ?? modalPanel.transform.Find("Title")?.GetComponent<Text>();
                if (prizeNameText == null) prizeNameText = modalPanel.transform.Find("DialogCard/PrizeName")?.GetComponent<Text>() ?? modalPanel.transform.Find("PrizeName")?.GetComponent<Text>();
                if (rarityBadgeText == null) rarityBadgeText = modalPanel.transform.Find("DialogCard/RarityBadge/RarityText")?.GetComponent<Text>() ?? modalPanel.transform.Find("RarityBadge/RarityText")?.GetComponent<Text>();
                if (rarityBadgeBg == null) rarityBadgeBg = modalPanel.transform.Find("DialogCard/RarityBadge")?.GetComponent<Image>() ?? modalPanel.transform.Find("RarityBadge")?.GetComponent<Image>();
                if (rewardDetailText == null) rewardDetailText = modalPanel.transform.Find("DialogCard/RewardDetail")?.GetComponent<Text>() ?? modalPanel.transform.Find("RewardDetail")?.GetComponent<Text>();
                if (claimButton == null) claimButton = modalPanel.transform.Find("DialogCard/Btn_Claim")?.GetComponent<Button>() ?? modalPanel.transform.Find("Btn_Claim")?.GetComponent<Button>();
            }
        }
    }
}
