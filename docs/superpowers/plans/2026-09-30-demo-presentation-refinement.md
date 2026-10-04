# Demo Presentation Refinement Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Elevate the ClawMachine demo from a rough greybox prototype into an eye-popping, highly polished, tactile arcade presentation with post-processing, smooth machine transitions, realistic glass & cable visuals, juicy win popups, and clean service architecture.

**Architecture:** 
1. **Visual Atmosphere**: URP Global Volume (Bloom, ACES Tonemapping, Vignette), cabinet glass material, and interior spot lighting.
2. **Carousel Dynamics**: Smooth camera glide/lerp and color cross-fade in `ArcadeConsoleUI` and `MachineController` when cycling between the 6 machines.
3. **Tactile Claw & Chute**: Dynamic rope/cable line renderer for the claw drop; prize chute collection delivery visual.
4. **Presentation & Juice**: Celebratory 3D Prize Inspection Win modal; crisp UI glow and particle polish.
5. **Architecture Decoupling**: Isolate `EconomyService` from `CollectionManager` god-node, adhering cleanly to `IEconomyService`.

**Tech Stack:** Unity 6000.3.19f1, URP (Universal Render Pipeline), TextMeshPro, C# .NET Standard 2.1.

---

### Task 1: URP Post-Processing, Lighting & Cabinet Glass Shader

**Files:**
- Create: `Assets/Materials/Cabinet/Mat_CabinetGlass.mat`
- Create: `Assets/Settings/PostProcess/ArcadeGlobalVolume.asset`
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs:70-85`
- Test: Verify in Unity Play Mode that Bloom glows on bright/metallic prizes and glass shows specular sheen without obscuring visibility.

- [ ] **Step 1: Create Glass Material and Post-Processing Volume Assets**
Create transparent URP Lit material for cabinet glass (Surface: Transparent, Blend: Alpha, Smoothness: 0.92, Metallic: 0.1, Color: (0.85, 0.95, 1.0, 0.12)).
Configure Global Volume with:
- Bloom: Threshold 1.1, Intensity 0.8, Scatter 0.7
- Tonemapping: ACES
- Vignette: Intensity 0.25, Smoothness 0.4

- [ ] **Step 2: Update Cabinet Glass & Interior Spotlight in Scene Setup**
Assign `Mat_CabinetGlass` to the 3 glass walls (Front, Left, Right) in `Phase0SceneSetup.cs`.
Add an interior warm spotlight (angle 75, intensity 1.5) above the prize floor pointing down for dramatic lighting.

- [ ] **Step 3: Test and Commit**
Verify cabinet looks enclosed with subtle reflections, and metallic/glow prizes pop with bloom.
```bash
git add Assets/Materials/Cabinet/ Assets/Settings/ Assets/Scripts/Editor/Phase0SceneSetup.cs
git commit -m "feat(visuals): add URP bloom post-processing, spotlight, and cabinet glass material"
```

---

### Task 2: Smooth Machine Carousel Transition

**Files:**
- Modify: `Assets/Scripts/Gameplay/MachineController.cs:60-100`
- Modify: `Assets/Scripts/UI/ArcadeConsoleUI.cs:175-210`
- Test: Cycle machines in game with `<` / `>` or `Q` / `E`. Verify smooth transition without frame freeze or snapping.

- [ ] **Step 1: Add Smooth Lerp Coroutine in MachineController**
In `MachineController.cs`, implement `SmoothTransitionMachine(MachineDefinition target, float duration = 0.4f)`:
- Fade cabinet frame and backdrop colors smoothly over 0.4s.
- Temporarily lock claw input during transition.
- Clear remaining prizes with a quick shrink or drop fade and repopulate new machine prizes cleanly.

- [ ] **Step 2: Integrate Transition in ArcadeConsoleUI**
In `ArcadeConsoleUI.cs`, update `SwitchToMachine`:
- Trigger `machineController.TransitionToMachine(target)` with audio transition stinger.
- Animate machine banner header with a slight punch/scale bounce (`Vector3.one * 1.1f` lerping back to `1.0f`).

- [ ] **Step 3: Test and Commit**
Verify switching through all 6 machines feels responsive, seamless, and visually appealing.
```bash
git add Assets/Scripts/Gameplay/MachineController.cs Assets/Scripts/UI/ArcadeConsoleUI.cs
git commit -m "feat(carousel): add smooth color lerp and prize transition between arcade machines"
```

---

### Task 3: Claw Cable Visual & Chute Collection Delivery

**Files:**
- Create: `Assets/Scripts/Gameplay/ClawCableVisual.cs`
- Modify: `Assets/Scripts/Gameplay/ClawController.cs:120-150`
- Modify: `Assets/Scripts/Gameplay/ChuteDetector.cs:60-90`
- Test: Drop claw, verify cable follows smooth vertical movement and chute sparkles on delivery.

- [ ] **Step 1: Implement ClawCableVisual**
Create `ClawCableVisual.cs` attached to Claw Trolley:
- Uses a `LineRenderer` with steel cable material or procedural segment tiling between the trolley ceiling anchor and the claw base.
- Updates in `LateUpdate` to match claw swaying and pendulum swings accurately.

- [ ] **Step 2: Add Chute Delivery Presentation**
In `ChuteDetector.cs`, add a brief chute spotlight flash and particle sparkle when a prize successfully falls through the funnel into the prize tray.

- [ ] **Step 3: Test and Commit**
Drop the claw and grab prizes; verify cable swings naturally with inertia.
```bash
git add Assets/Scripts/Gameplay/ClawCableVisual.cs Assets/Scripts/Gameplay/ClawController.cs Assets/Scripts/Gameplay/ChuteDetector.cs
git commit -m "feat(physics): add dynamic line-renderer claw cable and chute delivery juice"
```

---

### Task 4: Juicy Win Celebration Modal (3D Rotating Prize Preview)

**Files:**
- Create: `Assets/Scripts/UI/PrizeWinModalUI.cs`
- Modify: `Assets/Scripts/Gameplay/MachineController.cs:195-225`
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs:1400-1450`
- Test: Win a prize; verify celebratory popup appears with floating 3D/stylized preview, rarity badge, and coin reward.

- [ ] **Step 1: Create PrizeWinModalUI**
Create `PrizeWinModalUI.cs` in `ModalLayer`:
- Shows "★ PRIZE WON! ★" banner with pulsing gold aura.
- Displays prize display name, rarity tag (Normal, Rare, Secret), and duplicate value or "NEW UNLOCK!".
- Dismisses on `[ CLAIM PRIZE ]` tap or after 4 seconds.

- [ ] **Step 2: Wire Win Celebration in MachineController**
In `MachineController.HandlePrizeCollected`, invoke `PrizeWinModalUI.ShowPrize(prize)`.
Plays confetti celebration and audio fanfare.

- [ ] **Step 3: Test and Commit**
Win a toy; verify popup displays clearly on `ModalLayer` and collects cleanly.
```bash
git add Assets/Scripts/UI/PrizeWinModalUI.cs Assets/Scripts/Gameplay/MachineController.cs Assets/Scripts/Editor/Phase0SceneSetup.cs
git commit -m "feat(ui): add celebratory prize win modal with rarity and reward breakdown"
```

---

### Task 5: Clean Service Decoupling (IEconomyService vs ICollectionService)

**Files:**
- Create: `Assets/Scripts/Gameplay/EconomyService.cs`
- Modify: `Assets/Scripts/Gameplay/CollectionManager.cs:30-80`
- Modify: `Assets/Scripts/Core/Services/ServiceLocator.cs`
- Test: Verify coin earning, deductions, and upgrades continue to operate without regression.

- [ ] **Step 1: Extract EconomyService**
Implement `EconomyService.cs` implementing `IEconomyService`:
- Handles `Coins`, `TryDeductPlayCost`, `AwardCoins`, upgrade purchasing and levels.
- Registers with `ServiceLocator.Register<IEconomyService>(this)`.

- [ ] **Step 2: Slim Down CollectionManager**
Remove redundant coin tracking from `CollectionManager.cs`. Have `CollectionManager` focus exclusively on prize discovery, machine ownership, and catalog queries via `ICollectionService`.

- [ ] **Step 3: Test and Commit**
Verify all modals (Collection, Progress, OfflineEarnings, ArcadeConsole) read smoothly from `IEconomyService` and `ICollectionService`.
```bash
git add Assets/Scripts/Gameplay/EconomyService.cs Assets/Scripts/Gameplay/CollectionManager.cs
git commit -m "refactor(services): cleanly decouple EconomyService from CollectionManager"
```
