using UnityEngine;

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
