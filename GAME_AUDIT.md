# Claw Machine Android - Game Audit

## Summary
Claw Machine Android is a Unity-based arcade claw machine game where players control a claw to grab prizes from various themed machines. The game features multiple machines with different themes, a collection system, economy (coins, gems), progression, and various UI systems. The project uses URP (Universal Render Pipeline) and TextMeshPro for UI.

### Architecture
- **Engine:** Unity 6000.3.25f1 with Universal Render Pipeline (URP)
- **Primary Languages:** C#
- **Key Systems:**
  - **Claw Control:** `ClawController.cs` handles claw movement, states (Aiming, Descending, Closing, EvaluatingGrip, Lifting, Carrying, Returning, Releasing, Resolving).
  - **Machine Management:** `MachineController.cs` handles machine swapping (carousel), input, and coordination with claw and other systems.
  - **Services:** 
    - `CollectionManager.cs` implements `IEconomyService` and `ICollectionService` (economy, inventory, passive income, set bonuses).
    - `ClawAudio.cs` implements `IAudioService`.
    - `AdService.cs` implements `IAdService`.
    - `RetentionService.cs` implements `IRetentionService`.
    - `JsonFileSaveService.cs` implements `ISaveService`.
  - **Data Models:** `MachineDefinition.cs`, `PrizeDefinition.cs`, `ClawConfiguration.cs`.
  - **UI:** Various UI panels (ArcadeConsole, MachineControls, CollectionUI, modals for prizes, progress, settings, offline earnings).
  - **Audio:** Procedural audio (placeholder) with plans to replace with real clips.
  - **Particle Systems:** Basic particle effects for coins, confetti, etc.

## Current Verified State
- **Compilation:** The project compiles successfully with no script compilation errors.
- **Build:** The project builds successfully for Linux Standalone (StandaloneLinux64) using Unity CLI.
- **Editor Load:** The Unity Editor loads the project, initializes all systems, and enters batch mode without errors.
- **Scene:** The only scene is `Phase0_Prototype.unity`, which appears to be a functional prototype of the claw machine gameplay loop.
- **Dependencies:** All required Unity packages are present (as seen in `Packages/manifest.json` and the Package Manager log).

## Verified Findings and Classification
Based on inspection of the current codebase, we classify each claimed issue or missing feature as follows:

### Verified Defects (Issues present in current code)
| ID | Description | Evidence | Confidence |
|----|-------------|----------|------------|
| B1 | Duplicate coins double payout: `CollectionManager.RegisterCollectedPrize` awards coins for duplicates and then again via `SellAllDuplicates`. | In `CollectionManager.cs`, line ~333: duplicate prize triggers `AddCoins(reward)`; `SellAllDuplicates` also awards coins for duplicates. | High |
| B7 | Duplication of juice effect for first-time discovery: `MachineController.HandlePrizeCollected` and `PrizeWinModalUI.ShowWinDefinition` both call `ClawJuiceEffects.Instance?.PlayWinCelebration`. | In `MachineController.HandlePrizeCollected` (line ~385): `ClawJuiceEffects.Instance?.PlayWinCelebration(winPos);`<br>In `PrizeWinModalUI.ShowWinDefinition` (line ~200): `ClawJuiceEffects.Instance?.PlayWinCelebration(Vector3.up * 1.5f);` | High |
| B11 (part) | Application identifier incorrect: should be `com.drsuavidade.clawmachine` but is `com.drsuavidade.clawmachinearcade`. | In `ProjectSettings/ProjectSettings.asset`: `applicationIdentifier: Android: com.drsuavidade.clawmachinearcade` | High |
| B11 (part) | Default screen orientation incorrect: should be Portrait (0) but is Portrait Upside Down (1). | In `ProjectSettings/ProjectSettings.asset`: `defaultScreenOrientation: 1` | High |

### Verified Missing Requirements (Features not present but required per roadmap)
| ID | Description | Evidence | Confidence |
|----|-------------|----------|------------|
| B9 | Debug IMGUI box in `ClawGripAnchor` should be wrapped in `#if UNITY_EDITOR && CLAW_DEBUG` to prevent accidental inclusion in builds. | In `ClawGripAnchor.cs`: `OnGUI` method is not wrapped in any conditional compilation directive. | High |
| B4-B6 (part) | Machine switching buttons (prev/next) not blocked when modal is open. | In `ArcadeConsoleUI.OnPrevClicked`/`OnNextClicked`: no check for `ArcadeConsoleUI.IsAnyModalOpen()`. | High |
| B5 | Conflicting full-screen drag mode (floating screen drag) in `ArcadeJoystickUI.HandleScreenTouchMode` should be removed to unify input to joystick only. | In `ArcadeJoystickUI.Update`: calls `HandleScreenTouchMode()` when joystick is armed. | Medium |
| B6 (part) | Drop button UI input not explicitly blocked when modal is open (relies on implicit raycast blocking). | In `MachineControlsUI`: no explicit modal check for drop button UI interaction; relies on modal raycast blocking. | Medium |

### Design Proposals (Suggestions for improvement, not defects)
| ID | Description | Evidence |
|----|-------------|----------|
| B2 | Offline passive income calculation: already correct in current code (uses owned machines only). | In `CollectionManager.LoadData()`: calculates `totalRatePerMin` from `data.ownedMachineIds`. |
| B3 | Set bonus (+50% speed/grip for owned machines): already implemented. | In `CollectionManager.GetTrolleySpeedMultiplier()` and `GetGripPowerMultiplier()`: applies 1.5x multiplier when `IsMachineOwned(currentMachine.machineId)`. |
| B8 | Camera shake decoupling: already implemented (shake logic in `ClawCameraController`, triggered via `ClawAudio.TriggerCameraShake`). | In `ClawCameraController`: `TriggerShake` method and shake applied in `LateUpdate`.<br>In `ClawAudio`: `TriggerCameraShake` forwards to `ClawCameraController`. |
| B10 | Reset progress: already reloads scene to clear state. | In `CollectionManager.ResetProgress()`: calls `SceneManager.LoadScene(sceneIndex)`. |
| B11 (part) | Product name and company name: already correct. | In `ProjectSettings/ProjectSettings.asset`: `productName: Claw Machine Arcade`, `companyName: DrSuavidade`. |

### Unverified Hypotheses (Claims not supported by code inspection)
| ID | Description | Reason |
|----|-------------|--------|
| None | All claims from the audit have been either verified as defects, missing requirements, design proposals, or verified as already correct. | — |

## Proposed Prioritized Development Phases
Based on the verified findings, we recommend the following phases. Each phase addresses a set of verified defects and missing requirements. Refer to `DEVELOPMENT_PLAN.md` for detailed tasks.

1. **Phase 1: Critical Defects & Missing Requirements (P0)**
   - Fix B1 (duplicate coins double payout).
   - Fix B7 (juice effect duplication).
   - Fix B11 (application identifier and screen orientation).
   - Address B9 (debug GUI wrapping).
   - Address B4-B6 (part) (machine switching buttons modal blocking).
   - Address B5 (remove conflicting full-screen drag mode).
   - Address B6 (part) (explicit modal blocking for drop button UI and machine switching buttons).

2. **Phase 2: Unified Input & Real-Arcade Auto-Return (P1)**
   - Implement continuous joystick input, remove conflicting screen drags, modal raycast blocking, and the full auto-return sequence (Returning state, travel to chute, release, return home).
   - Files: `ClawController.cs`, `ArcadeJoystickUI.cs`, `MachineControlsUI.cs`, `ArcadeConsoleUI.cs`.

3. **Phase 3: Grab Drama, Physics Feel & Particles (P2)**
   - Implement dynamic grip evaluation (clean grip, tension slip, near-miss nudge), lift tension sparks, enhanced juice effects (cable strain, dust puff, prize sparkle).
   - Files: `ClawController.cs`, `ClawGripAnchor.cs`, `ClawJuiceEffects.cs`.

4. **Phase 4: Economy Balance & Rewarded Ad Monetization (P3)**
   - Balance machine entry costs, implement spawn weights (Normal 70%, Rare 25%, Secret 5%), dual currency (coins + gems), rewarded ad system for free plays.
   - Files: `CollectionManager.cs`, `PrizeSpawner.cs`, `AdService.cs` (implement real ad integration), `IAdvertisementService`, new UI for ad prompts.

5. **Phase 5: URP Graphics & TextMeshPro UI Modernization (P4)**
   - Complete URP migration (ensure URP asset is set as active, convert custom shaders to URP Shader Graph), migrate all UI Text to TextMeshPro, implement UI sprite icons, consistent rarity palette.
   - Files: URP assets, `Assets/Scripts/UI/*` (replace Text with TMP), shader conversions.

6. **Phase 6: Production Polish, Audio, Models & Android Release (P5, P6)**
   - Replace procedural audio with real clips (motor hum, claw clank, grab chord, chute drop, win fanfare), add background music, short Android haptics.
   - Prepare 3D model replacement hooks in `PrizeDefinition`.
   - Final Android build settings: IL2CPP, ARM64+ARMv7, portrait lock, target frame rate 60.
   - Perform end-to-end device testing.

## Risks and Unknowns
- **Save System Compatibility:** Changes to data structures (e.g., fixing duplicate coin logic) may break existing save files. Need to implement migration or clear save data on version update.
- **Third-Party Services:** Rewarded ad integration depends on external ad networks (Unity LevelPlay, AdMob, etc.) and requires proper setup and testing.
- **3D Model Integration:** Replacing primitive prize colliders with 3D models may affect physics and grab mechanics; requires careful tuning.
- **Performance on Target Devices:** The game must run smoothly on a range of Android devices; URP migration helps but needs profiling.
- **Scope Creep:** The roadmap is extensive; prioritizing Phase 1-2 first ensures a stable baseline before adding more complex systems.

## Recommended Initial Tasks
Based on the above, the first tasks to tackle are the Phase 1 defects and missing requirements, as they are blocking and foundational. Specifically:
1. Fix duplicate coins double payout (B1) in `CollectionManager.cs`.
2. Fix juice effect duplication (B7) in `MachineController.HandlePrizeCollected` and `PrizeWinModalUI.ShowWinDefinition`.
3. Fix application identifier (B11) in `ProjectSettings/ProjectSettings.asset`.
4. Fix default screen orientation (B11) in `ProjectSettings/ProjectSettings.asset`.
5. Wrap debug GUI in `ClawGripAnchor.cs` with `#if UNITY_EDITOR && CLAW_DEBUG` (B9).
6. Add modal blocking checks for machine switching buttons in `ArcadeConsoleUI.OnPrevClicked`/`OnNextClicked` (B4-B6 part).
7. Remove conflicting full-screen drag mode (`HandleScreenTouchMode`) in `ArcadeJoystickUI` (B5).
8. Add explicit modal blocking for drop button UI in `MachineControlsUI` (B6 part).

These tasks will stabilize the economy and core systems, allowing safer implementation of gameplay enhancements in later phases.

