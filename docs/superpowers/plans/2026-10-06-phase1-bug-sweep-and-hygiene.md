# Phase 1: Critical Bug Sweep & Architecture Hygiene Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix all 11 critical bugs identified in the audit (B1-B11), eliminate double payouts, fix false set bonuses, decouple camera shake from camera lerp, clean up double-fire events, and configure base Android metadata.

**Architecture:** Refactor `CollectionManager`, `ClawCameraController`, `ClawAudio`, `OfflineEarningsModalUI`, `ProgressModalUI`, and `ClawGripAnchor` to remove race conditions and economic exploits while preserving serializable save structure.

**Tech Stack:** C#, Unity 6000.3.25f1, uGUI.

---

### Task 1: Fix Duplicate Coins Double Payout (B1)

**Files:**
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Check duplicate registration logic**
In `RegisterCollectedPrize(Prize prize)`:
Currently line 333 gives `AddCoins(reward)` immediately for duplicates, *and* lines 298-307 increase the inventory count, allowing `SellAllDuplicates()` to pay a second time.
Change: Only increment inventory count on duplicate catch. Let player manually sell via "Sell Extra Duplicates", OR auto-sell and do not add extra to inventory. To preserve the collection tab's "Sell Extra Duplicates" button excitement, auto-sell on catch is removed, or count is not multiplied.
Decision: Duplicates store count in inventory; coins are awarded when sold in Collection UI.

- [ ] **Step 2: Apply the fix in CollectionManager.cs**
Update `RegisterCollectedPrize`:
```csharp
if (isNew)
{
    data.discoveredPrizeIds.Add(prizeId);
    reward = GetDiscoveryReward(def);
    AddCoins(reward);
    Debug.Log($"[CollectionManager] NEW PRIZE DISCOVERED: {prizeName} (+{reward} coins)");
    // check machine completion...
}
else
{
    // Duplicate added to inventory only; sold via CollectionUI Sell Duplicates button
    reward = 0;
    Debug.Log($"[CollectionManager] Duplicate prize {prizeName} stored in inventory (Total: {entry.count}).");
}
```

---

### Task 2: Fix Offline Passive Income Calculation (B2)

**Files:**
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Calculate total passive income from owned machines only**
Replace lines 475-481 in `LoadData()`:
```csharp
int totalOwnedRate = 0;
if (data.ownedMachineIds != null && catalog != null)
{
    for (int i = 0; i < data.ownedMachineIds.Count; i++)
    {
        var machineDef = catalog.GetMachine(data.ownedMachineIds[i]);
        if (machineDef != null)
        {
            totalOwnedRate += machineDef.passiveIncomePerMinute;
        }
    }
}
if (totalOwnedRate > 0)
{
    PendingOfflineCoins = minutes * totalOwnedRate;
    PendingOfflineTime = elapsed;
}
else
{
    PendingOfflineCoins = 0;
}
```

---

### Task 3: Implement Advertised Set Bonus (+50% Speed/Grip) (B3)

**Files:**
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Wire active machine owned check to multipliers**
Update `GetTrolleySpeedMultiplier()` and `GetGripPowerMultiplier()` in `CollectionManager.cs`:
```csharp
public float GetTrolleySpeedMultiplier()
{
    float baseMult = 1f + (GetUpgradeLevel(UpgradeType.TrolleySpeed) - 1) * 0.10f;
    if (currentMachine != null && IsMachineOwned(currentMachine.machineId))
    {
        baseMult *= 1.50f; // +50% Set bonus
    }
    return baseMult;
}

public float GetGripPowerMultiplier()
{
    float baseMult = 1f + GetUpgradeLevel(UpgradeType.GripPower) * 0.01f;
    if (currentMachine != null && IsMachineOwned(currentMachine.machineId))
    {
        baseMult *= 1.50f; // +50% Set bonus
    }
    return baseMult;
}
```

---

### Task 4: Camera Shake Decoupling (B8)

**Files:**
- Modify: `Assets/Scripts/Gameplay/ClawAudio.cs`
- Modify: `Assets/Scripts/Gameplay/ClawCameraController.cs`

- [ ] **Step 1: Move shake state to ClawCameraController**
Add `TriggerShake(float duration, float magnitude)` and manage `shakeOffset` inside `ClawCameraController.LateUpdate`:
Apply `transform.position = Vector3.Lerp(...) + shakeOffset;` in single place.
Remove shake application from `ClawAudio.LateUpdate`. Forward `ClawAudio.TriggerCameraShake` to `ClawCameraController`.

---

### Task 5: Eliminate Double-Fire Clickers & Event Duplication (B7, B9, B10)

**Files:**
- Modify: `Assets/Scripts/UI/OfflineEarningsModalUI.cs`
- Modify: `Assets/Scripts/Gameplay/MachineController.cs`
- Modify: `Assets/Scripts/Gameplay/ClawGripAnchor.cs`
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Remove redundant clicker in OfflineEarningsModalUI**
Remove `OfflineCollectPointerClicker` injection or deduplicate pointer clicks so `OnCollectClicked` only executes once per tap.
- [ ] **Step 2: Deduplicate Win Audio & Celebration**
Ensure `MachineController.HandlePrizeCollected` handles the physical chute celebration, and `PrizeWinModalUI` only handles the modal pop animation without re-triggering duplicated audio/shake.
- [ ] **Step 3: Hide debug IMGUI box in ClawGripAnchor (B9)**
Set `[SerializeField] private bool showDebugGui = false;` and wrap `OnGUI` in `#if UNITY_EDITOR && CLAW_DEBUG`.
- [ ] **Step 4: Fix Reset Progress (B10)**
In `ResetProgress()`, after clearing data, call `UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);` to guarantee complete state freshness.

---

### Task 6: Android App Identity & Git Cleanup

**Files:**
- Modify: `ProjectSettings/ProjectSettings.asset`
- Modify: `.gitignore`

- [ ] **Step 1: Update bundle identifier & orientation**
Set `applicationIdentifier` to `com.drsuavidade.clawmachine`, `companyName` to `DrSuavidade`, `productName` to `Claw Machine Arcade`, lock orientation to Portrait (`defaultScreenOrientation: 0` or `1`).
- [ ] **Step 2: Untrack graphify-out**
Execute `git rm -r --cached graphify-out/` and commit cleanup.
