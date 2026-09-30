# Modular Catalog Economy & Skeuomorphic Arcade Deck Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Transform content management into a modular ScriptableObject catalog with scaled economy tiers, and replace the thick bottom bar with a slim (16% height) skeuomorphic arcade deck featuring an interactive ball-top joystick (outline toggle + floating screen drag) and a 3D tactile drop button.

**Architecture:** 
1. Create `MachineCatalog.cs` as central ScriptableObject repository for all machine definitions, enabling instant modular expansion.
2. Scale economy curves: higher tiers have higher unlock/play costs and significantly larger duplicate payouts + passive yields.
3. Build `ArcadeJoystickUI.cs` to deliver an authentic arcade ball-top stick with armed outline state, dynamic touch centering, and knob deflection.
4. Build `ArcadePushButtonUI.cs` with raised chrome bezel and tactile plunge animation.
5. Restructure `ArcadeConsoleUI.cs` and `Phase0SceneSetup.cs` to fit the slim 16% deck footprint, maximizing playfield visibility to 84%.

**Tech Stack:** Unity 6 (6000.3.19f1), C#, UGUI, URP.

---

### Task 1: Modular MachineCatalog & Scaled Tier Economy

**Files:**
- Create: `Assets/Scripts/Data/MachineCatalog.cs`
- Modify: `Assets/Scripts/Editor/RetroArcadeAssetGenerator.cs`
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs`

- [ ] **Step 1: Create `MachineCatalog.cs` ScriptableObject**
  - Array of `MachineDefinition[] machines`.
  - Helper methods: `GetMachine(string id)`, `GetNextMachine(MachineDefinition cur)`, `GetPrevMachine(MachineDefinition cur)`.

- [ ] **Step 2: Update `RetroArcadeAssetGenerator.cs` economy values**
  - Increase duplicate coin rewards: Normal prizes = 30 coins, Rare prizes = 80-100 coins, Golden Cabinet Secret = 250 coins.
  - Set `passiveIncomePerMinute = 15` coins/min.
  - Set `unlockCost = 150`, `entryCost = 10`.

- [ ] **Step 3: Wire `MachineCatalog` into `CollectionManager.cs`**
  - Reference `MachineCatalog catalog` on `CollectionManager`.
  - Auto-resolve `catalog` from `Assets/Data/MachineCatalog.asset` if null.

---

### Task 2: Skeuomorphic Ball-Top Arcade Joystick

**Files:**
- Create: `Assets/Scripts/UI/ArcadeJoystickUI.cs`

- [ ] **Step 1: Implement `ArcadeJoystickUI.cs`**
  - Visual hierarchy:
    - Base Collar: Circular ring with metallic bevel.
    - Outline Ring: Glow outline that illuminates when joystick is active/armed.
    - Shaft & Ball Knob: Red glossy sphere knob that translates up to `maxRadius = 45px`.
  - State logic:
    - Tap on joystick: toggles `isArmed` state.
    - When `isArmed == true`, outline illuminates. Touch anywhere on the screen establishes a relative drag anchor; dragging deflects the joystick knob and sends input to `MachineController.SetDragInput()`.
    - Tap joystick again: disarms, turns off outline, centers knob.
    - Direct drag on the joystick base itself works seamlessly in both armed and unarmed modes.

---

### Task 3: Skeuomorphic Arcade Push Button

**Files:**
- Create: `Assets/Scripts/UI/ArcadePushButtonUI.cs`

- [ ] **Step 1: Implement `ArcadePushButtonUI.cs`**
  - Chrome outer bezel ring + convex red arcade plunger.
  - Uses `IPointerDownHandler`, `IPointerUpHandler`, `IPointerClickHandler`.
  - Pointer down: Plunger depresses downward by 6px with darkening tint.
  - Pointer up: Plunger springs back up with subtle bounce.
  - Click: Calls `MachineController.TriggerDrop()`.

---

### Task 4: Slim Skeuomorphic Control Deck (16% Screen Height)

**Files:**
- Modify: `Assets/Scripts/UI/ArcadeConsoleUI.cs`

- [ ] **Step 1: Update `ArcadeConsoleUI.cs` for slim console layout**
  - Support `MachineCatalog` asset binding.
  - Compact carousel center display with `<` `>` arrows.
  - Forward joystick and button events cleanly.

---

### Task 5: Scene Builder Integration & Verification

**Files:**
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs`

- [ ] **Step 1: Update `Phase0SceneSetup.cs`**
  - Generate/load `MachineCatalog.asset`.
  - Build slim 16% height console deck with corner bolts, `ArcadeJoystickUI`, `ArcadePushButtonUI`, and compact carousel.
  - Enlarge playfield drag area to 84% screen height.
  - Save scene to `Assets/Scenes/Phase0_Prototype.unity`.

- [ ] **Step 2: Compiler and Runtime Verification**
  - Check `Editor.log` for zero compile errors.
  - Verify joystick toggle, outline highlight, floating drag, and drop button animation.
