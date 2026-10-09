using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Graybox colour language (mvp.md §4, 03 §4.1, D-051): materials keep identity colours at reduced saturation;
    /// red = required, purple = protected, gold/green = helpful interactive.
    /// </summary>
    public static class MaterialPalette
    {
        public static readonly Color Straw = new Color32(0xE8, 0xD2, 0x7A, 0xFF);
        public static readonly Color Timber = new Color32(0x9A, 0x6B, 0x3F, 0xFF);
        public static readonly Color Stone = new Color32(0x7D, 0x8A, 0xA0, 0xFF);
        public static readonly Color Ice = new Color32(0x7F, 0xE3, 0xF2, 0xFF);
        public static readonly Color Metal = new Color32(0x3A, 0x3F, 0x47, 0xFF);
        public static readonly Color Earth = new Color32(0x6B, 0x8E, 0x4E, 0xFF);

        public static readonly Color Required = new Color32(0xE2, 0x3B, 0x3B, 0xFF);
        public static readonly Color Protected = new Color32(0x8E, 0x4F, 0xD8, 0xFF);
        public static readonly Color Helpful = new Color32(0xF2, 0xB8, 0x3A, 0xFF);
        public static readonly Color Water = new Color32(0x4F, 0xA8, 0xE0, 0xFF);

        public static Color For(MaterialKind kind)
        {
            switch (kind)
            {
                case MaterialKind.Straw: return Straw;
                case MaterialKind.Timber: return Timber;
                case MaterialKind.Stone: return Stone;
                case MaterialKind.Ice: return Ice;
                case MaterialKind.Metal: return Metal;
                default: return Earth;
            }
        }
    }
}
