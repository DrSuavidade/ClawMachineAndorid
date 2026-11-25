using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClawMachine.Gameplay;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class MachineControlsUI : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [Header("Machine Reference")]
        [SerializeField] private MachineController machine;

        [Header("UI Elements")]
        [SerializeField] private Button dropButton;
        [SerializeField] private Text stateText;
        [SerializeField] private Text winsText;

        [Header("Tuning")]
        [SerializeField] private float dragSensitivity = 0.015f;

        private Vector2 lastPointerPos;
        private int winsCount;
        private float winBannerTimer;

        private void Awake()
        {
            if (machine == null)
            {
                machine = FindFirstObjectByType<MachineController>();
            }

            if (dropButton == null)
            {
                dropButton = GetComponentInChildren<Button>();
            }

            if (dropButton != null)
            {
                dropButton.onClick.AddListener(OnDropClicked);
                UIStyleHelper.StyleButton(dropButton);
            }

            if (stateText != null) stateText.gameObject.SetActive(false);
            if (winsText != null) winsText.gameObject.SetActive(false);
        }

        private RectTransform toastBannerRt;
        private Text toastBannerText;
        private CanvasGroup toastCanvasGroup;
        private float toastTimer;

        private void Update()
        {
            if (!ArcadeConsoleUI.IsAnyModalOpen())
            {
                // Keyboard controls
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
                {
                    if (machine != null)
                    {
                        machine.SetDragInput(new Vector2(h, v) * 0.8f);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    OnDropClicked();
                }
            }

            if (toastTimer > 0f && toastBannerRt != null)
            {
                toastTimer -= Time.deltaTime;
                float progress = 1f - Mathf.Clamp01(toastTimer / 2.2f);
                toastBannerRt.anchoredPosition = new Vector2(0f, 180f + progress * 90f);
                if (toastCanvasGroup != null)
                {
                    toastCanvasGroup.alpha = toastTimer < 0.5f ? (toastTimer / 0.5f) : 1f;
                }
                if (toastTimer <= 0f)
                {
                    toastBannerRt.gameObject.SetActive(false);
                }
            }

            if (winBannerTimer > 0f)
            {
                winBannerTimer -= Time.deltaTime;
                if (winsText != null)
                {
                    float pulse = 1f + 0.15f * Mathf.Sin(winBannerTimer * 16f);
                    winsText.transform.localScale = Vector3.one * pulse;
                    winsText.color = Color.Lerp(Color.yellow, new Color(1f, 0.45f, 0.1f), Mathf.PingPong(winBannerTimer * 5f, 1f));
                }
            }
            else if (winsText != null && winsText.transform.localScale != Vector3.one)
            {
                winsText.transform.localScale = Vector3.one;
                winsText.color = Color.yellow;
            }
        }

        private void OnEnable()
        {
            if (machine != null)
            {
                machine.OnStateChanged += UpdateStateUI;
                machine.OnPrizeWon += HandlePrizeWon;
                if (machine.Claw != null)
                {
                    machine.Claw.OnCarryTimerUpdated += HandleCarryTimerUpdated;
                }
            }
        }

        private void OnDisable()
        {
            if (machine != null)
            {
                machine.OnStateChanged -= UpdateStateUI;
                machine.OnPrizeWon -= HandlePrizeWon;
                if (machine.Claw != null)
                {
                    machine.Claw.OnCarryTimerUpdated -= HandleCarryTimerUpdated;
                }
            }
        }

        private void HandleCarryTimerUpdated(float remainingSeconds)
        {
            int sec = Mathf.CeilToInt(remainingSeconds);
            if (stateText != null)
            {
                stateText.text = $"CARRYING: DROP IN {sec}S";
            }
            if (dropButton != null)
            {
                Text btnText = dropButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = $"DROP ({sec}s)";
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastPointerPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (ArcadeConsoleUI.IsAnyModalOpen()) return;
            Vector2 delta = (eventData.position - lastPointerPos) * dragSensitivity;
            lastPointerPos = eventData.position;

            if (machine != null)
            {
                machine.SetDragInput(delta);
            }
        }

        public void OnDropClicked()
        {
            if (ArcadeConsoleUI.IsAnyModalOpen()) return;
            Debug.Log("[MachineControlsUI] DROP triggered!");
            if (machine != null)
            {
                machine.TriggerDrop();
            }
        }

        private void UpdateStateUI(ClawState state)
        {
            if (stateText != null)
            {
                stateText.text = $"STATUS: {state.ToString().ToUpper()}";
            }

            if (dropButton != null)
            {
                bool canAct = (state == ClawState.Aiming || state == ClawState.Carrying);
                dropButton.interactable = canAct;
                Text btnText = dropButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    if (state == ClawState.Carrying)
                    {
                        btnText.text = "DROP PRIZE";
                    }
                    else if (state == ClawState.Aiming)
                    {
                        btnText.text = "DROP";
                    }
                    else
                    {
                        btnText.text = "...";
                    }
                }
            }
        }

        private void HandlePrizeWon(Prize prize)
        {
            winsCount++;
            winBannerTimer = 3.5f;
            PrizeDefinition def = prize != null ? prize.Definition : null;
            string prizeName = def != null ? def.displayName : (prize != null ? prize.name : "PRIZE");

            int count = 1;
            int reward = 30;
            bool isNew = true;

            if (ServiceLocator.TryGet<ICollectionService>(out var coll))
            {
                string id = def != null ? def.id : "";
                // Note: HandlePrizeWon fires right before RegisterCollectedPrize in MachineController,
                // so IsPrizeDiscovered tells us accurately whether this is a first discovery!
                isNew = !coll.IsPrizeDiscovered(id);
                count = coll.GetPrizeCount(id) + 1;
            }

            if (isNew)
            {
                reward = def != null ? (def.rarity switch {
                    PrizeRarity.Secret => 250,
                    PrizeRarity.Rare => 75,
                    _ => 30
                }) : 30;
                if (winsText != null)
                {
                    winsText.text = $"NEW DISCOVERY! {prizeName.ToUpper()} (+{reward} COINS)";
                }
            }
            else
            {
                if (ServiceLocator.TryGet<IEconomyService>(out var econ) && def != null)
                {
                    reward = econ.GetDuplicateSellPrice(def);
                }
                else
                {
                    reward = def != null ? Mathf.Max(10, (int)(def.duplicateCoinValue * def.sellMultiplier)) : 15;
                }

                if (winsText != null)
                {
                    winsText.text = $"{prizeName.ToUpper()} x{count}! (+{reward} COINS)";
                }
                ShowDuplicateToast(prizeName, reward);
            }

            UIStyleHelper.SpawnFloatingCoinReward(this, reward, new Vector2(0f, 140f));
        }

        private void ShowDuplicateToast(string prizeName, int reward)
        {
            if (toastBannerRt == null)
            {
                Canvas rootCanvas = GetComponentInParent<Canvas>();
                Transform parentTf = rootCanvas != null ? rootCanvas.transform : transform;
                Font font = UIStyleHelper.MainFont;

                GameObject toastGo = new GameObject("DuplicateCoinToast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                toastGo.transform.SetParent(parentTf, false);
                toastBannerRt = toastGo.GetComponent<RectTransform>();
                toastBannerRt.anchorMin = new Vector2(0.5f, 0.5f);
                toastBannerRt.anchorMax = new Vector2(0.5f, 0.5f);
                toastBannerRt.sizeDelta = new Vector2(600f, 64f);
                toastCanvasGroup = toastGo.GetComponent<CanvasGroup>();
                toastCanvasGroup.blocksRaycasts = false;

                Image bg = toastGo.GetComponent<Image>();
                bg.sprite = UIStyleHelper.RoundedCardSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.08f, 0.12f, 0.20f, 0.95f);
                bg.raycastTarget = false;

                GameObject txtGo = new GameObject("ToastText", typeof(RectTransform), typeof(Text));
                txtGo.transform.SetParent(toastGo.transform, false);
                RectTransform txtRt = txtGo.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;

                toastBannerText = txtGo.GetComponent<Text>();
                toastBannerText.font = font;
                toastBannerText.fontSize = 22;
                toastBannerText.fontStyle = FontStyle.Bold;
                toastBannerText.alignment = TextAnchor.MiddleCenter;
                toastBannerText.color = new Color(1f, 0.88f, 0.22f, 1f);
                toastBannerText.raycastTarget = false;
                UIStyleHelper.AddDropShadow(toastBannerText, new Color(0f, 0f, 0f, 0.7f), new Vector2(1.5f, -2f));
            }

            toastBannerText.text = $"+{reward} COINS  DUPLICATE SOLD! ({prizeName.ToUpper()})";
            toastBannerRt.anchoredPosition = new Vector2(0f, 180f);
            if (toastCanvasGroup != null) toastCanvasGroup.alpha = 1f;
            toastBannerRt.gameObject.SetActive(true);
            toastTimer = 2.2f;
        }
    }
}
