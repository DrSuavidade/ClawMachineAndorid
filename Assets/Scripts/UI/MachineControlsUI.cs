using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClawMachine.Gameplay;

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
            }
        }

        private void Update()
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

            // Direct mouse drag fallback
            if (Input.GetMouseButtonDown(0))
            {
                lastPointerPos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0) && (Input.mousePosition.y < Screen.height * 0.65f))
            {
                Vector2 currentPos = Input.mousePosition;
                Vector2 delta = (currentPos - lastPointerPos) * dragSensitivity;
                lastPointerPos = currentPos;
                if (delta.sqrMagnitude > 0.0001f && machine != null)
                {
                    machine.SetDragInput(delta);
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
            }
        }

        private void OnDisable()
        {
            if (machine != null)
            {
                machine.OnStateChanged -= UpdateStateUI;
                machine.OnPrizeWon -= HandlePrizeWon;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastPointerPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 delta = (eventData.position - lastPointerPos) * dragSensitivity;
            lastPointerPos = eventData.position;

            if (machine != null)
            {
                machine.SetDragInput(delta);
            }
        }

        public void OnDropClicked()
        {
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
            winBannerTimer = 3.0f;
            if (winsText != null)
            {
                string prizeName = prize.Definition != null ? prize.Definition.displayName : prize.name;
                winsText.text = $"★ WINNER! {prizeName} (Wins: {winsCount}) ★";
            }
        }
    }
}
