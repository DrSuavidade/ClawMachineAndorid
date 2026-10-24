using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ClawMachine.Data;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class ArcadeConsoleUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Controllers")]
        [SerializeField] private MachineController machineController;

        [Header("Machine Catalog")]
        [SerializeField] private MachineCatalog catalog;
        [SerializeField] private MachineDefinition[] availableMachines;
        [SerializeField] private int currentMachineIndex = 0;

        [Header("Carousel UI Elements")]
        [SerializeField] private Button prevMachineButton;
        [SerializeField] private Button nextMachineButton;
        [SerializeField] private Text machineNameText;
        [SerializeField] private Text machineStatusText;

        [Header("Unlock Modal")]
        [SerializeField] private GameObject unlockModal;
        [SerializeField] private Text unlockTitleText;
        [SerializeField] private Text unlockDescriptionText;
        [SerializeField] private Text unlockCoinsBalanceText;
        [SerializeField] private Button unlockConfirmButton;
        [SerializeField] private Text unlockConfirmButtonText;
        [SerializeField] private Button unlockCancelButton;

        private MachineDefinition pendingUnlockMachine;
        private Vector2 dragStartPos;
        private const float SWIPE_THRESHOLD = 70f;
        private ArcadePushButtonUI cachedPushButton;
        private ArcadeJoystickUI cachedJoystick;
        private Transform coinBadgeTransform;
        private Text coinBadgeText;
        private int lastDisplayedCoins = -1;

        private void Awake()
        {
            if (machineController == null)
            {
                machineController = FindFirstObjectByType<MachineController>();
            }

            if (cachedModalLayer == null)
            {
                cachedModalLayer = transform.Find("ModalLayer") ?? transform.root.Find("ModalLayer");
            }

            if (ClawMachine.UI.Theming.UIThemeManager.Instance == null)
            {
                var mgr = gameObject.GetComponent<ClawMachine.UI.Theming.UIThemeManager>();
                if (mgr == null) gameObject.AddComponent<ClawMachine.UI.Theming.UIThemeManager>();
            }

            SetupThemedUIElements();
            UIStyleHelper.ApplyAll(gameObject);

            if (prevMachineButton != null)
            {
                prevMachineButton.onClick.AddListener(OnPrevClicked);
            }
            if (nextMachineButton != null)
            {
                nextMachineButton.onClick.AddListener(OnNextClicked);
            }

            if (unlockConfirmButton != null)
            {
                unlockConfirmButton.onClick.AddListener(OnConfirmUnlockClicked);
            }
            if (unlockCancelButton != null)
            {
                unlockCancelButton.onClick.AddListener(CloseUnlockModal);
            }

            if (unlockModal != null)
            {
                unlockModal.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.OnMachineCompleted -= HandleMachineStateUpdated;
                coll.OnMachineCompleted += HandleMachineStateUpdated;
                coll.OnMachineUnlocked -= HandleMachineStateUpdated;
                coll.OnMachineUnlocked += HandleMachineStateUpdated;
            }
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged -= HandleCoinsChanged;
                econ.OnCoinsChanged += HandleCoinsChanged;
            }
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                coll.OnMachineCompleted -= HandleMachineStateUpdated;
                coll.OnMachineUnlocked -= HandleMachineStateUpdated;
            }
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                econ.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void HandleMachineStateUpdated(MachineDefinition _) => UpdateCarouselDisplay();

        private void HandleCoinsChanged(int newCoins)
        {
            if (coinBadgeText != null)
            {
                int from = lastDisplayedCoins >= 0 ? lastDisplayedCoins : newCoins;
                UIStyleHelper.AnimateNumberRoll(this, coinBadgeText, from, newCoins, 0.35f, "", " COINS");
                if (coinBadgeTransform != null)
                {
                    UIStyleHelper.AnimateScalePunch(this, coinBadgeTransform, 1.18f, 0.22f);
                }
            }
            lastDisplayedCoins = newCoins;
        }

        private void Start()
        {
            ICollectionService collService = ServiceLocator.Get<ICollectionService>();

            if (catalog == null && collService is CollectionManager cm && cm.Catalog != null)
            {
                catalog = cm.Catalog;
            }

            if (catalog != null && catalog.Machines != null && catalog.Machines.Length > 0)
            {
                availableMachines = catalog.Machines;
            }
            else if (availableMachines == null || availableMachines.Length == 0)
            {
                Debug.LogWarning("[ArcadeConsoleUI] No machines available. Please assign MachineCatalog in inspector.");
            }

            // Sync index with current machine
            if (collService != null && collService.CurrentMachine != null && availableMachines != null)
            {
                for (int i = 0; i < availableMachines.Length; i++)
                {
                    if (availableMachines[i] != null && availableMachines[i].machineId == collService.CurrentMachine.machineId)
                    {
                        currentMachineIndex = i;
                        break;
                    }
                }
            }

            SetupTopBarHUD();
            SetupConsoleCenterLayout();
            EnsureGameplayHintBanner();
            UpdateCarouselDisplay();
        }

        private void SetupTopBarHUD()
        {
            Transform canopy = transform.Find("SafeAreaContainer/MachineMarqueeCanopy");
            if (canopy == null) return;

            // Hide raw legacy rivets and frame lines in canopy
            for (int i = canopy.childCount - 1; i >= 0; i--)
            {
                Transform c = canopy.GetChild(i);
                if (c.name == "Rivet" || c.name == "MarqueeRim" || c.name == "NeonRunner")
                {
                    c.gameObject.SetActive(false);
                }
            }

            RectTransform canopyRt = canopy.GetComponent<RectTransform>();
            if (canopyRt != null)
            {
                canopyRt.anchorMin = new Vector2(0f, 1f);
                canopyRt.anchorMax = new Vector2(1f, 1f);
                canopyRt.pivot = new Vector2(0.5f, 1f);
                canopyRt.anchoredPosition = new Vector2(0f, -8f);
                canopyRt.sizeDelta = new Vector2(-16f, 66f);
            }

            Image canopyImg = canopy.GetComponent<Image>();
            if (canopyImg != null)
            {
                canopyImg.sprite = UIStyleHelper.BadgePillSprite;
                canopyImg.type = Image.Type.Sliced;
                canopyImg.color = new Color(0.06f, 0.08f, 0.12f, 0.90f);
            }

            // 1. Btn_Collection (Left)
            Transform collTr = canopy.Find("Btn_Collection");
            if (collTr != null)
            {
                RectTransform rt = collTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.015f, 0.12f);
                rt.anchorMax = new Vector2(0.245f, 0.88f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Text t = collTr.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.text = "TOYS";
                    t.fontSize = 16;
                    t.fontStyle = FontStyle.Bold;
                    t.color = Color.white;
                    t.alignment = TextAnchor.MiddleCenter;
                    RectTransform trt = t.GetComponent<RectTransform>();
                    trt.anchorMin = new Vector2(0.5f, 0.5f);
                    trt.anchorMax = new Vector2(0.5f, 0.5f);
                    trt.anchoredPosition = new Vector2(12f, 0f);
                    trt.sizeDelta = new Vector2(64f, 32f);
                    UIStyleHelper.AddDropShadow(t, new Color(0f, 0f, 0f, 0.7f), new Vector2(1f, -1.5f));
                }
                UIStyleHelper.EnsureChildIcon(collTr, "Icon_Trophy", UIStyleHelper.IconTrophy, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(22f, 22f), new Vector2(-28f, 0f));
                Button btn = collTr.GetComponent<Button>();
                if (btn != null) UIStyleHelper.StyleButton(btn);
            }

            // 2. CoinBankPlate (Move coin counter to top bar!)
            Transform coinBadge = transform.Find("SafeAreaContainer/ConsoleDeck/CarouselStrip/CoinBankPlate")
                               ?? transform.Find("SafeAreaContainer/ConsoleDeck/CarouselStrip/Badge_Coins")
                               ?? canopy.Find("CoinBankPlate")
                               ?? canopy.Find("Badge_Coins");

            if (coinBadge != null)
            {
                coinBadge.name = "CoinBankPlate";
                coinBadge.SetParent(canopy, false);
                RectTransform rt = coinBadge.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.26f, 0.12f);
                rt.anchorMax = new Vector2(0.62f, 0.88f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image cbImg = coinBadge.GetComponent<Image>();
                if (cbImg != null)
                {
                    cbImg.sprite = UIStyleHelper.BadgePillSprite;
                    cbImg.type = Image.Type.Sliced;
                    cbImg.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);
                }

                coinBadgeTransform = coinBadge;
                coinBadgeText = coinBadge.GetComponentInChildren<Text>();
                if (coinBadgeText != null)
                {
                    coinBadgeText.fontSize = 17;
                    coinBadgeText.fontStyle = FontStyle.Bold;
                    coinBadgeText.color = new Color(1.0f, 0.85f, 0.22f, 1f);
                    coinBadgeText.alignment = TextAnchor.MiddleCenter;
                    RectTransform ctrt = coinBadgeText.GetComponent<RectTransform>();
                    ctrt.anchorMin = new Vector2(0.5f, 0.5f);
                    ctrt.anchorMax = new Vector2(0.5f, 0.5f);
                    ctrt.anchoredPosition = new Vector2(14f, 0f);
                    ctrt.sizeDelta = new Vector2(130f, 32f);
                    UIStyleHelper.AddDropShadow(coinBadgeText, new Color(0f, 0f, 0f, 0.8f), new Vector2(1f, -1.5f));
                }

                UIStyleHelper.EnsureChildIcon(coinBadge, "Icon_Coin", UIStyleHelper.IconCoin, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(24f, 24f), new Vector2(-54f, 0f));

                Button coinBtn = coinBadge.GetComponent<Button>();
                if (coinBtn == null) coinBtn = coinBadge.gameObject.AddComponent<Button>();
                coinBtn.onClick.RemoveAllListeners();
                coinBtn.onClick.AddListener(() =>
                {
                    var adSvc = AdService.Instance ?? FindFirstObjectByType<AdService>();
                    if (adSvc != null) adSvc.ShowOutOfCoinsPrompt();
                });

                if (ServiceLocator.TryGet<IEconomyService>(out var econ))
                {
                    lastDisplayedCoins = econ.Coins;
                    if (coinBadgeText != null) coinBadgeText.text = $"{lastDisplayedCoins:N0} COINS";
                }
            }

            // 3. Btn_Upgrades (Right-Center)
            Transform upgTr = canopy.Find("Btn_Upgrades");
            if (upgTr != null)
            {
                RectTransform rt = upgTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.635f, 0.12f);
                rt.anchorMax = new Vector2(0.865f, 0.88f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Text t = upgTr.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.text = "UPGRADE";
                    t.fontSize = 15;
                    t.fontStyle = FontStyle.Bold;
                    t.color = Color.white;
                    t.alignment = TextAnchor.MiddleCenter;
                    RectTransform trt = t.GetComponent<RectTransform>();
                    trt.anchorMin = new Vector2(0.5f, 0.5f);
                    trt.anchorMax = new Vector2(0.5f, 0.5f);
                    trt.anchoredPosition = new Vector2(12f, 0f);
                    trt.sizeDelta = new Vector2(76f, 32f);
                    UIStyleHelper.AddDropShadow(t, new Color(0f, 0f, 0f, 0.7f), new Vector2(1f, -1.5f));
                }
                UIStyleHelper.EnsureChildIcon(upgTr, "Icon_Bolt", UIStyleHelper.IconLightning, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20f, 20f), new Vector2(-36f, 0f));

                Transform pip = upgTr.Find("Pip_Progress");
                if (pip != null)
                {
                    Image pImg = pip.GetComponent<Image>();
                    if (pImg != null) UIStyleHelper.StyleNotificationPip(pImg);
                    RectTransform prt = pip.GetComponent<RectTransform>();
                    if (prt != null)
                    {
                        prt.anchorMin = Vector2.one;
                        prt.anchorMax = Vector2.one;
                        prt.pivot = new Vector2(0.5f, 0.5f);
                        prt.anchoredPosition = new Vector2(-4f, -4f);
                        prt.sizeDelta = new Vector2(16f, 16f);
                    }
                }

                Button btn = upgTr.GetComponent<Button>();
                if (btn != null) UIStyleHelper.StyleButton(btn);
            }

            // 4. Btn_Settings (Far Right)
            Transform setTr = canopy.Find("Btn_Settings");
            if (setTr != null)
            {
                RectTransform rt = setTr.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.88f, 0.12f);
                rt.anchorMax = new Vector2(0.985f, 0.88f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Text t = setTr.GetComponentInChildren<Text>();
                if (t != null) t.text = "";
                UIStyleHelper.EnsureChildIcon(setTr, "Icon_Gear", UIStyleHelper.IconGear, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(24f, 24f), Vector2.zero);
                Button btn = setTr.GetComponent<Button>();
                if (btn != null) UIStyleHelper.StyleButton(btn);
            }
        }

        private void SetupConsoleCenterLayout()
        {
            Transform deck = transform.Find("SafeAreaContainer/ConsoleDeck");
            if (deck != null)
            {
                for (int i = 0; i < deck.childCount; i++)
                {
                    Transform c = deck.GetChild(i);
                    if (c.name == "Rivet" || c.name == "ConsoleRim")
                    {
                        c.gameObject.SetActive(false);
                    }
                }
            }

            Transform carouselStrip = machineNameText != null ? machineNameText.transform.parent : transform.Find("SafeAreaContainer/ConsoleDeck/CarouselStrip");
            if (carouselStrip == null) return;

            // Remove any legacy overlapping backing pill
            Transform oldPill = carouselStrip.Find("Pill_MachineStatus");
            if (oldPill != null) Destroy(oldPill.gameObject);

            Image stripImg = carouselStrip.GetComponent<Image>();
            if (stripImg != null)
            {
                stripImg.sprite = UIStyleHelper.ArcadeDisplayScreenSprite;
                stripImg.type = Image.Type.Sliced;
                stripImg.color = Color.white;
            }

            // Prev Machine Button (Left Chevron)
            if (prevMachineButton != null)
            {
                RectTransform pRt = prevMachineButton.GetComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0f, 0.5f);
                pRt.anchorMax = new Vector2(0f, 0.5f);
                pRt.pivot = new Vector2(0f, 0.5f);
                pRt.anchoredPosition = new Vector2(10f, 0f);
                pRt.sizeDelta = new Vector2(52f, 52f);

                Image pImg = prevMachineButton.GetComponent<Image>();
                if (pImg != null)
                {
                    pImg.sprite = UIStyleHelper.BadgePillSprite;
                    pImg.type = Image.Type.Sliced;
                    pImg.color = new Color(0.14f, 0.18f, 0.28f, 0.95f);
                }

                Text pt = prevMachineButton.GetComponentInChildren<Text>();
                if (pt != null) pt.text = "";
                UIStyleHelper.EnsureChildIcon(prevMachineButton.transform, "Icon_Arrow", UIStyleHelper.IconArrowLeft, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(26f, 26f), Vector2.zero);
                UIStyleHelper.StyleButton(prevMachineButton);
            }

            // Next Machine Button (Right Chevron)
            if (nextMachineButton != null)
            {
                RectTransform nRt = nextMachineButton.GetComponent<RectTransform>();
                nRt.anchorMin = new Vector2(1f, 0.5f);
                nRt.anchorMax = new Vector2(1f, 0.5f);
                nRt.pivot = new Vector2(1f, 0.5f);
                nRt.anchoredPosition = new Vector2(-10f, 0f);
                nRt.sizeDelta = new Vector2(52f, 52f);

                Image nImg = nextMachineButton.GetComponent<Image>();
                if (nImg != null)
                {
                    nImg.sprite = UIStyleHelper.BadgePillSprite;
                    nImg.type = Image.Type.Sliced;
                    nImg.color = new Color(0.14f, 0.18f, 0.28f, 0.95f);
                }

                Text nt = nextMachineButton.GetComponentInChildren<Text>();
                if (nt != null) nt.text = "";
                UIStyleHelper.EnsureChildIcon(nextMachineButton.transform, "Icon_Arrow", UIStyleHelper.IconArrowRight, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(26f, 26f), Vector2.zero);
                UIStyleHelper.StyleButton(nextMachineButton);
            }

            // Machine Name & Status centered
            if (machineNameText != null)
            {
                RectTransform mnRt = machineNameText.GetComponent<RectTransform>();
                mnRt.anchorMin = new Vector2(0.16f, 0.46f);
                mnRt.anchorMax = new Vector2(0.84f, 0.96f);
                mnRt.offsetMin = Vector2.zero;
                mnRt.offsetMax = Vector2.zero;
                machineNameText.fontSize = 20;
                machineNameText.fontStyle = FontStyle.Bold;
                machineNameText.color = Color.white;
                machineNameText.alignment = TextAnchor.MiddleCenter;
                UIStyleHelper.AddDropShadow(machineNameText, new Color(0f, 0f, 0f, 0.8f), new Vector2(1f, -1.5f));
            }

            if (machineStatusText != null)
            {
                RectTransform msRt = machineStatusText.GetComponent<RectTransform>();
                msRt.anchorMin = new Vector2(0.16f, 0.04f);
                msRt.anchorMax = new Vector2(0.84f, 0.48f);
                msRt.offsetMin = Vector2.zero;
                msRt.offsetMax = Vector2.zero;
                machineStatusText.fontSize = 16;
                machineStatusText.fontStyle = FontStyle.Bold;
                machineStatusText.alignment = TextAnchor.MiddleCenter;
                UIStyleHelper.AddDropShadow(machineStatusText, new Color(0f, 0f, 0f, 0.8f), new Vector2(1f, -1.5f));
            }
        }

        private GameObject hintBannerObj;
        private Text hintBannerText;
        private bool hasDroppedThisSession;
        private float aimIdleTimer = 0f;
        private Vector2 swipeTouchStart;
        private bool isTouchSwiping;

        private void EnsureGameplayHintBanner()
        {
            if (hintBannerObj != null) return;
            Transform parent = transform.Find("SafeAreaContainer") ?? transform;
            Transform existing = parent.Find("GameplayHintBanner");
            if (existing != null)
            {
                hintBannerObj = existing.gameObject;
                hintBannerText = hintBannerObj.GetComponentInChildren<Text>();
                return;
            }

            hintBannerObj = new GameObject("GameplayHintBanner", typeof(RectTransform), typeof(Image));
            hintBannerObj.transform.SetParent(parent, false);
            RectTransform rt = hintBannerObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.27f);
            rt.anchorMax = new Vector2(0.5f, 0.27f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(780f, 54f);

            Image bg = hintBannerObj.GetComponent<Image>();
            bg.sprite = UIStyleHelper.BadgePillSprite;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.05f, 0.07f, 0.12f, 0.84f);
            bg.raycastTarget = false;

            GameObject txtGo = new GameObject("HintText", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(hintBannerObj.transform, false);
            RectTransform txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(16f, 0f);
            txtRt.offsetMax = new Vector2(-16f, 0f);

            hintBannerText = txtGo.GetComponent<Text>();
            hintBannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            hintBannerText.fontSize = 23;
            hintBannerText.fontStyle = FontStyle.Bold;
            hintBannerText.alignment = TextAnchor.MiddleCenter;
            hintBannerText.raycastTarget = false;
        }

        private void UpdateGameplayHintBanner()
        {
            if (hintBannerObj == null || machineController == null || machineController.Claw == null) return;

            var claw = machineController.Claw;
            var state = claw.CurrentState;
            if (state == ClawState.Descending)
            {
                hasDroppedThisSession = true;
            }

            if (state == ClawState.Carrying)
            {
                if (!hintBannerObj.activeSelf) hintBannerObj.SetActive(true);
                bool overChute = claw.IsOverChute;
                hintBannerText.text = overChute
                    ? "[!] OVER CHUTE! TAP DROP PRIZE NOW!"
                    : "<< STEER TO FRONT-LEFT CHUTE & TAP DROP!";
                hintBannerText.color = overChute
                    ? new Color(0.25f, 1.0f, 0.45f, 1f)
                    : new Color(1.0f, 0.86f, 0.22f, 1f);
                float pulse = 1f + Mathf.Sin(Time.time * 6.5f) * 0.035f;
                hintBannerObj.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else if (state == ClawState.Aiming && !hasDroppedThisSession && aimIdleTimer >= 10.0f)
            {
                if (!hintBannerObj.activeSelf) hintBannerObj.SetActive(true);
                hintBannerText.text = "DRAG JOYSTICK TO AIM  •  TAP DROP TO GRAB";
                float alpha = 0.78f + Mathf.Sin(Time.time * 3.5f) * 0.20f;
                hintBannerText.color = new Color(0.92f, 0.96f, 1.0f, alpha);
                hintBannerObj.transform.localScale = Vector3.one;
            }
            else if (hintBannerObj.activeSelf)
            {
                hintBannerObj.SetActive(false);
            }
        }

        private void Update()
        {
            // Reset aim idle timer whenever user provides any input
            bool userActive = Input.anyKeyDown || Input.touchCount > 0 || Input.GetMouseButton(0) ||
                             Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.05f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.05f;
            if (userActive)
            {
                aimIdleTimer = 0f;
            }
            else
            {
                aimIdleTimer += Time.deltaTime;
            }
            // Android Hardware Back / Escape key: close active modal
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (TryCloseTopModal()) return;
            }

            if (IsAnyModalOpen())
            {
                if (hintBannerObj != null && hintBannerObj.activeSelf) hintBannerObj.SetActive(false);
                return;
            }

            UpdateGameplayHintBanner();

            bool canSwitch = machineController == null || machineController.CanSwitchMachine;
            if (prevMachineButton != null && prevMachineButton.interactable != canSwitch)
            {
                prevMachineButton.interactable = canSwitch;
            }
            if (nextMachineButton != null && nextMachineButton.interactable != canSwitch)
            {
                nextMachineButton.interactable = canSwitch;
            }

            if (!canSwitch) return;

            // Keyboard shortcuts to switch machine (Q / E keys)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnPrevClicked();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                OnNextClicked();
            }

            // Touch / mouse horizontal swipe detection ONLY on top header marquee (prevents mid-aim accidental swipes)
            if (Input.GetMouseButtonDown(0))
            {
                // Only begin swipe if tapping in upper marquee region (>70% screen height) and not over other UI
                if (Input.mousePosition.y > Screen.height * 0.70f && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
                {
                    swipeTouchStart = Input.mousePosition;
                    isTouchSwiping = true;
                }
            }
            else if (Input.GetMouseButtonUp(0) && isTouchSwiping)
            {
                isTouchSwiping = false;
                Vector2 delta = (Vector2)Input.mousePosition - swipeTouchStart;
                if (Mathf.Abs(delta.x) > 85f && Mathf.Abs(delta.y) < Mathf.Abs(delta.x) * 1.2f)
                {
                    if (delta.x < 0) OnNextClicked();
                    else OnPrevClicked();
                }
            }
        }

        private static Transform cachedModalLayer;

        public static bool IsAnyModalOpen()
        {
            if (AdService.Instance != null && !AdService.Instance.IsAdReady) return true;

            if (cachedModalLayer != null)
            {
                for (int i = 0; i < cachedModalLayer.childCount; i++)
                {
                    if (cachedModalLayer.GetChild(i).gameObject.activeInHierarchy) return true;
                }
            }
            return false;
        }

        private bool TryCloseTopModal()
        {
            if (unlockModal != null && unlockModal.activeSelf)
            {
                CloseUnlockModal();
                return true;
            }

            Transform modalLayer = cachedModalLayer != null ? cachedModalLayer : transform.Find("ModalLayer");
            if (modalLayer != null)
            {
                for (int i = modalLayer.childCount - 1; i >= 0; i--)
                {
                    GameObject child = modalLayer.GetChild(i).gameObject;
                    if (child.activeSelf)
                    {
                        if (PrizeWinModalUI.Instance != null && child.name == "PrizeWinModal")
                        {
                            PrizeWinModalUI.Instance.Dismiss();
                        }
                        else
                        {
                            child.SetActive(false);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public void SetCatalog(MachineDefinition[] machines)
        {
            availableMachines = machines;
            UpdateCarouselDisplay();
        }

        public void OnPrevClicked()
        {
            if (IsAnyModalOpen()) return;
            if (machineController != null && !machineController.CanSwitchMachine) return;
            if (availableMachines == null || availableMachines.Length <= 1) return;
            int target = currentMachineIndex - 1;
            if (target < 0) target = availableMachines.Length - 1;
            TryNavigateToMachine(target);
        }

        public void OnNextClicked()
        {
            if (IsAnyModalOpen()) return;
            if (machineController != null && !machineController.CanSwitchMachine) return;
            if (availableMachines == null || availableMachines.Length <= 1) return;
            int target = (currentMachineIndex + 1) % availableMachines.Length;
            TryNavigateToMachine(target);
        }

        private void TryNavigateToMachine(int targetIndex)
        {
            if (targetIndex < 0 || targetIndex >= availableMachines.Length) return;

            // Block switching while claw is active or carrying a prize
            if (machineController != null && !machineController.CanSwitchMachine)
            {
                return;
            }

            MachineDefinition target = availableMachines[targetIndex];
            if (target == null) return;

            ICollectionService collService = ServiceLocator.Get<ICollectionService>();

            bool isUnlocked = collService == null || collService.IsMachineUnlocked(target.machineId);

            if (isUnlocked)
            {
                SwitchToMachine(targetIndex);
            }
            else
            {
                OpenUnlockModal(target, targetIndex);
            }
        }

        private Coroutine titleBounceRoutine;

        private void BounceTitle()
        {
            if (machineNameText == null) return;
            if (titleBounceRoutine != null) StopCoroutine(titleBounceRoutine);
            titleBounceRoutine = StartCoroutine(TitleBounceRoutine());
        }

        private System.Collections.IEnumerator TitleBounceRoutine()
        {
            float elapsed = 0f;
            float duration = 0.25f;
            Vector3 punch = Vector3.one * 1.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                machineNameText.transform.localScale = Vector3.Lerp(punch, Vector3.one, t);
                yield return null;
            }
            machineNameText.transform.localScale = Vector3.one;
            titleBounceRoutine = null;
        }

        private void SwitchToMachine(int index)
        {
            currentMachineIndex = index;
            MachineDefinition target = availableMachines[currentMachineIndex];

            if (machineController != null)
            {
                machineController.SwipeToMachine(index, target);
            }
            else
            {
                ServiceLocator.Get<ICollectionService>()?.SetCurrentMachine(target);
            }

            BounceTitle();
            var audioSvc = ServiceLocator.Get<IAudioService>();
            if (audioSvc != null)
            {
                audioSvc.PlayCarouselSwipe();
                audioSvc.TriggerHapticLight();
            }

            UpdateCarouselDisplay();
            Debug.Log($"[ArcadeConsoleUI] Switched to machine: {target.displayName}");
        }

        private void UpdateCarouselDisplay()
        {
            if (availableMachines == null || availableMachines.Length == 0 || currentMachineIndex >= availableMachines.Length) return;
            MachineDefinition cur = availableMachines[currentMachineIndex];
            if (cur == null) return;

            ICollectionService collService = ServiceLocator.Get<ICollectionService>();

            if (machineNameText != null)
            {
                machineNameText.text = cur.displayName.ToUpper();
            }

            if (machineStatusText != null)
            {
                if (collService != null && collService.IsMachineOwned(cur.machineId))
                {
                    machineStatusText.text = "★ FREE PLAY ★";
                    machineStatusText.color = new Color(0.28f, 1.0f, 0.52f, 1f);
                }
                else if (collService != null && !collService.IsMachineUnlocked(cur.machineId))
                {
                    machineStatusText.text = $"LOCKED • {cur.unlockCost} COINS";
                    machineStatusText.color = new Color(1.0f, 0.45f, 0.45f, 1f);
                }
                else
                {
                    machineStatusText.text = cur.entryCost > 0 ? $"INSERT COIN: {cur.entryCost}" : "★ FREE PLAY ★";
                    machineStatusText.color = new Color(0.25f, 0.90f, 1.0f, 1f);
                }
            }

            if (cachedPushButton == null) cachedPushButton = GetComponentInChildren<ArcadePushButtonUI>() ?? FindFirstObjectByType<ArcadePushButtonUI>();
            if (cachedPushButton != null) cachedPushButton.SetThemeColor(cur.cabinetFrameColor);

            if (cachedJoystick == null) cachedJoystick = GetComponentInChildren<ArcadeJoystickUI>() ?? FindFirstObjectByType<ArcadeJoystickUI>();
            if (cachedJoystick != null) cachedJoystick.SetThemeColor(cur.cabinetFrameColor);
        }

        private void OpenUnlockModal(MachineDefinition machine, int targetIndex)
        {
            pendingUnlockMachine = machine;
            if (unlockModal == null) return;

            UIStyleHelper.EnsureModalShell(unlockModal, "UNLOCK CABINET", CloseUnlockModal, 580f, 620f);
            if (unlockConfirmButton != null) UIStyleHelper.StyleButton(unlockConfirmButton);
            if (unlockCancelButton != null) unlockCancelButton.gameObject.SetActive(false);

            int currentCoins = 0;
            if (ServiceLocator.TryGet<IEconomyService>(out var econ))
            {
                currentCoins = econ.Coins;
            }

            bool canAfford = currentCoins >= machine.unlockCost;

            if (unlockTitleText != null)
            {
                unlockTitleText.text = $"UNLOCK {machine.displayName.ToUpper()}";
            }
            if (unlockDescriptionText != null)
            {
                unlockDescriptionText.text = $"Unlock access to the {machine.displayName} arcade cabinet with exclusive retro collectibles!";
            }
            if (unlockCoinsBalanceText != null)
            {
                unlockCoinsBalanceText.text = $"Cost: {machine.unlockCost} Coins  |  Balance: {currentCoins} Coins";
                unlockCoinsBalanceText.color = canAfford ? new Color(1f, 0.85f, 0.2f) : new Color(0.95f, 0.35f, 0.35f);
            }
            if (unlockConfirmButton != null)
            {
                unlockConfirmButton.interactable = canAfford;
            }
            if (unlockConfirmButtonText != null)
            {
                unlockConfirmButtonText.text = canAfford ? $"UNLOCK ({machine.unlockCost} COINS)" : "NOT ENOUGH COINS";
            }

            unlockModal.SetActive(true);
            Transform card = unlockModal.transform.Find("DialogCard") ?? unlockModal.transform;
            UIStyleHelper.AnimatePopIn(this, card);
            ServiceLocator.Get<IAudioService>()?.PlayModalOpen();
        }

        private void OnConfirmUnlockClicked()
        {
            if (pendingUnlockMachine == null) return;

            bool success = false;
            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                success = coll.TryUnlockMachine(pendingUnlockMachine);
            }

            if (success)
            {
                ServiceLocator.Get<IAudioService>()?.PlayUnlockFanfare();
                CloseUnlockModal();
                // Find index and switch
                for (int i = 0; i < availableMachines.Length; i++)
                {
                    if (availableMachines[i] == pendingUnlockMachine)
                    {
                        SwitchToMachine(i);
                        break;
                    }
                }
            }
            else
            {
                ServiceLocator.Get<IAudioService>()?.PlayErrorBuzz();
                if (unlockConfirmButtonText != null)
                {
                    unlockConfirmButtonText.text = "NOT ENOUGH COINS!";
                }
            }
        }

        public void CloseUnlockModal()
        {
            pendingUnlockMachine = null;
            if (unlockModal != null)
            {
                unlockModal.SetActive(false);
            }
        }

        // Swipe handling
        public void OnBeginDrag(PointerEventData eventData)
        {
            dragStartPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Pass-through
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - dragStartPos;
            if (Mathf.Abs(delta.x) > SWIPE_THRESHOLD && Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                if (delta.x < 0)
                {
                    // Swipe left -> Next machine
                    OnNextClicked();
                }
                else
                {
                    // Swipe right -> Previous machine
                    OnPrevClicked();
                }
            }
        }

        private void SetupThemedUIElements()
        {
            BindElement(prevMachineButton != null ? prevMachineButton.gameObject : null, Theming.ThemeTargetType.ButtonBackground);
            BindElement(nextMachineButton != null ? nextMachineButton.gameObject : null, Theming.ThemeTargetType.ButtonBackground);
            BindElement(unlockConfirmButton != null ? unlockConfirmButton.gameObject : null, Theming.ThemeTargetType.ButtonBackground);

            var images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                var img = images[i];
                if (img == null) continue;
                string n = img.gameObject.name;

                if (n == "MarqueeRim")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.FrameBorder);
                }
                else if (n == "ConsoleRim")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.Secondary);
                }
                else if (n == "CarouselStrip")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.Secondary);
                }
                else if (n == "Rivet")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.RivetBulb);
                }
                else if (n == "DialogCard")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.FrameBorder);
                }
                else if (n == "Header" && img.transform.parent != null && img.transform.parent.name.EndsWith("Modal"))
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.HeaderBanner);
                }
                else if (n == "Btn_Awesome" || n == "Btn_SellDuplicates")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.ButtonBackground);
                }
                else if (n == "UnlockModal" || n == "CollectionModal" || n == "ProgressModal" || n == "SettingsModal" || n == "CelebrationModal")
                {
                    BindElement(img.gameObject, Theming.ThemeTargetType.Surface);
                }
            }
        }

        private void BindElement(GameObject go, Theming.ThemeTargetType targetType)
        {
            if (go == null) return;
            var themed = go.GetComponent<Theming.ThemedUIElement>();
            if (themed == null)
            {
                themed = go.AddComponent<Theming.ThemedUIElement>();
            }
            themed.TargetType = targetType;
        }
    }
}
