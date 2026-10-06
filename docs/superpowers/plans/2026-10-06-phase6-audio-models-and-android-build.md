# Phase 6: Production Polish, Audio, 3D Models & Android Build Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Provide native short Android haptic vibration triggers via Android Java wrappers (instead of 400ms Handheld.Vibrate), audio mixer slots for background music, external 3D model swap documentation/hooks, and verify build settings.

**Architecture:**
- `ClawAudio.cs`: Implement Android `VibrationEffect` 25ms one-shot buzz for crisp physical tactile feedback. Add BGM playback channel.
- `ClawConfiguration.cs`: Verify tuning ranges.
- `docs/superpowers/plans/`: Record full asset import guidelines for dropping in Blender/FBX models.

---

### Task 1: Native Short Android Haptics & BGM Support

**Files:**
- Modify: `Assets/Scripts/Gameplay/ClawAudio.cs`

- [ ] **Step 1: Implement crisp Android native vibration**
Replace obsolete `Handheld.Vibrate()` with Android `Vibrator` / `VibrationEffect.createOneShot(25, DEFAULT_AMPLITUDE)`.

---

### Task 2: 3D Model Swap Pipeline Guideline

**Files:**
- Create: `docs/superpowers/specs/2026-10-06-3d-model-asset-pipeline.md`

- [ ] **Step 1: Document model swap specification**
Detail vertex budget, collider requirements, and how to assign new custom prefabs to `PrizeDefinition.prefab`.

- [ ] **Step 2: Commit Phase 6 changes**
Commit changes with clean conventional commit message.
