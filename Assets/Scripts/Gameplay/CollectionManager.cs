using System;
using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Data;

namespace ClawMachine.Gameplay
{
    public enum UpgradeType
    {
        TrolleySpeed,
        GripPower,
        DropPrecision
    }

    [Serializable]
    public class PlayerCollectionData
    {
        public int coins = 100;
        public List<string> discoveredPrizeIds = new List<string>();
        public List<string> unlockedMachineIds = new List<string> { "toy_box" };
        public List<string> ownedMachineIds = new List<string>();
        public int trolleySpeedLevel = 1;
        public int gripPowerLevel = 1;
        public int dropSpeedLevel = 1;
        public long lastPassiveIncomeTimestamp;
    }

    public class CollectionManager : MonoBehaviour
    {
        private const string SAVE_KEY = "ProjectClaw_SaveData";

        private static CollectionManager instance;
        public static CollectionManager Instance => instance;

        [Header("State")]
        [SerializeField] private PlayerCollectionData data = new PlayerCollectionData();
        [SerializeField] private MachineCatalog catalog;
        [SerializeField] private MachineDefinition currentMachine;

        public int Coins => data.coins;
        public MachineCatalog Catalog => catalog;
        public MachineDefinition CurrentMachine => currentMachine;
        public bool HasGoldenClawUnlocked => data.ownedMachineIds.Count > 0;

        public event Action<int> OnCoinsChanged;
        public event Action<PrizeDefinition, bool> OnPrizeRegistered; // (prize, isNew)
        public event Action<MachineDefinition> OnMachineCompleted;
        public event Action<MachineDefinition> OnMachineUnlocked;
        public event Action<MachineDefinition> OnCurrentMachineChanged;
        public event Action<UpgradeType, int> OnUpgradePurchased;

        private float passiveTimer;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (catalog == null)
            {
#if UNITY_EDITOR
                catalog = UnityEditor.AssetDatabase.LoadAssetAtPath<MachineCatalog>("Assets/Data/MachineCatalog.asset");
#endif
            }

            if (currentMachine == null && catalog != null && catalog.Count > 0)
            {
                currentMachine = catalog.GetMachine(0);
            }
            else if (currentMachine == null)
            {
#if UNITY_EDITOR
                currentMachine = UnityEditor.AssetDatabase.LoadAssetAtPath<MachineDefinition>("Assets/Data/Machine_ToyBox.asset");
#endif
            }

            LoadData();
        }

        private void Update()
        {
            // Passive income tick for owned machines
            if (data.ownedMachineIds.Count > 0)
            {
                passiveTimer += Time.deltaTime;
                if (passiveTimer >= 60f)
                {
                    passiveTimer = 0f;
                    int income = 0;
                    if (currentMachine != null && IsMachineOwned(currentMachine.machineId))
                    {
                        income += currentMachine.passiveIncomePerMinute;
                    }
                    if (income > 0)
                    {
                        AddCoins(income);
                        Debug.Log($"[CollectionManager] Generated {income} passive coins from owned machines!");
                    }
                }
            }
        }

        public void SetCurrentMachine(MachineDefinition def)
        {
            if (currentMachine == def) return;
            currentMachine = def;
            OnCurrentMachineChanged?.Invoke(def);
        }

        public bool IsMachineUnlocked(string machineId)
        {
            if (string.IsNullOrEmpty(machineId) || machineId == "toy_box") return true;
            return data.unlockedMachineIds != null && data.unlockedMachineIds.Contains(machineId);
        }

        public bool TryUnlockMachine(MachineDefinition machine)
        {
            if (machine == null) return false;
            if (IsMachineUnlocked(machine.machineId)) return true;

            if (data.coins >= machine.unlockCost)
            {
                data.coins -= machine.unlockCost;
                if (data.unlockedMachineIds == null) data.unlockedMachineIds = new List<string>();
                if (!data.unlockedMachineIds.Contains(machine.machineId))
                {
                    data.unlockedMachineIds.Add(machine.machineId);
                }
                OnCoinsChanged?.Invoke(data.coins);
                OnMachineUnlocked?.Invoke(machine);
                SaveData();
                Debug.Log($"[CollectionManager] Unlocked machine: {machine.displayName} for {machine.unlockCost} coins!");
                return true;
            }
            return false;
        }

        public bool IsPrizeDiscovered(string prizeId)
        {
            return data.discoveredPrizeIds.Contains(prizeId);
        }

        public bool IsMachineOwned(string machineId)
        {
            return data.ownedMachineIds.Contains(machineId);
        }

        public int GetDiscoveredCount(MachineDefinition machine)
        {
            if (machine == null || machine.prizes == null) return 0;
            int count = 0;
            for (int i = 0; i < machine.prizes.Length; i++)
            {
                if (machine.prizes[i] != null && IsPrizeDiscovered(machine.prizes[i].id))
                {
                    count++;
                }
            }
            return count;
        }

        public bool CanPlayMachine(MachineDefinition machine)
        {
            if (machine == null) return true;
            if (IsMachineOwned(machine.machineId)) return true;
            return data.coins >= machine.entryCost;
        }

        public bool TryDeductPlayCost(MachineDefinition machine)
        {
            if (machine == null) return true;
            if (IsMachineOwned(machine.machineId)) return true; // Free plays once owned!

            if (data.coins >= machine.entryCost)
            {
                data.coins -= machine.entryCost;
                OnCoinsChanged?.Invoke(data.coins);
                SaveData();
                return true;
            }
            return false;
        }

        public void AddCoins(int amount)
        {
            data.coins += amount;
            OnCoinsChanged?.Invoke(data.coins);
            SaveData();
        }

        public void RegisterCollectedPrize(Prize prize)
        {
            if (prize == null) return;

            PrizeDefinition def = prize.Definition;

            // Robust fallback if prize lacked definition: map to current machine's prize
            if (def == null && currentMachine != null && currentMachine.prizes != null)
            {
                for (int i = 0; i < currentMachine.prizes.Length; i++)
                {
                    if (currentMachine.prizes[i] != null && !IsPrizeDiscovered(currentMachine.prizes[i].id))
                    {
                        def = currentMachine.prizes[i];
                        break;
                    }
                }
                if (def == null && currentMachine.prizes.Length > 0)
                {
                    def = currentMachine.prizes[0];
                }
            }

            string prizeId = def != null ? def.id : prize.name;
            string prizeName = def != null ? def.displayName : prize.name;

            bool isNew = !data.discoveredPrizeIds.Contains(prizeId);

            if (isNew)
            {
                data.discoveredPrizeIds.Add(prizeId);
                Debug.Log($"[CollectionManager] NEW PRIZE DISCOVERED: {prizeName} (ID: {prizeId})!");

                if (currentMachine != null && !IsMachineOwned(currentMachine.machineId))
                {
                    int totalDiscovered = GetDiscoveredCount(currentMachine);
                    if (totalDiscovered >= 9)
                    {
                        data.ownedMachineIds.Add(currentMachine.machineId);
                        Debug.Log($"[CollectionManager] ★ MACHINE OWNED! {currentMachine.displayName} collection complete! ★");
                        OnMachineCompleted?.Invoke(currentMachine);
                    }
                }
            }
            else
            {
                int reward = def != null ? def.duplicateCoinValue : 15;
                AddCoins(reward);
                Debug.Log($"[CollectionManager] Duplicate prize {prizeName}. Awarded +{reward} coins!");
            }

            OnPrizeRegistered?.Invoke(def, isNew);
            SaveData();
        }

        public void SaveData()
        {
            data.lastPassiveIncomeTimestamp = DateTime.UtcNow.Ticks;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        public void LoadData()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                try
                {
                    data = JsonUtility.FromJson<PlayerCollectionData>(json) ?? new PlayerCollectionData();
                }
                catch
                {
                    data = new PlayerCollectionData();
                }
            }
            else
            {
                data = new PlayerCollectionData();
            }

            if (data.unlockedMachineIds == null)
            {
                data.unlockedMachineIds = new List<string>();
            }
            if (!data.unlockedMachineIds.Contains("toy_box"))
            {
                data.unlockedMachineIds.Add("toy_box");
            }

            // Calculate offline passive income
            if (data.ownedMachineIds.Count > 0 && data.lastPassiveIncomeTimestamp > 0)
            {
                TimeSpan elapsed = DateTime.UtcNow - new DateTime(data.lastPassiveIncomeTimestamp);
                int minutes = Mathf.Clamp((int)elapsed.TotalMinutes, 0, 480); // Cap at 8 hours
                if (minutes > 0 && currentMachine != null && IsMachineOwned(currentMachine.machineId))
                {
                    int offlineCoins = minutes * currentMachine.passiveIncomePerMinute;
                    data.coins += offlineCoins;
                    Debug.Log($"[CollectionManager] Welcome back! Earned {offlineCoins} coins while offline ({minutes}m).");
                }
            }
        }

        public int GetUpgradeLevel(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.TrolleySpeed => Mathf.Clamp(data.trolleySpeedLevel, 1, 5),
                UpgradeType.GripPower => Mathf.Clamp(data.gripPowerLevel, 1, 5),
                UpgradeType.DropPrecision => Mathf.Clamp(data.dropSpeedLevel, 1, 5),
                _ => 1
            };
        }

        public int GetUpgradeCost(UpgradeType type)
        {
            int lvl = GetUpgradeLevel(type);
            if (lvl >= 5) return -1; // Max level
            return lvl switch
            {
                1 => 45,
                2 => 85,
                3 => 150,
                4 => 250,
                _ => -1
            };
        }

        public bool TryPurchaseUpgrade(UpgradeType type)
        {
            int cost = GetUpgradeCost(type);
            if (cost < 0 || data.coins < cost) return false;

            data.coins -= cost;
            int newLvl = 1;

            switch (type)
            {
                case UpgradeType.TrolleySpeed:
                    data.trolleySpeedLevel = Mathf.Min(5, data.trolleySpeedLevel + 1);
                    newLvl = data.trolleySpeedLevel;
                    break;
                case UpgradeType.GripPower:
                    data.gripPowerLevel = Mathf.Min(5, data.gripPowerLevel + 1);
                    newLvl = data.gripPowerLevel;
                    break;
                case UpgradeType.DropPrecision:
                    data.dropSpeedLevel = Mathf.Min(5, data.dropSpeedLevel + 1);
                    newLvl = data.dropSpeedLevel;
                    break;
            }

            OnCoinsChanged?.Invoke(data.coins);
            OnUpgradePurchased?.Invoke(type, newLvl);
            SaveData();
            Debug.Log($"[CollectionManager] Upgraded {type} to Level {newLvl}!");
            return true;
        }

        public float GetTrolleySpeedMultiplier() => 1f + (GetUpgradeLevel(UpgradeType.TrolleySpeed) - 1) * 0.15f;
        public float GetGripPowerMultiplier() => 1f + (GetUpgradeLevel(UpgradeType.GripPower) - 1) * 0.20f;
        public float GetDropSpeedMultiplier() => 1f + (GetUpgradeLevel(UpgradeType.DropPrecision) - 1) * 0.18f;

        public void ResetProgress()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            data = new PlayerCollectionData();
            OnCoinsChanged?.Invoke(data.coins);
        }
    }
}
