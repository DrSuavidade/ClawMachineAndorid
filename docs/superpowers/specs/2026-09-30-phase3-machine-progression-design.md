# Phase 3 Design Spec: Arcade Bottom Console & Machine Progression

## 1. Overview
Phase 3 expands Project Claw from a single machine to a multi-machine collection arcade. It introduces an integrated bottom arcade control panel, horizontal machine navigation with slide transitions, unlockable machines (Retro Arcade for 150 coins), and a full set of 9 Retro Arcade prizes.

---

## 2. Core Features & Systems

### 2.1 Machine Data & Theming
- **`MachineDefinition` Enhancements**:
  - `cabinetFrameColor`: Accent color for arcade cabinet frame pillars.
  - `cabinetBackdropColor`: Backdrop wall color.
  - `unlockCost`: Coins required to unlock access to machine (0 for Toy Box, 150 for Retro Arcade).
- **Machine 2 ("Retro Arcade")**:
  - `machineId`: `retro_arcade`
  - `displayName`: "Retro Arcade"
  - `entryCost`: 10 coins / play (free once completed/owned)
  - `unlockCost`: 150 coins
  - `passiveIncomePerMinute`: 8 coins
  - 9 unique 3D procedural prizes (Pixel Cartridge, 8-Bit Sword, Floppy Disk, Arcade Token, Handheld Console, CRT Monitor, VR Headset, Synth Keytar, Golden Arcade Cabinet).

### 2.2 Bottom Console UI (`ArcadeConsoleUI`)
- Replaces raw overlay controls with an arcade machine lower deck interface:
  - **Console Panel**: Metallic/matte textured dashboard across bottom 30% of screen.
  - **Claw Joystick / Drag Touchpad**: Integrated into left/center of console.
  - **Arcade DROP Button**: Large tactile push button on right with press feedback.
  - **Machine Carousel Bar**: Displays `< [Current Machine Name] >` with subtle left/right arrows and swipe drag support.
  - **Quick Buttons**: Collection modal button, live coin counter, status ticker.

### 2.3 Machine Slide & Unlock Flow
- **Horizontal Swipe / Navigation**:
  - Dragging or clicking next/previous arrows initiates machine swap.
- **Unlock Modal**:
  - If target machine is not in `unlockedMachineIds`:
    - Shows dialog: *"Unlock Retro Arcade for 150 🪙? (Your balance: X 🪙)"*.
    - **Unlock Button**: If balance $\ge$ cost, deducts coins, adds to `unlockedMachineIds`, saves, and transitions.
    - **Cancel Button**: Remains on current machine.
- **Active Machine Swap Execution**:
  1. Sets active `currentMachine` in `CollectionManager`.
  2. Updates cabinet materials (pillars, backdrop).
  3. Clears and respawns `PrizeSpawner` with target machine's prize pool.
  4. Resets claw position to home.
  5. Updates Collection UI to render active machine's 9 cards and progress.

---

## 3. Data Persistence (`PlayerCollectionData`)
- `coins`: Current coin balance.
- `discoveredPrizeIds`: Set of collected prize IDs across all machines.
- `unlockedMachineIds`: Machines unlocked for play (defaults to `["toy_box"]`).
- `ownedMachineIds`: Completed machines yielding passive income and free plays.
- `lastPassiveIncomeTimestamp`: Timestamp for offline idle earnings.

---

## 4. Verification & Testing
1. Verify Retro Arcade generator creates 9 definitions, prefabs, and `Machine_RetroArcade.asset`.
2. Verify bottom console bar renders cleanly in portrait layout.
3. Test swipe / arrow navigation from Toy Box to Retro Arcade.
4. Verify unlock popup triggers when locked, prohibits purchase if insufficient coins, and unlocks cleanly when player has 150 coins.
5. Verify cabinet color shifts and prize pile repopulates with Retro prizes upon switching.
6. Verify collection modal reflects active machine's items.
