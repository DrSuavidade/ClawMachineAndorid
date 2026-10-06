# Phase 5: Game Feel & Retention Design

**Date**: 2026-09-30  
**Status**: Approved  
**Topic**: Active Claw Cable Sway, Dynamic Cinematic Camera, Physical Chute Tray Delivery, and Daily Rewards/Quests Retention Loop.

---

## 1. Claw Cable Sway (Active Skill Timing)
* **Goal**: Implement arcade-authentic cable pendulum inertia that directly affects drop aim.
* **Component**: `ClawPendulumSway.cs` (attached to hoist/claw body).
* **Kinematics**:
  * Tracks trolley acceleration and velocity.
  * Angular displacement $\theta_{X}, \theta_{Z}$ driven by trolley inertia, restored by gravity spring-damper ($k_{spring}, c_{damping}$).
  * Natural frequency $\omega = \sqrt{g / L_{cable}}$ where $L_{cable}$ increases as claw descends, reducing sway frequency and amplitude.
* **Skill Trajectory**:
  * Claw descent follows current pendulum angle and offset.
  * Projected reticle tracks true ground nadir under swinging claw, allowing player to time the swing apex or nadir drop.

---

## 2. Dynamic Cinematic Camera
* **Goal**: Add dramatic visual flair and tension during descent, grab, and win moments.
* **Component**: `ClawCameraController.cs` (controls Main Camera or dynamic framing target).
* **Behaviors**:
  * `Aiming`: Standard arcade cabinet wide view ($FOV \approx 52^\circ$).
  * `Descending` / `Closing` / `EvaluatingGrip`: Smooth lerp zoom to $FOV \approx 40^\circ$ tracking claw and prize pile.
  * `Lifting` / `Returning`: Follows claw upward, smoothly widening back to $48^\circ$ as it tracks toward chute.
  * `Releasing` / `Win`: Brief tilt-down toward chute exit tray.
  * Integrates with `IAudioService` camera shake on impacts.

---

## 3. Physical Chute Ramp & Tray Delivery
* **Geometry**: Interior chute ramp inclined at $25^\circ$ directing dropped prizes forward toward lower transparent tray / flap.
* **Flow**:
  * Claw releases prize over chute drop point.
  * Prize falls onto inclined ramp and slides forward into player view in the collection tray.
  * `ChuteDetector` sensor triggers collection event once prize settles in the tray.

---

## 4. Retention & Progression Loop
* **Components**: `IRetentionService`, `RetentionService`, extended `PlayerCollectionData`.
* **7-Day Daily Streak**:
  * Day 1: 50 🪙
  * Day 2: 75 🪙
  * Day 3: 100 🪙
  * Day 4: 150 🪙
  * Day 5: 200 🪙
  * Day 6: 300 🪙
  * Day 7: 500 🪙 (resets to Day 1 after Day 7 claim).
  * Cooldown: 20 hours to qualify as next day, 48 hours before streak resets.
* **Daily Quests (3 Active per day)**:
  * Seeded from daily date key.
  * Quest Types:
    1. Drop Count ("Execute 5 drops" -> 40 🪙)
    2. Prize Collection ("Win 2 prizes" -> 60 🪙)
    3. Rarity Hunt ("Catch 1 Rare or Secret prize" -> 100 🪙)
  * Real-time event tracking via `MachineController.OnStateChanged` and `ICollectionService.OnPrizeAwarded`.

---

## 5. Combined Progress Modal UI
* **Header Button**: Top marquee canopy middle button updated to **`[⚡ PROGRESS]`**.
* **Modal Structure** (`ProgressModalUI.cs`):
  * Replaces / supersedes `UpgradesModalUI` while preserving existing upgrade card logic.
  * Top Tab Bar:
    * `Tab 0: [⚡ UPGRADES]` -> Displays existing machine upgrade cards (Speed, Grip, Precision).
    * `Tab 1: [🎁 REWARDS]` -> Displays Daily Streak calendar bar (7 days) + 3 Daily Quest cards with live progress bars and `[CLAIM]` buttons.
