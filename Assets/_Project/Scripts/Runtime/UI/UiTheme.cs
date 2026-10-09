using TMPro;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>UI colours and font (05 §14): warm parchment panels, dark ink text, gold primary buttons.</summary>
    public static class UiTheme
    {
        public static readonly Color Parchment = new Color32(0xFF, 0xF4, 0xDC, 0xFF);
        public static readonly Color ParchmentDark = new Color32(0xF1, 0xDE, 0xB4, 0xFF);
        public static readonly Color Ink = new Color32(0x3B, 0x2A, 0x1A, 0xFF);
        public static readonly Color InkSoft = new Color32(0x7A, 0x60, 0x45, 0xFF);
        public static readonly Color Gold = new Color32(0xF2, 0xB8, 0x3A, 0xFF);
        public static readonly Color GoldDark = new Color32(0xC8, 0x86, 0x1E, 0xFF);
        public static readonly Color Leaf = new Color32(0x5B, 0xA8, 0x4C, 0xFF);
        public static readonly Color Red = new Color32(0xE2, 0x3B, 0x3B, 0xFF);
        public static readonly Color Purple = new Color32(0x8E, 0x4F, 0xD8, 0xFF);
        public static readonly Color Shade = new Color(0.08f, 0.06f, 0.04f, 0.55f);
        public static readonly Color HudChip = new Color(0.16f, 0.12f, 0.08f, 0.62f);
        public static readonly Color White = Color.white;
        public static readonly Color StarEmpty = new Color32(0xD9, 0xC8, 0xA6, 0xFF);

        public static TMP_FontAsset Font
        {
            get
            {
                AppConfig config = AppConfig.Load();
                if (config != null && config.UiFont != null) return config.UiFont;
                return TMP_Settings.defaultFontAsset;
            }
        }
    }
}
