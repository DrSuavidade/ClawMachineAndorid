using System;
using UnityEngine;
using UnityEngine.UI;
using ClawMachine.Gameplay;
using ClawMachine.Core.Services;

namespace ClawMachine.UI
{
    public class OfflineEarningsModalUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject modalPanel;
        [SerializeField] private Text awayTimeText;
        [SerializeField] private Text earnedCoinsText;
        [SerializeField] private Button collectButton;

        private void Awake()
        {
            AutoFindRefsIfNull();
            if (modalPanel != null) modalPanel.SetActive(false);
            SetupCollectButton();
        }

        private void Start()
        {
            AutoFindRefsIfNull();
            SetupCollectButton();

            if (ServiceLocator.TryGet<ICollectionService>(out var collService) && collService is CollectionManager cm)
            {
                cm.OnOfflineEarningsPending += ShowOfflineEarnings;
                if (cm.PendingOfflineCoins > 0)
                {
                    ShowOfflineEarnings(cm.PendingOfflineCoins, cm.PendingOfflineTime);
                }
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var collService) && collService is CollectionManager cm)
            {
                cm.OnOfflineEarningsPending -= ShowOfflineEarnings;
            }
        }

        private void AutoFindRefsIfNull()
        {
            if (modalPanel == null)
            {
                var found = transform.Find("OfflineEarningsModal");
                if (found == null)
                {
                    var go = GameObject.Find("OfflineEarningsModal");
                    if (go != null) found = go.transform;
                }
                if (found != null) modalPanel = found.gameObject;
            }

            if (modalPanel != null)
            {
                if (awayTimeText == null)
                {
                    var t = modalPanel.transform.Find("DialogCard/TimeText");
                    if (t != null) awayTimeText = t.GetComponent<Text>();
                }
                if (earnedCoinsText == null)
                {
                    var c = modalPanel.transform.Find("DialogCard/CoinsText");
                    if (c != null) earnedCoinsText = c.GetComponent<Text>();
                }
                if (collectButton == null)
                {
                    var b = modalPanel.transform.Find("DialogCard/Btn_Collect");
                    if (b != null) collectButton = b.GetComponent<Button>();
                }
            }
        }

        private void SetupCollectButton()
        {
            if (collectButton != null)
            {
                collectButton.onClick.RemoveAllListeners();
                collectButton.onClick.AddListener(OnCollectClicked);

                var clicker = collectButton.GetComponent<OfflineCollectPointerClicker>();
                if (clicker == null) clicker = collectButton.gameObject.AddComponent<OfflineCollectPointerClicker>();
                clicker.targetModal = this;
            }
        }

        public void ShowOfflineEarnings(int coins, TimeSpan awayTime)
        {
            AutoFindRefsIfNull();
            SetupCollectButton();

            if (modalPanel != null) modalPanel.SetActive(true);

            string timeStr;
            if (awayTime.TotalHours >= 1.0)
            {
                timeStr = $"{(int)awayTime.TotalHours}h {awayTime.Minutes}m";
            }
            else
            {
                timeStr = $"{awayTime.Minutes}m {awayTime.Seconds}s";
            }

            if (awayTimeText != null)
            {
                awayTimeText.text = $"TIME AWAY: {timeStr}";
            }

            if (earnedCoinsText != null)
            {
                earnedCoinsText.text = $"+{coins:N0} 🪙";
            }
        }

        public void OnCollectClicked()
        {
            Debug.Log("[OfflineEarningsModalUI] OnCollectClicked triggered!");
            if (ServiceLocator.TryGet<ICollectionService>(out var collService) && collService is CollectionManager cm)
            {
                cm.ClaimOfflineEarnings();
            }

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                audio.PlayWin();
            }

            ClawJuiceEffects.Instance?.PlayWinCelebration(transform.position);

            if (modalPanel != null)
            {
                modalPanel.SetActive(false);
            }
        }
    }

    public class OfflineCollectPointerClicker : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public OfflineEarningsModalUI targetModal;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (targetModal == null) targetModal = UnityEngine.Object.FindFirstObjectByType<OfflineEarningsModalUI>();
            if (targetModal != null)
            {
                targetModal.OnCollectClicked();
            }
        }
    }
}
