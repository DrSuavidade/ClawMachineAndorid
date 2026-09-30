# Phase 3: Arcade Bottom Console & Machine Progression Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement physical arcade bottom console deck with horizontal machine switching, unlock flow (150 coins for Retro Arcade), and a full set of 9 Retro Arcade prizes with dynamic cabinet accent colors.

**Architecture:** Extend `MachineDefinition` with theme colors and unlock costs. Add `RetroArcadeAssetGenerator` for 9 procedural retro prizes. Create `ArcadeConsoleUI` as lower deck housing joystick touchpad, drop button, and swipe/carousel machine switcher. Handle unlock prompts and machine transitions in `CollectionManager` and `MachineController`.

**Tech Stack:** Unity 6 (6000.3.19f1), C#, UGUI, URP.

---

### Task 1: Extend Machine Data & Player Persistence

**Files:**
- Modify: `Assets/Scripts/Data/MachineDefinition.cs`
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Add theme colors and unlock cost to `MachineDefinition.cs`**
  - Add `public Color cabinetFrameColor = new Color(0.85f, 0.25f, 0.25f);`
  - Add `public Color cabinetBackdropColor = new Color(0.40f, 0.48f, 0.62f);`
  - Add `public int unlockCost = 0;`

- [ ] **Step 2: Add unlocked machine tracking in `CollectionManager.cs`**
  - Add `public List<string> unlockedMachineIds = new List<string> { "toy_box" };` to `PlayerCollectionData`.
  - Add `public bool IsMachineUnlocked(string machineId)` method.
  - Add `public bool TryUnlockMachine(MachineDefinition machine)` method (checks balance, deducts coins, saves).

---

### Task 2: Retro Arcade Asset Generator (9 Prizes)

**Files:**
- Create: `Assets/Scripts/Editor/RetroArcadeAssetGenerator.cs`

- [ ] **Step 1: Implement generator class**
  - Create 9 retro themed assets:
    1. `retro_cartridge` (Normal) - Pixel Cartridge
    2. `retro_sword` (Normal) - 8-Bit Pixel Sword
    3. `retro_floppy` (Normal) - 3.5" Floppy Disk
    4. `retro_token` (Normal) - Golden Arcade Token
    5. `retro_handheld` (Normal) - Portable Handheld Console
    6. `retro_crt` (Rare) - Vintage CRT Monitor
    7. `retro_vr` (Rare) - Cyber VR Headset
    8. `retro_keytar` (Rare) - 80s Synth Keytar
    9. `retro_golden_cabinet` (Secret) - Mini Gold Arcade Cabinet
  - Create and configure `Assets/Data/Machine_RetroArcade.asset` with:
    - `machineId`: `"retro_arcade"`
    - `displayName`: `"Retro Arcade"`
    - `unlockCost`: 150
    - `entryCost`: 10
    - `passiveIncomePerMinute`: 8
    - `cabinetFrameColor`: Magenta/Purple `(0.75f, 0.15f, 0.85f)`
    - `cabinetBackdropColor`: Dark Cyber Navy `(0.12f, 0.14f, 0.24f)`

---

### Task 3: Machine Switching & Cabinet Re-skinning

**Files:**
- Modify: `Assets/Scripts/Gameplay/MachineController.cs`
- Modify: `Assets/Scripts/Gameplay/PrizeSpawner.cs`

- [ ] **Step 1: Add cabinet re-skinning and respawn to `MachineController.cs`**
  - Add references to cabinet frame mesh renderers and backdrop wall mesh renderer.
  - Implement `public void ApplyMachine(MachineDefinition machine)`:
    - Update cabinet materials to `machine.cabinetFrameColor` and `machine.cabinetBackdropColor`.
    - Reset claw to home position.
    - Instruct `prizeSpawner.SetMachine(machine)` to repopulate pile.

- [ ] **Step 2: Add `SetMachine` to `PrizeSpawner.cs`**
  - Update `machineDefinition` and `prizePool`.
  - Clear current active prizes and spawn fresh initial pile.

---

### Task 4: Bottom Arcade Console UI & Unlock Modal

**Files:**
- Create: `Assets/Scripts/UI/ArcadeConsoleUI.cs`
- Modify: `Assets/Scripts/UI/CollectionUI.cs`

- [ ] **Step 1: Implement `ArcadeConsoleUI.cs`**
  - Integrated bottom 30% control deck.
  - Machine switcher strip with `<` and `>` buttons and swipe gesture detection.
  - Unlock confirmation modal: "Unlock [Machine] for [X] Coins?" with Confirm and Cancel.
  - Communicates with `MachineController` and `CollectionManager`.

- [ ] **Step 2: Update `CollectionUI.cs` for active machine context**
  - Ensure collection modal dynamically reflects whatever machine is currently active.

---

### Task 5: Scene Assembly & Verification

**Files:**
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs`

- [ ] **Step 1: Update scene builder**
  - Generate both Toy Box and Retro Arcade assets.
  - Build bottom console deck with new UI components.
  - Wire cabinet renderer references to `MachineController`.
  - Save scene to `Assets/Scenes/Phase0_Prototype.unity`.

- [ ] **Step 2: Verify in Unity**
  - Check `Editor.log` for zero compile errors.
  - Validate machine swap, unlock prompt, and prize spawning.
