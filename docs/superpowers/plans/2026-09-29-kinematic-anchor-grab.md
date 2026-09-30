# Kinematic Anchor Claw Grab Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a stable, glitch-free kinematic anchor grab system with procedural sway, deterministically evaluated hold thresholds, and natural physics detach.

**Architecture:** 
- `ClawGripAnchor`: Manages kinematic attachment to `GripSocket`, ignores collisions with claw prongs while held, applies acceleration-based procedural sway, and handles detach restoring dynamic Rigidbody velocity.
- `GripEvaluator`: Calculates deterministic grip score based on prize proximity to center axis, depth inside claw, and prong coverage. Determines whether prize reaches chute or slips mid-travel.
- `ClawController`: Updates state transitions to use `ClawGripAnchor` and `GripEvaluator`.

**Tech Stack:** Unity 6 C#, PhysX Rigidbody, UGUI.

---

### Task 1: Create GripEvaluator

**Files:**
- Create: `Assets/Scripts/Gameplay/GripEvaluator.cs`

- [ ] **Step 1: Write GripEvaluator script**
Calculates normalized grip quality (0.0 to 1.0) and evaluates whether grab is valid, stable (reaches chute), or weak (slips mid-transit).

- [ ] **Step 2: Verify GripEvaluator compilation**

---

### Task 2: Create ClawGripAnchor

**Files:**
- Create: `Assets/Scripts/Gameplay/ClawGripAnchor.cs`

- [ ] **Step 1: Write ClawGripAnchor script**
Handles anchoring prize to GripSocket, setting `isKinematic = true`, ignoring prong collisions, applying procedural sway from trolley/hoist motion, and clean release restoring dynamic physics with momentum.

- [ ] **Step 2: Verify ClawGripAnchor compilation**

---

### Task 3: Integrate into ClawController and Scene

**Files:**
- Modify: `Assets/Scripts/Gameplay/ClawController.cs`
- Modify: `Assets/Scripts/Editor/Phase0SceneSetup.cs`

- [ ] **Step 1: Update ClawController to drive ClawGripAnchor during grab, lift, returning, and releasing**
- [ ] **Step 2: Update Phase0SceneSetup to wire ClawGripAnchor and GripEvaluator to the ClawAssembly**
- [ ] **Step 3: Remove obsolete GripAssist dependencies**

---

### Task 4: Verification and Play Test

- [ ] **Step 1: Run menu build or update scene**
- [ ] **Step 2: Verify zero collision explosion and reliable prize transport**
