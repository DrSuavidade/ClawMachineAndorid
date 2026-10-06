# UI Cabinet Theming System Design (Fixed UX + Machine Skinning)

## 1. Overview & Goals

Create a tactile, finalized UI experience centered on the claw machine / gachapon carnival motif.
The design implements a **3-Layer Architecture**:
1. **Fixed UX Skeleton**: Navigation, layout geometry, button hitboxes, modal tab structures, and touch ergonomics remain 100% constant across every machine.
2. **Machine Theme ("CSS Skin")**: Machine-specific palettes (primary, secondary, accent, surface), border trims, bulb glows, button bevels, cabinet materials, and ambient effects.
3. **Global Game Identity**: Unbroken core rules — currency symbols, rarity tier colors (Normal/Rare/Secret), and standard action conventions (Claim/Cancel).

This makes unlocking/switching cabinets feel like stepping in front of an entirely new physical arcade machine without forcing players to re-learn the UI.

---

## 2. Core Architecture

### 2.1 `CabinetTheme` ScriptableObject
Defines visual identity tokens per machine type:

```csharp
namespace ClawMachine.Data
{
    [CreateAssetMenu(fileName = "Theme_", menuName = "ClawMachine/Cabinet Theme")]
    public class CabinetTheme : ScriptableObject
    {
        [Header("Identity")]
        public string themeId = "carnival_vintage";
        public string displayName = "Mid-Century Carnival";

        [Header("Palette")]
        public Color primaryColor = new Color(0.66f, 0.16f, 0.17f);    // Marquee borders / headers
        public Color secondaryColor = new Color(0.12f, 0.17f, 0.24f);  // Inner containers / cards
        public Color accentColor = new Color(0.95f, 0.76f, 0.21f);     // Active tabs, push buttons, rivets
        public Color surfaceColor = new Color(0.10f, 0.11f, 0.14f);    // Modal backdrop panels
        public Color textColorHeader = Color.white;
        public Color textColorBody = new Color(0.78f, 0.82f, 0.88f);

        [Header("Sprites & Sliced Frames")]
        public Sprite frameBorder;        // 9-slice marquee frame
        public Sprite buttonBackground;   // Beveled button plate
        public Sprite headerBanner;       // Ribbon/sign header
        public Sprite rivetBulb;          // Corner rivet / bulb accent

        [Header("Physical Cabinet 3D")]
        public Material cabinetMaterial;
    }
}
```

### 2.2 `MachineDefinition` Association
`MachineDefinition` gains a direct link:
```csharp
[Header("Cabinet Theming")]
public CabinetTheme theme;
```
If `theme == null`, falls back to default fallback theme asset.

### 2.3 `UIThemeManager` Service
Central dispatcher.
- Listens to `ICollectionService.OnMachineChanged`.
- Dispatches event:
  ```csharp
  public static event Action<CabinetTheme> OnThemeChanged;
  ```
- Exposes `CurrentTheme` property.
- Supports `[ExecuteAlways]` in editor mode for instant design inspection.

### 2.4 Declarative `ThemedUIElement`
Attached to Canvas GameObjects to consume theme changes without polluting modal logic:
```csharp
namespace ClawMachine.UI
{
    public enum ThemeTargetType
    {
        Primary,
        Secondary,
        Accent,
        Surface,
        HeaderText,
        BodyText,
        FrameBorder,
        ButtonBackground,
        RivetBulb
    }

    [RequireComponent(typeof(Graphic))]
    public class ThemedUIElement : MonoBehaviour
    {
        public ThemeTargetType targetType;
        // Registers with UIThemeManager, recolors Graphic / Image sprite on OnThemeChanged
    }
}
```

---

## 3. Presets & Tasteful Palette Definitions

### Theme 1: Mid-Century Carnival (Toy Box Starter)
- **Primary**: Vintage Crimson (`#A8282B`)
- **Secondary**: Deep Midnight Navy (`#1E2C3D`)
- **Accent**: Warm Brass / Marquee Bulb Gold (`#F2C335`)
- **Surface**: Dark Enamel (`#16191F`)
- **Tone**: Authentic 1950s boardwalk arcade with warm incandescent bulb vibes.

### Theme 2: Neo-Arcade Gachapon (Retro Arcade)
- **Primary**: Cyber Violet Indigo (`#171B30`)
- **Secondary**: Frosted Acrylic Indigo (`#1C233D`)
- **Accent**: Electric Cyan (`#00F0FF`) & Magenta Pip (`#FF007F`)
- **Surface**: Smoked Transparent Acrylic (`#0D111A`)
- **Tone**: Japanese claw game center, crisp translucent bezels.

### Theme 3: Grand Traveling Circus (Fantasy Dungeon / Secret)
- **Primary**: Royal Burgundy Velvet (`#5E1924`)
- **Secondary**: Deep Forest Emerald (`#143324`)
- **Accent**: Polished Brass / Gold Foil (`#E0B354`)
- **Surface**: Antiqued Walnut Chassis (`#1A1412`)
- **Tone**: Handcrafted turn-of-the-century circus cabinet.

---

## 4. Invariable Fixed UX Layout

Regardless of active theme, the following layouts are invariant:
- **Top Bar**: Fixed anchors (`AnchorMin: (0, 0.915)`, `AnchorMax: (1, 1)`).
  - Left: Collection modal launcher (`[🎁 Collection]`).
  - Center: Machine Switcher Strip (Prev button, machine title, Next button).
  - Right: Coins counter (`🪙 [count]`) + Upgrades + Settings buttons.
- **Center Playfield**: Transparent touch/claw viewing window.
- **Bottom Console**: Fixed anchors (`AnchorMin: (0, 0)`, `AnchorMax: (1, 0.16)`).
  - Left: Virtual joystick (`ArcadeJoystickUI`).
  - Right: Tactile drop push button (`ArcadePushButtonUI`).
- **Modals**:
  - `ProgressModalUI`: 2 tabs (Upgrades | Rewards), 7-day streak cards (top), 3 quest rows (mid), 6 milestone cards (bot).
  - `CollectionModalUI`: 3x3 prize card grid, count badges, duplicate sell bar.
  - `PrizeWinModalUI`: Off-screen 3D prize preview, rarity badge, claim CTA.

---

## 5. Global Identity Safeguards

Items that NEVER change color or styling across themes:
- **Rarity Badges**:
  - Normal: Slate Blue (`#4A90E2`)
  - Rare: Amethyst Purple (`#9013FE`)
  - Secret: Radiant Gold (`#F5A623`)
- **Currency**: Arcade Coin token (`🪙`) and clear numerical typography.
- **Action Intent**: Confirm/Claim is consistently Green/Gold; Close/Cancel is consistently Neutral/Red.

---

## 6. Verification & Testing

1. **Theme Switching**: Cycle through all 6 machines in carousel; verify all HUD frames, modal backdrops, and button bevels update smoothly with zero frame drops.
2. **Missing Theme Fallback**: Instantiate a machine with `theme = null`; verify it gracefully uses the fallback theme.
3. **Data Integrity**: Ensure modal inputs, click triggers, joystick drags, and drop operations function identically under all themes.
