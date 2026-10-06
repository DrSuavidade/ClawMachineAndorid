# Phase 5: URP Graphics & TextMeshPro UI Modernization Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Configure Universal Render Pipeline (URP) in `Packages/manifest.json`, adapt custom toon outline shader for SRP compatibility, and convert legacy UI Text to TextMeshPro with consistent rarity color palettes.

**Architecture:**
- `Packages/manifest.json`: Add `"com.unity.render-pipelines.universal": "17.0.3"`.
- `Assets/Shaders/StylizedToonOutline.shader`: Add UniversalForward / UniversalPipeline subshader tag or fallback so toon shading renders with SRP batcher.
- UI Scripts (`ArcadeConsoleUI.cs`, `CollectionUI.cs`, `PrizeWinModalUI.cs`): Refactor references from `UnityEngine.UI.Text` to `TMPro.TextMeshProUGUI` and unify color constants.

---

### Task 1: Package Manifest URP Inclusion

**Files:**
- Modify: `Packages/manifest.json`

- [ ] **Step 1: Add URP dependency**
Add `"com.unity.render-pipelines.universal": "17.0.3"` to packages manifest.

---

### Task 2: URP-Compatible Toon Outline Shader

**Files:**
- Modify: `Assets/Shaders/StylizedToonOutline.shader`

- [ ] **Step 1: Add UniversalPipeline RenderPipeline Tag**
Ensure shader includes `Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }` and support for `UniversalForward` passes so URP forward renderer draws the stylized outlines seamlessly.

---

### Task 3: Unified Rarity Color Palette in UI

**Files:**
- Modify: `Assets/Scripts/UI/PrizeWinModalUI.cs`
- Modify: `Assets/Scripts/UI/CollectionUI.cs`

- [ ] **Step 1: Synchronize rarity colors**
Standardize across both screens:
- Normal = `#3DD4F2` (Bright cyan)
- Rare = `#C256E6` (Arcade purple)
- Secret = `#FFD026` (Gold shimmer)

- [ ] **Step 2: Commit Phase 5 changes**
Commit changes with clean conventional commit message.
