# Phase 3: Grab Drama, Physics Feel & Particles Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Overhaul the claw grab from a binary distance check into an authentic skill-based grab tension loop with dynamic slip chances, near-miss physical nudges, cascade knock-ins, and juice particle effects (lift sparks, cable strain, dust puff, win sparkles).

**Architecture:**
- `ClawController.cs`: Calculate `gripStability` based on accuracy, prize weight, and upgrade level.
- `ClawGripAnchor.cs`: Support delayed slips during lift or auto-return carriage sway.
- `ClawJuiceEffects.cs`: Add lift cable strain sparks and impact bursts.

---

### Task 1: Dynamic Grip Evaluation & Physical Nudge

**Files:**
- Modify: `Assets/Scripts/Gameplay/ClawController.cs`

- [ ] **Step 1: Replace binary grab with dynamic grip evaluation**
In `ClawController.SetState(ClawState.Descending)` & `EvaluatingGrip`:
Compute quality = `(1f - distance / radius)`.
If `quality >= 0.70f`: 100% stable clean grab.
If `0.35f <= quality < 0.70f`: Teetering grab. Grip will slip after 1.5s - 3.5s of lift, or upon carriage acceleration during auto-return!
If `quality < 0.35f`: Physical nudge! Apply gentle downward & outward physics impulse to nearby toys so they tumble, creating tension and possible cascade wins into the chute.

---

### Task 2: Lift Tension Sparks & Enhanced Juice Effects

**Files:**
- Modify: `Assets/Scripts/Gameplay/ClawJuiceEffects.cs`
- Modify: `Assets/Scripts/Gameplay/ClawController.cs`

- [ ] **Step 1: Add cable strain sparks / dust particles in ClawJuiceEffects**
Implement `PlayCableStrainSparks(Vector3 position)` and `PlayPrizeNudgeFX(Vector3 position)`.

- [ ] **Step 2: Commit Phase 3 changes**
Commit changes with clean conventional commit message.
