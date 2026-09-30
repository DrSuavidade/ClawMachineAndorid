using System;
using System.Collections.Generic;
using UnityEngine;
using ClawMachine.Data;
using ClawMachine.Core.Services;

namespace ClawMachine.Gameplay
{
    public enum UpgradeType
    {
        TrolleySpeed,
        GripPower,
        DropPrecision
    }

    [Serializable]
    public class MachineUpgradeData
    {
        public string machineId;
        public int trolleySpeedLevel = 1;
        public int gripPowerLevel = 1;
        public int dropPrecisionLevel = 1;
    }

    [Serializable]
    public class PrizeInventoryEntry
    {
        public string prizeId;
        public int count = 1;
    }

    [Serializable]
    public class PlayerCollectionData
    {
        public int saveVersion = 2;
        public int coins = 100;
        public List<string> discoveredPrizeIds = new List<string>();
        public List<string> unlockedMachineIds = new List<string> { "toy_box" };
        public List<string> ownedMachineIds = new List<string>();
        public List<MachineUpgradeData> machineUpgrades = new List<MachineUpgradeData>();
        public List<PrizeInventoryEntry> inventory = new List<PrizeInventoryEntry>();
        // Legacy global fields — kept for v1 migration only
        public int trolleySpeedLevel = 1;
        public int gripPowerLevel = 1;
        public int dropSpeedLevel = 1;
        public long lastPassiveIncomeTimestamp;
    }

    public class CollectionManager : MonoBehaviour, IEconomyService, ICollectionService
    {
        private const string SAVE_KEY = "ProjectClaw_SaveData";
        private const int CURRENT_SAVE_VERSION = 2;

        private static CollectionManager instance;
        public static CollectionManager Instance => instance;

        private ISaveService saveService;

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
        public event Action<PrizeDefinition, bool, int> OnPrizeAwarded; // (prize, isNew, coinReward)
        public event Action<int> OnDuplicatesSold; // (totalCoinsEarned)
        public event Action<MachineDefinition> OnMachineCompleted;
        public event Action<MachineDefinition> OnMachineUnlocked;
        public event Action<MachineDefinition> OnCurrentMachineChanged;
        public event Action<UpgradeType, int> OnUpgradePurchased;

        private float passiveTimer;

        public void SetSaveService(ISaveService customSaveService)
        {
            saveService = customSaveService;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            if (saveService == null)
            {
                saveService = new JsonFileSaveService();
            }

            ServiceLocator.Register<IEconomyService>(this);
            ServiceLocator.Register<ICollectionService>(this);
            ServiceLocator.Register<ISaveService>(saveService);

            if (catalog == null)
            {
                Debug.LogWarning("[CollectionManager] MachineCatalog not assigned! Please assign it in inspector.");
            }

            if (currentMachine == null && catalog != null && catalog.Count > 0)
            {
                currentMachine = catalog.GetMachine(0);
            }
            else if (currentMachine == null)
            {
                Debug.LogWarning("[CollectionManager] No currentMachine and no catalog assigned! Please assign MachineCatalog in inspector.");
            }

            LoadData();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                ServiceLocator.Unregister<IEconomyService>();
                ServiceLocator.Unregister<ICollectionService>();
                ServiceLocator.Unregister<ISaveService>();
                instance = null;
            }
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

            // 1. Track inventory quantity
            if (data.inventory == null) data.inventory = new List<PrizeInventoryEntry>();
            var entry = data.inventory.Find(e => e.prizeId == prizeId);
            if (entry == null)
            {
                entry = new PrizeInventoryEntry { prizeId = prizeId, count = 1 };
                data.inventory.Add(entry);
            }
            else
            {
                entry.count++;
            }

            bool isNew = !data.discoveredPrizeIds.Contains(prizeId);
            int reward = 0;

            if (isNew)
            {
                data.discoveredPrizeIds.Add(prizeId);
                reward = GetDiscoveryReward(def);
                AddCoins(reward);
                Debug.Log($"[CollectionManager] ★ NEW PRIZE DISCOVERED: {prizeName} (ID: {prizeId})! Awarded +{reward} coins!");

                if (currentMachine != null && !IsMachineOwned(currentMachine.machineId))
                {
                    int totalDiscovered = GetDiscoveredCount(currentMachine);
                    if (totalDiscovered >= currentMachine.prizes.Length)
                    {
                        data.ownedMachineIds.Add(currentMachine.machineId);
                        Debug.Log($"[CollectionManager] ★ MACHINE OWNED! {currentMachine.displayName} collection complete! ★");
                        OnMachineCompleted?.Invoke(currentMachine);
                    }
                }
            }
            else
            {
                reward = GetDuplicateSellPrice(def);
                AddCoins(reward);
                Debug.Log($"[CollectionManager] Duplicate prize {prizeName} (Total Owned: {entry.count}). Awarded +{reward} coins!");
            }

            OnPrizeAwarded?.Invoke(def, isNew, reward);
            OnPrizeRegistered?.Invoke(def, isNew);
            SaveData();
        }

        public int GetPrizeCount(string prizeId)
        {
            if (string.IsNullOrEmpty(prizeId) || data.inventory == null) return 0;
            var entry = data.inventory.Find(e => e.prizeId == prizeId);
            if (entry != null) return entry.count;
            return data.discoveredPrizeIds.Contains(prizeId) ? 1 : 0;
        }

        public int GetDiscoveryReward(PrizeDefinition def)
        {
            if (def == null) return 30;
            return def.rarity switch
            {
                PrizeRarity.Secret => 250,
                PrizeRarity.Rare => 75,
                _ => 30
            };
        }

        public int GetDuplicateSellPrice(PrizeDefinition def)
        {
            if (def == null) return 15;
            return Mathf.Max(10, (int)(def.duplicateCoinValue * def.sellMultiplier));
        }

        public int GetTotalDuplicateValue()
        {
            if (data.inventory == null) return 0;
            int total = 0;
            foreach (var item in data.inventory)
            {
                if (item.count > 1)
                {
                    PrizeDefinition def = FindPrizeDefinition(item.prizeId);
                    int sellPrice = GetDuplicateSellPrice(def);
                    total += sellPrice * (item.count - 1);
                }
            }
            return total;
        }

        public int SellAllDuplicates()
        {
            if (data.inventory == null) return 0;

            int totalEarned = 0;
            int itemsSold = 0;

            foreach (var item in data.inventory)
            {
                if (item.count > 1)
                {
                    int extras = item.count - 1;
                    PrizeDefinition def = FindPrizeDefinition(item.prizeId);
                    int sellPrice = GetDuplicateSellPrice(def);
                    totalEarned += sellPrice * extras;
                    itemsSold += extras;
                    item.count = 1;
                }
            }

            if (totalEarned > 0)
            {
                AddCoins(totalEarned);
                OnDuplicatesSold?.Invoke(totalEarned);
                Debug.Log($"[CollectionManager] Sold {itemsSold} duplicates for +{totalEarned} coins!");
                SaveData();
            }

            return totalEarned;
        }

        public bool TrySellPrize(string prizeId, int count = 1)
        {
            if (string.IsNullOrEmpty(prizeId) || data.inventory == null) return false;
            var entry = data.inventory.Find(e => e.prizeId == prizeId);
            if (entry == null || entry.count <= count) return false;

            entry.count -= count;
            PrizeDefinition def = FindPrizeDefinition(prizeId);
            int earned = GetDuplicateSellPrice(def) * count;
            AddCoins(earned);
            OnDuplicatesSold?.Invoke(earned);
            SaveData();
            return true;
        }

        private PrizeDefinition FindPrizeDefinition(string prizeId)
        {
            if (catalog != null && catalog.Machines != null)
            {
                foreach (var m in catalog.Machines)
                {
                    if (m != null && m.prizes != null)
                    {
                        foreach (var p in m.prizes)
                        {
                            if (p != null && p.id == prizeId) return p;
                        }
                    }
                }
            }
            return null;
        }

        public void SaveData()
        {
            data.saveVersion = CURRENT_SAVE_VERSION;
            data.lastPassiveIncomeTimestamp = DateTime.UtcNow.Ticks;
            if (saveService == null) saveService = new JsonFileSaveService();
            saveService.Save(SAVE_KEY, data, CURRENT_SAVE_VERSION);
        }

        public void LoadData()
        {
            if (saveService == null) saveService = new JsonFileSaveService();
            data = saveService.Load<PlayerCollectionData>(SAVE_KEY, CURRENT_SAVE_VERSION, MigrateSaveDataPayload) ?? new PlayerCollectionData();

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

            MigrateSaveData();
        }

        private PlayerCollectionData MigrateSaveDataPayload(string json, int oldVersion)
        {
            try
            {
                var loaded = JsonUtility.FromJson<PlayerCollectionData>(json);
                return loaded ?? new PlayerCollectionData();
            }
            catch
            {
                return new PlayerCollectionData();
            }
        }

        private void MigrateSaveData()
        {
            if (data.saveVersion < 2)
            {
                // v1 → v2: Migrate global upgrades to first machine (toy_box)
                if (data.machineUpgrades == null)
                    data.machineUpgrades = new List<MachineUpgradeData>();

                data.machineUpgrades.Add(new MachineUpgradeData
                {
                    machineId = "toy_box",
                    trolleySpeedLevel = data.trolleySpeedLevel,
                    gripPowerLevel = data.gripPowerLevel,
                    dropPrecisionLevel = data.dropSpeedLevel
                });
                data.saveVersion = 2;
                Debug.Log("[CollectionManager] Migrated save data v1 → v2 (per-machine upgrades)");
            }

            if (data.saveVersion < CURRENT_SAVE_VERSION)
            {
                data.saveVersion = CURRENT_SAVE_VERSION;
                SaveData();
            }
        }

        private MachineUpgradeData GetOrCreateMachineUpgrades(string machineId)
        {
            if (data.machineUpgrades == null)
                data.machineUpgrades = new List<MachineUpgradeData>();

            for (int i = 0; i < data.machineUpgrades.Count; i++)
            {
                if (data.machineUpgrades[i].machineId == machineId)
                    return data.machineUpgrades[i];
            }

            var entry = new MachineUpgradeData { machineId = machineId };
            data.machineUpgrades.Add(entry);
            return entry;
        }

        private MachineUpgradeData CurrentMachineUpgrades =>
            currentMachine != null ? GetOrCreateMachineUpgrades(currentMachine.machineId) : null;

        public int GetUpgradeLevel(UpgradeType type)
        {
            var upg = CurrentMachineUpgrades;
            if (upg == null) return 1;
            return type switch
            {
                UpgradeType.TrolleySpeed => Mathf.Clamp(upg.trolleySpeedLevel, 1, 5),
                UpgradeType.GripPower => Mathf.Clamp(upg.gripPowerLevel, 1, 5),
                UpgradeType.DropPrecision => Mathf.Clamp(upg.dropPrecisionLevel, 1, 5),
                _ => 1
            };
        }

        public int GetUpgradeCost(UpgradeType type)
        {
            int lvl = GetUpgradeLevel(type);
            int[] defaultCosts = { 45, 85, 150, 250 };
            int idx = lvl - 1; // level 1 = index 0 (cost for lvl 1 -> 2)
            if (idx < 0 || idx >= defaultCosts.Length) return -1; // Max level reached
            return defaultCosts[idx];
        }

        public bool TryPurchaseUpgrade(UpgradeType type)
        {
            int cost = GetUpgradeCost(type);
            if (cost < 0 || data.coins < cost) return false;

            var upg = CurrentMachineUpgrades;
            if (upg == null) return false;

            data.coins -= cost;
            int newLvl = 1;

            switch (type)
            {
                case UpgradeType.TrolleySpeed:
                    upg.trolleySpeedLevel = Mathf.Min(5, upg.trolleySpeedLevel + 1);
                    newLvl = upg.trolleySpeedLevel;
                    break;
                case UpgradeType.GripPower:
                    upg.gripPowerLevel = Mathf.Min(5, upg.gripPowerLevel + 1);
                    newLvl = upg.gripPowerLevel;
                    break;
                case UpgradeType.DropPrecision:
                    upg.dropPrecisionLevel = Mathf.Min(5, upg.dropPrecisionLevel + 1);
                    newLvl = upg.dropPrecisionLevel;
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
            if (saveService == null) saveService = new JsonFileSaveService();
            saveService.Delete(SAVE_KEY);
            data = new PlayerCollectionData();
            OnCoinsChanged?.Invoke(data.coins);
        }
    }
}
