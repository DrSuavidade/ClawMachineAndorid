# Phase 2: Unified Input & Real-Arcade Auto-Return Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Unify claw controls to a single dedicated virtual joystick on the lower console half, prevent inputs while modals are open, restrict carousel machine switching to top header / explicit swipe, and implement authentic arcade auto-return sequence (`ClawState.Returning` -> Chute -> Release -> Return Home).

**Architecture:** Refactor `ClawController.cs` to execute the full auto-return journey. Update `ArcadeJoystickUI.cs` to provide continuous directional input while pressed. Refactor `MachineControlsUI.cs` and `ArcadeConsoleUI.cs` to eliminate input collision and enforce modal raycast blocking.

**Tech Stack:** C#, Unity 6000.3.25f1, uGUI event interfaces.

---

### Task 1: Complete Real-Arcade Auto-Return Sequence

**Files:**
- Modify: `Assets/Scripts/Gameplay/ClawController.cs`

- [ ] **Step 1: Wire AscendHoist into ClawState.Returning**
In `AscendHoist()`, when `pos.y >= config.homeY`:
If `gripAnchor != null && gripAnchor.HasPrize`:
Transition to `ClawState.Returning` (instead of `ClawState.Carrying`).
If no prize was caught: transition directly back to `ClawState.Aiming`.

- [ ] **Step 2: Implement smooth trolley transit to chute in TravelToChute**
In `TravelToChute()`:
Move trolley towards `chuteDropPoint` at `config.returnSpeed`.
When trolley is within 0.05m of chute coordinates, transition to `ClawState.Releasing`.

- [ ] **Step 3: Auto-return claw back to machine center after release**
In `ClawState.Resolving`:
Once `resolveTimer <= 0f`:
Add smooth return motion back to initial machine aim center (or keep at chute until next move), then transition to `ClawState.Aiming`.

---

### Task 2: Continuous Joystick Input & Remove Conflicting Screen Drags

**Files:**
- Modify: `Assets/Scripts/UI/ArcadeJoystickUI.cs`
- Modify: `Assets/Scripts/UI/MachineControlsUI.cs`

- [ ] **Step 1: Update ArcadeJoystickUI to feed continuous movement**
Store `currentInputVector` on `OnDrag` and keep feeding it to `machineController.SetDragInput()` in `Update()` as long as the finger is held down. Clear to `Vector2.zero` on `OnEndDrag`.

- [ ] **Step 2: Clean up MachineControlsUI redundant full-screen drag**
Remove legacy mouse-drag zone in `MachineControlsUI.Update()` that was fighting the joystick. Keep keyboard arrows/WASD for editor testing.

---

### Task 3: Modal Raycast Blocking & Carousel Boundary Separation

**Files:**
- Modify: `Assets/Scripts/UI/ArcadeConsoleUI.cs`

- [ ] **Step 1: Prevent machine swipe during active claw states or modal open**
Check `EventSystem.current.IsPointerOverGameObject()` and modal visibility before processing touch swipes. Only trigger machine navigation on carousel arrows or deliberate swipe on top header area (>60% screen height).

- [ ] **Step 2: Commit Phase 2 changes**
Commit changes with clean conventional commit message.
