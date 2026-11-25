using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Data;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class CollectionUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text coinsText;
        [SerializeField] private Text titleText;
        [SerializeField] private Text progressText;
        [SerializeField] private Text ownershipText;
        [SerializeField] private Transform gridContainer;

        [Header("Duplicate Sales")]
        [SerializeField] private Button sellDuplicatesButton;
        [SerializeField] private Text sellDuplicatesText;

        [Header("Celebration Modal")]
        [SerializeField] private GameObject completionModal;
        [SerializeField] private Text completionText;
        [SerializeField] private Button completionCloseButton;

        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleModal);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseModal);
            }
            if (sellDuplicatesButton != null)
            {
                sellDuplicatesButton.gameObject.SetActive(false);
            }
            if (completionCloseButton != null)
            {
                completionCloseButton.onClick.AddListener(() => {
                    if (completionModal != null) completionModal.SetActive(false);
                });
            }

            if (modalPanel != null) modalPanel.SetActive(false);
            if (completionModal != null) completionModal.SetActive(false);
        }

        private bool isSubscribed;

        private void OnEnable()
        {
            SubscribeToManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromManager();
        }

        private void SubscribeToManager()
        {
            if (isSubscribed) return;

            bool hasEcon = ServiceLocator.TryGet<IEconomyService>(out var econ);
            if (hasEcon)
            {
                econ.OnCoinsChanged -= HandleCoinsChanged;
                econ.OnCoinsChanged += HandleCoinsChanged;
            }

            bool hasColl = ServiceLocator.TryGet<ICollectionService>(out var coll);
            if (hasColl)
            {
                coll.OnPrizeRegistered -= HandlePrizeRegistered;
                coll.OnPrizeRegistered += HandlePrizeRegistered;
                coll.OnMachineCompleted -= HandleMachineCompleted;
                coll.OnMachineCompleted += HandleMachineCompleted;
                coll.OnCurrentMachineChanged -= HandleCurrentMachineChanged;
                coll.OnCurrentMachineChanged += HandleCurrentMachineChanged;
            }

            isSubscribed = hasEcon && hasColl;
        }

        private void UnsubscribeFromManager()
        {
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged -= HandleCoinsChanged;
            }

            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.OnPrizeRegistered -= HandlePrizeRegistered;
                coll.OnMachineCompleted -= HandleMachineCompleted;
                coll.OnCurrentMachineChanged -= HandleCurrentMachineChanged;
            }

            isSubscribed = false;
        }

        private void HandleCurrentMachineChanged(MachineDefinition def)
        {
            RefreshCollectionGrid();
        }

        private void Start()
        {
            SubscribeToManager();
            UpdateCoinsUI();
            RefreshCollectionGrid();
        }

        public void ToggleModal()
        {
            if (modalPanel == null) return;
            bool active = !modalPanel.activeSelf;
            if (active)
            {
                RectTransform contentTr = UIStyleHelper.EnsureModalShell(modalPanel, "TOY COLLECTION", CloseModal, 640f, 920f);
                Transform oldClose = modalPanel.transform.Find("DialogCard/Btn_Close") ?? modalPanel.transform.Find("Btn_Close");
                if (oldClose != null) oldClose.gameObject.SetActive(false);

                Transform oldHeader = modalPanel.transform.Find("Header");
                if (oldHeader != null) oldHeader.gameObject.SetActive(false);

                Transform statsBanner = contentTr.Find("StatsBanner");
                if (statsBanner == null)
                {
                    var sbGo = new GameObject("StatsBanner", typeof(RectTransform), typeof(Image));
                    sbGo.transform.SetParent(contentTr, false);
                    statsBanner = sbGo.transform;
                    RectTransform sbRt = sbGo.GetComponent<RectTransform>();
                    sbRt.anchorMin = new Vector2(0.04f, 0.91f);
                    sbRt.anchorMax = new Vector2(0.96f, 0.98f);
                    sbRt.offsetMin = Vector2.zero;
                    sbRt.offsetMax = Vector2.zero;

                    Image sbImg = sbGo.GetComponent<Image>();
                    sbImg.sprite = UIStyleHelper.BadgePillSprite;
                    sbImg.type = Image.Type.Sliced;
                    sbImg.color = new Color(0.12f, 0.15f, 0.22f, 0.95f);

                    var tGo = new GameObject("StatsText", typeof(RectTransform), typeof(Text));
                    tGo.transform.SetParent(statsBanner, false);
                    RectTransform tRt = tGo.GetComponent<RectTransform>();
                    tRt.anchorMin = Vector2.zero;
                    tRt.anchorMax = Vector2.one;
                    tRt.offsetMin = Vector2.zero;
                    tRt.offsetMax = Vector2.zero;

                    Text stText = tGo.GetComponent<Text>();
                    stText.font = UIStyleHelper.MainFont;
                    stText.fontSize = 17;
                    stText.fontStyle = FontStyle.Bold;
                    stText.alignment = TextAnchor.MiddleCenter;
                    stText.color = new Color(1.0f, 0.85f, 0.22f, 1f);
                    UIStyleHelper.AddDropShadow(stText, new Color(0f, 0f, 0f, 0.7f), new Vector2(1f, -1.5f));
                    progressText = stText;
                }

                if (gridContainer != null)
                {
                    gridContainer.SetParent(contentTr, false);
                    RectTransform gRt = gridContainer.GetComponent<RectTransform>();
                    gRt.anchorMin = new Vector2(0.02f, 0.02f);
                    gRt.anchorMax = new Vector2(0.98f, 0.89f);
                    gRt.offsetMin = Vector2.zero;
                    gRt.offsetMax = Vector2.zero;

                    var glg = gridContainer.GetComponent<GridLayoutGroup>();
                    if (glg != null)
                    {
                        glg.cellSize = new Vector2(185f, 230f);
                        glg.spacing = new Vector2(12f, 12f);
                        glg.childAlignment = TextAnchor.MiddleCenter;
                    }
                }

                modalPanel.SetActive(true);
                Transform card = modalPanel.transform.Find("DialogCard") ?? modalPanel.transform;
                UIStyleHelper.AnimatePopIn(this, card);
                ServiceLocator.Get<IAudioService>()?.PlayModalOpen();
                RefreshCollectionGrid();
            }
            else
            {
                modalPanel.SetActive(false);
            }
        }

        public void CloseModal()
        {
            if (modalPanel != null) modalPanel.SetActive(false);
        }

        private void HandleCoinsChanged(int newCoins)
        {
            UpdateCoinsUI();
        }

        private void HandlePrizeRegistered(PrizeDefinition prize, bool isNew)
        {
            UpdateCoinsUI();
            if (modalPanel != null && modalPanel.activeSelf)
            {
                RefreshCollectionGrid();
            }
        }

        private void HandleMachineCompleted(MachineDefinition machine)
        {
            RefreshCollectionGrid();
            ServiceLocator.Get<IAudioService>()?.PlayUnlockFanfare();
            if (completionModal != null)
            {
                if (completionText != null)
                {
                    completionText.text = $"CONGRATULATIONS!\n\nYOU COMPLETED {machine.displayName.ToUpper()}!\n\n• Machine is now permanently YOURS!\n• Free plays unlocked forever!\n• Passive income: +{machine.passiveIncomePerMinute} coins/min!";
                }
                completionModal.SetActive(true);
                Transform compCard = completionModal.transform.Find("DialogCard") ?? completionModal.transform;
                UIStyleHelper.AnimatePopIn(this, compCard);
            }
        }

        private void UpdateCoinsUI()
        {
            if (coinsText == null) return;
            int coins = 0;
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                coins = econ.Coins;
            }
            coinsText.text = $"{coins:N0} COINS";
        }

        public void RefreshCollectionGrid()
        {
            ICollectionService collService = ServiceLocator.Get<ICollectionService>();
            if (collService == null) return;

            MachineDefinition machine = collService.CurrentMachine;
            if (machine == null) return;

            int discovered = collService.GetDiscoveredCount(machine);
            bool isOwned = collService.IsMachineOwned(machine.machineId);

            if (titleText != null)
            {
                titleText.text = $"{machine.displayName.ToUpper()}";
            }
            if (progressText != null)
            {
                int total = machine.prizes != null ? machine.prizes.Length : 9;
                int pct = total > 0 ? (int)((discovered / (float)total) * 100f) : 0;
                progressText.text = $"COLLECTION: {discovered} / {total} ({pct}%)";
            }
            if (ownershipText != null)
            {
                ownershipText.text = isOwned ? "[OWNED] FREE PLAYS + PASSIVE INCOME" : $"UNOWNED (ENTRY: {machine.entryCost} COINS)";
                ownershipText.color = isOwned ? new Color(0.3f, 0.9f, 0.4f) : new Color(0.85f, 0.85f, 0.9f);
            }

            if (sellDuplicatesButton != null && sellDuplicatesButton.gameObject.activeSelf)
            {
                sellDuplicatesButton.gameObject.SetActive(false);
            }

            // Populate cards dynamically
            if (gridContainer == null || machine.prizes == null) return;

            for (int i = 0; i < machine.prizes.Length; i++)
            {
                PrizeDefinition def = machine.prizes[i];
                if (def == null) continue;

                Transform cardTr = gridContainer.childCount > i ? gridContainer.GetChild(i) : null;
                if (cardTr == null && gridContainer.childCount > 0)
                {
                    GameObject clone = Instantiate(gridContainer.GetChild(0).gameObject, gridContainer);
                    clone.name = $"Card_{i}";
                    cardTr = clone.transform;
                }
                if (cardTr == null) continue;

                cardTr.gameObject.SetActive(true);
                bool hasFound = collService.IsPrizeDiscovered(def.id);
                int count = collService.GetPrizeCount(def.id);
                UpdateCard(cardTr.gameObject, def, hasFound, count);
            }

            for (int i = machine.prizes.Length; i < gridContainer.childCount; i++)
            {
                gridContainer.GetChild(i).gameObject.SetActive(false);
            }
        }

        private readonly Dictionary<string, Sprite> generatedIcons = new Dictionary<string, Sprite>();

        private void OnDestroy()
        {
            foreach (var kvp in generatedIcons)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.texture != null) Destroy(kvp.Value.texture);
                    Destroy(kvp.Value);
                }
            }
            generatedIcons.Clear();
        }

        private void UpdateCard(GameObject card, PrizeDefinition def, bool discovered, int count = 1)
        {
            Image bg = card.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = UIStyleHelper.ShelfPedestalSprite;
                bg.type = Image.Type.Sliced;
                bg.color = Color.white;
            }

            Color rarityColor = def.rarity switch
            {
                PrizeRarity.Normal => new Color(0.20f, 0.82f, 1.0f),
                PrizeRarity.Rare => new Color(0.92f, 0.36f, 0.98f),
                PrizeRarity.Secret => new Color(1.0f, 0.85f, 0.18f),
                _ => Color.gray
            };

            // Prize Icon / Model Display sitting on shelf
            Transform iconTr = card.transform.Find("PrizeIcon");
            Image iconImg = null;
            if (iconTr == null)
            {
                GameObject iconGo = new GameObject("PrizeIcon", typeof(RectTransform), typeof(Image));
                iconGo.transform.SetParent(card.transform, false);
                RectTransform rt = iconGo.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, 24f);
                rt.sizeDelta = new Vector2(120f, 120f);
                iconImg = iconGo.GetComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
            }
            else
            {
                iconImg = iconTr.GetComponent<Image>();
                RectTransform rt = iconTr.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(0f, 24f);
                    rt.sizeDelta = new Vector2(120f, 120f);
                }
                if (iconImg != null) iconImg.preserveAspect = true;
            }

            if (iconImg != null)
            {
                Sprite sp = GetOrGeneratePrizeIcon(def);
                if (sp != null)
                {
                    // Undiscovered = black silhouette (curiosity hook) instead of an empty shelf.
                    iconImg.enabled = true;
                    iconImg.sprite = sp;
                    iconImg.color = discovered ? Color.white : new Color(0f, 0f, 0f, 0.85f);
                }
                else
                {
                    iconImg.enabled = false;
                }
            }

            // Hide or remove legacy texts if they exist directly under card
            Transform legacyName = card.transform.Find("Name");
            if (legacyName != null) legacyName.gameObject.SetActive(false);
            Transform legacyRarity = card.transform.Find("Rarity");
            if (legacyRarity != null) legacyRarity.gameObject.SetActive(false);
            Transform legacyStatus = card.transform.Find("Status");
            if (legacyStatus != null) legacyStatus.gameObject.SetActive(false);

            // Brass Placard Plate under the prize spot
            Transform placardTr = card.transform.Find("PlacardPlate");
            if (placardTr == null)
            {
                GameObject placardGo = new GameObject("PlacardPlate", typeof(RectTransform), typeof(Image));
                placardGo.transform.SetParent(card.transform, false);
                RectTransform pRt = placardGo.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0f);
                pRt.anchorMax = new Vector2(0.5f, 0f);
                pRt.anchoredPosition = new Vector2(0f, 26f);
                pRt.sizeDelta = new Vector2(165f, 44f);

                Image pImg = placardGo.GetComponent<Image>();
                pImg.sprite = UIStyleHelper.BrassPlacardSprite;
                pImg.type = Image.Type.Sliced;
                pImg.color = Color.white;

                Font font = UIStyleHelper.MainFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

                // Title Text on placard
                GameObject pTitleGo = new GameObject("PlacardTitle", typeof(RectTransform), typeof(Text));
                pTitleGo.transform.SetParent(placardGo.transform, false);
                RectTransform ptRt = pTitleGo.GetComponent<RectTransform>();
                ptRt.anchorMin = new Vector2(0f, 0.38f);
                ptRt.anchorMax = new Vector2(1f, 1f);
                ptRt.offsetMin = new Vector2(6f, 0f);
                ptRt.offsetMax = new Vector2(-6f, -1f);
                Text ptTxt = pTitleGo.GetComponent<Text>();
                ptTxt.font = font;
                ptTxt.fontSize = 16;
                ptTxt.fontStyle = FontStyle.Bold;
                ptTxt.alignment = TextAnchor.MiddleCenter;
                ptTxt.color = new Color(0.18f, 0.12f, 0.05f, 1f);

                // Subtitle / Rarity on placard
                GameObject pSubGo = new GameObject("PlacardRarity", typeof(RectTransform), typeof(Text));
                pSubGo.transform.SetParent(placardGo.transform, false);
                RectTransform psRt = pSubGo.GetComponent<RectTransform>();
                psRt.anchorMin = new Vector2(0f, 0f);
                psRt.anchorMax = new Vector2(1f, 0.42f);
                psRt.offsetMin = new Vector2(6f, 1f);
                psRt.offsetMax = new Vector2(-6f, 0f);
                Text psTxt = pSubGo.GetComponent<Text>();
                psTxt.font = font;
                psTxt.fontSize = 14;
                psTxt.fontStyle = FontStyle.Bold;
                psTxt.alignment = TextAnchor.MiddleCenter;

                placardTr = placardGo.transform;
            }

            if (placardTr != null)
            {
                Text pTitle = placardTr.Find("PlacardTitle")?.GetComponent<Text>();
                Text pRarity = placardTr.Find("PlacardRarity")?.GetComponent<Text>();

                if (pTitle != null)
                {
                    pTitle.text = discovered ? def.displayName.ToUpper() : "EMPTY ALCOVE";
                    pTitle.color = discovered ? new Color(0.18f, 0.12f, 0.05f, 1f) : new Color(0.45f, 0.38f, 0.28f, 1f);
                }
                if (pRarity != null)
                {
                    pRarity.text = discovered ? $"✦ {def.rarity.ToString().ToUpper()} ✦" : "— LOCKED —";
                    pRarity.color = discovered ? rarityColor : new Color(0.55f, 0.48f, 0.38f, 1f);
                    if (discovered) UIStyleHelper.AddDropShadow(pRarity, new Color(0f, 0f, 0f, 0.6f), new Vector2(1f, -1f));
                }
            }

            // Click to inspect 3D turntable for discovered trophies
            Button cardBtn = card.GetComponent<Button>();
            if (cardBtn == null) cardBtn = card.AddComponent<Button>();
            cardBtn.onClick.RemoveAllListeners();
            cardBtn.interactable = discovered;
            if (discovered)
            {
                PrizeDefinition capturedDef = def;
                cardBtn.onClick.AddListener(() =>
                {
                    var winModal = PrizeWinModalUI.Instance ?? FindFirstObjectByType<PrizeWinModalUI>(FindObjectsInactive.Include);
                    if (winModal != null)
                    {
                        winModal.ShowWinDefinition(capturedDef, capturedDef.displayName, capturedDef.rarity, isInspectOnly: true);
                    }
                });
            }
        }

        private Sprite GetOrGeneratePrizeIcon(PrizeDefinition def)
        {
            if (def == null) return null;
            if (def.icon != null) return def.icon;
            if (def.prefab == null) return null;

            string key = !string.IsNullOrEmpty(def.id) ? def.id : def.prefab.name;
            if (generatedIcons.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            // Render a crisp 128x128 square thumbnail offscreen
            const int size = 128;
            RenderTexture rt = RenderTexture.GetTemporary(size, size, 16, RenderTextureFormat.ARGB32);
            GameObject rig = new GameObject("TempIconRig");
            rig.transform.position = new Vector3(0f, -400f, 0f);

            GameObject camGo = new GameObject("IconCam");
            camGo.transform.SetParent(rig.transform, false);
            camGo.transform.localPosition = new Vector3(0f, 0.15f, -1.6f);
            camGo.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.fieldOfView = 30f;
            cam.aspect = 1.0f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 6f;
            cam.targetTexture = rt;

            GameObject lightGo = new GameObject("IconLight");
            lightGo.transform.SetParent(rig.transform, false);
            lightGo.transform.localPosition = new Vector3(0.8f, 1.2f, -1.2f);
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 4f;
            l.intensity = 2.6f;

            GameObject model = Instantiate(def.prefab, rig.transform);

            Renderer[] rends = model.GetComponentsInChildren<Renderer>(true);
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
                Vector3 offset = model.transform.position - b.center;
                float maxDim = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
                float scale = maxDim > 0.01f ? (0.58f / maxDim) : 1f;
                model.transform.localScale = Vector3.one * scale;
                model.transform.localPosition = offset * scale;
                model.transform.localRotation = Quaternion.Euler(0f, -28f, 0f);
            }

            cam.Render();

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;

            cam.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            rig.SetActive(false);
            Destroy(rig);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            generatedIcons[key] = sprite;
            return sprite;
        }
    }
}
