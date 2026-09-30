# Phase 5: Game Feel & Retention Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement active arcade-skill claw pendulum sway, dynamic cinematic camera tracking, physical chute ramp delivery, and a comprehensive 7-day streak + daily quest retention loop.

**Architecture:**
- **Claw Sway**: `ClawPendulumSway.cs` calculates 2D angular harmonic pendulum motion driven by trolley acceleration. Claw descent follows sway offset; reticle projection reflects true nadir.
- **Dynamic Camera**: `ClawCameraController.cs` listens to `ClawState` transitions and smoothly zooms/pans between wide view, descent tension, lift follow, and chute win focus.
- **Retention**: `IRetentionService` and `RetentionService.cs` manage a 7-day login streak calendar and 3 daily rotating quests stored in `PlayerCollectionData`.
- **UI**: `ProgressModalUI.cs` unifies Upgrades and Rewards into a 2-tab skeuomorphic modal on the top canopy marquee (`[⚡ PROGRESS]`).

**Tech Stack:** Unity 6 C# (6000.3.19f1), PhysX, ServiceLocator, JSON file save.

---

### Task 1: Active Claw Cable Sway (`ClawPendulumSway.cs`)

**Files:**
- Create: `Assets/Scripts/Gameplay/ClawPendulumSway.cs`
- Modify: `Assets/Scripts/Gameplay/ClawController.cs`

- [ ] **Step 1: Create `ClawPendulumSway.cs`**
  - Implement 2D harmonic damped oscillation ($\ddot{\theta} + 2\zeta\omega\dot{\theta} + \omega^2\theta = -\frac{a_{trolley}}{L}$).
  - Expose `GetSwayOffset()`, `GetSwayRotation()`, and `CurrentCableLength`.
  - Dampen amplitude and frequency as claw descends.
- [ ] **Step 2: Integrate Sway with `ClawController.cs`**
  - Offset visual ClawHub and gripSocket by sway offset and rotation during Aiming and Descending.
  - Reticle shadow projection matches swing nadir.
- [ ] **Step 3: Commit Task 1**
  - `git commit -m "feat(gameplay): active claw pendulum sway and swing aim projection"`

---

### Task 2: Dynamic Cinematic Camera (`ClawCameraController.cs`)

**Files:**
- Create: `Assets/Scripts/Gameplay/ClawCameraController.cs`
- Modify: `Assets/Scripts/Gameplay/MachineController.cs`

- [ ] **Step 1: Create `ClawCameraController.cs`**
  - State machine listening to `ClawState`:
    - `Aiming`: FOV 52°, default wide framing.
    - `Descending` / `Closing`: FOV 40°, smooth zoom focusing on claw hub & prize pile.
    - `Lifting` / `Carrying`: FOV 48°, tracking claw ascent and transit.
    - `Releasing` / `Win`: Brief downward pan to chute delivery tray.
  - Integrates shake dampening from `IAudioService`.
- [ ] **Step 2: Wire Camera to `MachineController.cs`**
  - MachineController initializes and syncs camera target on machine switch.
- [ ] **Step 3: Commit Task 2**
  - `git commit -m "feat(camera): dynamic cinematic camera zoom and state tracking"`

---

### Task 3: Chute Guide Ramp & Physical Tray Delivery

**Files:**
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs`
- Modify: `Assets/Scripts/Gameplay/ChuteDetector.cs`

- [ ] **Step 1: Add physical chute ramp & viewing tray in `Phase0SceneSetup.cs`**
  - Build slanted ramp inside chute collider ($25^\circ$).
  - Add front-facing clear delivery tray where prize slides into view.
- [ ] **Step 2: Update `ChuteDetector.cs` trigger placement**
  - Position sensor at base of tray so prize slides down before triggering collection.
- [ ] **Step 3: Commit Task 3**
  - `git commit -m "feat(chute): inclined chute guide ramp and prize collection tray"`

---

### Task 4: Retention & Daily Quest Engine (`IRetentionService.cs` + `RetentionService.cs`)

**Files:**
- Create: `Assets/Scripts/Core/Services/IRetentionService.cs`
- Create: `Assets/Scripts/Gameplay/RetentionService.cs`
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Create `IRetentionService.cs`**
  - Methods: `GetStreakDay()`, `CanClaimDaily()`, `ClaimDailyReward()`, `GetActiveQuests()`, `ClaimQuestReward(string questId)`.
- [ ] **Step 2: Extend `PlayerCollectionData` in `CollectionManager.cs`**
  - Add `lastDailyClaimUtc`, `dailyStreak`, and `dailyQuests` list.
- [ ] **Step 3: Implement `RetentionService.cs`**
  - Register with `ServiceLocator`.
  - Daily login streak rewards (50 -> 500 🪙).
  - 3 daily quests (drops, wins, rarities) tracking live gameplay events.
- [ ] **Step 4: Commit Task 4**
  - `git commit -m "feat(retention): 7-day streak calendar and daily quest engine"`

---

### Task 5: Unified Progress Modal UI (`ProgressModalUI.cs`)

**Files:**
- Create: `Assets/Scripts/UI/ProgressModalUI.cs`
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs`

- [ ] **Step 1: Create `ProgressModalUI.cs`**
  - Tab 0: Upgrades (Speed, Grip, Precision cards with purchase buttons).
  - Tab 1: Rewards (7-day streak visual cards + 3 daily quest progress bars & claim buttons).
- [ ] **Step 2: Update Marquee Canopy in `Phase0SceneSetup.cs`**
  - Rename middle canopy button to `[⚡ PROGRESS]`.
  - Build 2-tab modal hierarchy with both views under `SafeAreaContainer`.
- [ ] **Step 3: Commit Task 5**
  - `git commit -m "feat(ui): unified progress modal with upgrades and rewards tabs"`

---

### Task 6: Verification & Scene Regeneration

**Files:**
- Scene: `Assets/Scenes/Phase0_Prototype.unity`

- [ ] **Step 1: Compile Check**
  - Verify zero compiler errors across all new and modified scripts.
- [ ] **Step 2: Document Unity Execution Instructions**
  - Rebuild scene via `ClawMachine -> Build Phase 0 Prototype Scene`.
  - Verify sway, camera zoom, quests, and tabs in Play mode.
- [ ] **Step 3: Commit Phase 5 Completion**
  - `git commit -m "chore(release): phase 5 complete - game feel and retention systems"`
