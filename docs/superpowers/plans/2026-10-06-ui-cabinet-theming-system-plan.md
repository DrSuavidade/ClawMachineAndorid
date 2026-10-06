# UI Cabinet Theming System Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a responsive 3-layer UI theming architecture (Fixed UX skeleton + Dynamic CabinetTheme + Global identity) allowing machines to dynamically transform their visual flavor while keeping layout and interaction identical.

**Architecture:** Create `CabinetTheme` ScriptableObject data model, link to `MachineDefinition`, build `UIThemeManager` dispatcher service and declarative `ThemedUIElement` component. Create carnival, gachapon, and circus preset theme assets, and attach them to machines.

**Tech Stack:** Unity 6 C# (uGUI, ScriptableObject, Event dispatching).

---

### File Structure Map
- **Create**: `Assets/Scripts/Data/CabinetTheme.cs` — ScriptableObject definition holding palette and sprite references.
- **Modify**: `Assets/Scripts/Data/MachineDefinition.cs` — Add `public CabinetTheme theme;` reference.
- **Create**: `Assets/Scripts/UI/Theming/UIThemeManager.cs` — Runtime singleton/service broadcasting theme changes.
- **Create**: `Assets/Scripts/UI/Theming/ThemedUIElement.cs` — uGUI element subscriber applying colors/sprites on change.
- **Create Assets**:
  - `Assets/Data/Themes/Theme_Carnival_ToyBox.asset`
  - `Assets/Data/Themes/Theme_Gachapon_RetroArcade.asset`
  - `Assets/Data/Themes/Theme_Circus_FantasyDungeon.asset`
  - `Assets/Data/Themes/Theme_Cosmic_Galaxy.asset`
  - `Assets/Data/Themes/Theme_Abyss_Ocean.asset`
  - `Assets/Data/Themes/Theme_Candy_Shop.asset`

---

## Tasks

### Task 1: Create `CabinetTheme` ScriptableObject & Update `MachineDefinition`
- [ ] Create `Assets/Scripts/Data/CabinetTheme.cs` with palette and sprite fields.
- [ ] Add `public CabinetTheme theme;` to `Assets/Scripts/Data/MachineDefinition.cs`.
- [ ] Verify clean compilation.
- [ ] Commit changes.

### Task 2: Build `UIThemeManager` and `ThemedUIElement`
- [ ] Create `Assets/Scripts/UI/Theming/UIThemeManager.cs` listening to `ICollectionService.OnMachineChanged` and broadcasting `OnThemeChanged`.
- [ ] Create `Assets/Scripts/UI/Theming/ThemedUIElement.cs` applying `primary`, `secondary`, `accent`, `surface`, `textColorHeader`, and `textColorBody` to uGUI `Graphic`/`Image`/`Text` components.
- [ ] Verify clean compilation.
- [ ] Commit changes.

### Task 3: Create Theme ScriptableObject Assets & Assign to Machines
- [ ] Create theme assets under `Assets/Data/Themes/` with tuned palettes:
  - Mid-Century Carnival (Toy Box)
  - Neo-Arcade Gachapon (Retro Arcade)
  - Traveling Circus (Fantasy Dungeon)
  - Cosmic Nebula (Cosmic Galaxy)
  - Deep Abyss (Ocean Abyss)
  - Sweet Pastry (Sweet Candy)
- [ ] Assign respective `theme` references to each `Machine_*.asset`.
- [ ] Commit changes.

### Task 4: Hook Theming to UI Prefabs and Scene Hierarchy
- [ ] Attach `ThemedUIElement` to key HUD components (`SafeAreaContainer` header, bottom console frame, button highlights, modal frames).
- [ ] Verify `UIThemeManager` initializes on start and updates when scrolling machine carousel.
- [ ] Verify zero runtime errors.
- [ ] Commit changes.
