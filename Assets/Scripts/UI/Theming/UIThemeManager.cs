using System;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.UI.Theming
{
    [DefaultExecutionOrder(-50)]
    public class UIThemeManager : MonoBehaviour
    {
        public static UIThemeManager Instance { get; private set; }

        public static event Action<CabinetTheme> OnThemeChanged;

        [Header("Default Fallback Theme")]
        [SerializeField] private CabinetTheme fallbackTheme;

        [Header("Runtime State")]
        [SerializeField] private CabinetTheme currentTheme;

        public CabinetTheme CurrentTheme => currentTheme != null ? currentTheme : fallbackTheme;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (fallbackTheme == null)
            {
                fallbackTheme = ScriptableObject.CreateInstance<CabinetTheme>();
                fallbackTheme.themeId = "default_fallback";
                fallbackTheme.displayName = "Default Carnival";
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var collectionService))
            {
                collectionService.OnCurrentMachineChanged += HandleMachineChanged;
                if (collectionService.CurrentMachine != null)
                {
                    ApplyMachineTheme(collectionService.CurrentMachine);
                }
            }

            if (currentTheme == null)
            {
                ApplyTheme(fallbackTheme);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet<ICollectionService>(out var collectionService))
            {
                collectionService.OnCurrentMachineChanged -= HandleMachineChanged;
            }
        }

        private void HandleMachineChanged(MachineDefinition newMachine)
        {
            ApplyMachineTheme(newMachine);
        }

        public void ApplyMachineTheme(MachineDefinition machine)
        {
            if (machine != null && machine.theme != null)
            {
                ApplyTheme(machine.theme);
            }
            else
            {
                ApplyTheme(fallbackTheme);
            }
        }

        public void ApplyTheme(CabinetTheme theme)
        {
            currentTheme = theme != null ? theme : fallbackTheme;
            OnThemeChanged?.Invoke(currentTheme);
        }
    }
}
