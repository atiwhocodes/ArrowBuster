using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Small textures and sprites generated at runtime (original, no imported art): particle shapes and UI
    /// primitives (rounded panels, circles, stars, icons). Cached for the session.
    /// </summary>
    public static class ProceduralTextures
    {
        private static Texture2D _softCircle;
        private static Texture2D _roundedSquare;
        private static Sprite _panel;
        private static Sprite _pill;
        private static Sprite _circle;
        private static Sprite _ring;
        private static Sprite _star;
        private static Sprite _crest;
        private static Sprite _arrowIcon;
        private static Sprite _vase;
        private static Sprite _pause;
        private static Sprite _restart;
        private static Sprite _hand;

        public static Texture2D SoftCircle => _softCircle ? _softCircle : (_softCircle = Make(64, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f)) * 2f;
            return new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d));
        }));

        public static Texture2D RoundedSquare => _roundedSquare ? _roundedSquare : (_roundedSquare = Make(32, (x, y) =>
            new Color(1f, 1f, 1f, RoundedRectAlpha(x, y, 0.5f, 0.5f, 0.3f, 32))));

        /// <summary>9-sliced rounded panel (radius 24 px of a 64 px texture).</summary>
        public static Sprite Panel => _panel ? _panel : (_panel = NineSlice(64, 24));

        /// <summary>Fully rounded pill/button (9-sliced).</summary>
        public static Sprite Pill => _pill ? _pill : (_pill = NineSlice(64, 31));

        public static Sprite Circle => _circle ? _circle : (_circle = ToSprite(Make(128, (x, y) =>
            new Color(1f, 1f, 1f, Edge(1f - Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f)) * 2f, 128)))));

        public static Sprite Ring => _ring ? _ring : (_ring = ToSprite(Make(128, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f)) * 2f;
            return new Color(1f, 1f, 1f, Edge(1f - d, 128) * Edge(d - 0.72f, 128));
        })));

        public static Sprite Star => _star ? _star : (_star = ToSprite(Make(128, (x, y) =>
            new Color(1f, 1f, 1f, Edge(StarField(x - 0.5f, y - 0.5f), 128)))));

        /// <summary>Crest target icon: concentric rings (required objective).</summary>
        public static Sprite Crest => _crest ? _crest : (_crest = ToSprite(Make(128, (x, y) =>
        {
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(0.5f, 0.5f)) * 2f;
            float outer = Edge(1f - d, 128);
            bool band = (d > 0.55f && d < 0.75f) || d < 0.25f;
            float shade = band ? 1f : 0.55f;
            return new Color(shade, shade, shade, outer);
        })));

        /// <summary>Arrow icon pointing up (quiver HUD).</summary>
        public static Sprite ArrowIcon => _arrowIcon ? _arrowIcon : (_arrowIcon = ToSprite(Make(64, (x, y) =>
        {
            float shaft = Mathf.Abs(x - 0.5f) < 0.06f && y > 0.12f && y < 0.78f ? 1f : 0f;
            float head = y >= 0.66f && Mathf.Abs(x - 0.5f) < (0.98f - y) * 0.75f ? 1f : 0f;
            float fletch = y < 0.3f && Mathf.Abs(x - 0.5f) < 0.06f + (0.3f - y) * 0.6f && Mathf.Abs(x - 0.5f) > 0.04f ? 0.8f : 0f;
            return new Color(1f, 1f, 1f, Mathf.Max(shaft, head, fletch));
        })));

        /// <summary>Vase icon (protected objective).</summary>
        public static Sprite Vase => _vase ? _vase : (_vase = ToSprite(Make(64, (x, y) =>
        {
            float w = 0.12f + 0.26f * Mathf.Sin(Mathf.Clamp01((y - 0.08f) / 0.7f) * Mathf.PI) * (y < 0.78f ? 1f : 0f);
            if (y >= 0.78f && y < 0.92f) w = 0.16f;
            float body = y > 0.08f && y < 0.92f && Mathf.Abs(x - 0.5f) < w ? 1f : 0f;
            return new Color(1f, 1f, 1f, body);
        })));

        public static Sprite PauseIcon => _pause ? _pause : (_pause = ToSprite(Make(64, (x, y) =>
        {
            bool bar = y > 0.22f && y < 0.78f && ((x > 0.28f && x < 0.42f) || (x > 0.58f && x < 0.72f));
            return new Color(1f, 1f, 1f, bar ? 1f : 0f);
        })));

        public static Sprite RestartIcon => _restart ? _restart : (_restart = ToSprite(Make(64, (x, y) =>
        {
            Vector2 p = new Vector2(x - 0.5f, y - 0.5f);
            float d = p.magnitude;
            float angle = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg;
            bool arc = d > 0.2f && d < 0.31f && !(angle > 50f && angle < 110f);
            Vector2 tip = p - new Vector2(0.06f, 0.24f);
            bool head = tip.x > -0.12f && tip.y > -0.1f && tip.x + tip.y < 0.06f && tip.y < 0.1f && tip.x < 0.1f;
            return new Color(1f, 1f, 1f, arc || head ? 1f : 0f);
        })));

        /// <summary>Simple pointing-hand glyph for the onboarding ghost hand.</summary>
        public static Sprite Hand => _hand ? _hand : (_hand = ToSprite(Make(128, (x, y) =>
        {
            float palm = RoundedRectAlpha(x, y, 0.5f, 0.32f, 0.22f, 128, 0.2f);
            float finger = RoundedRectAlpha(x, y, 0.5f, 0.66f, 0.07f, 128, 0.22f);
            return new Color(1f, 1f, 1f, Mathf.Max(palm, finger));
        })));

        private static float StarField(float x, float y)
        {
            float angle = Mathf.Atan2(y, x) + Mathf.PI / 2f;
            float r = Mathf.Sqrt(x * x + y * y) * 2f;
            float k = Mathf.Cos(5f * angle) * 0.5f + 0.5f;
            float radius = Mathf.Lerp(0.5f, 0.98f, Mathf.Pow(k, 2.2f));
            return radius - r;
        }

        private static float RoundedRectAlpha(float x, float y, float cx, float cy, float half, int size, float halfY = -1f)
        {
            if (halfY < 0f) halfY = half;
            float radius = Mathf.Min(half, halfY) * 0.6f;
            float dx = Mathf.Max(Mathf.Abs(x - cx) - (half - radius), 0f);
            float dy = Mathf.Max(Mathf.Abs(y - cy) - (halfY - radius), 0f);
            return Edge(radius - Mathf.Sqrt(dx * dx + dy * dy), size);
        }

        /// <summary>Anti-aliased edge: signed distance (texture-relative) to alpha.</summary>
        private static float Edge(float signedDistance, int size) => Mathf.Clamp01(signedDistance * size * 0.5f + 0.5f);

        private static Texture2D Make(int size, System.Func<float, float, Color> pixel)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.DontSave
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    pixels[y * size + x] = pixel((x + 0.5f) / size, (y + 0.5f) / size);
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Sprite ToSprite(Texture2D texture)
        {
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        private static Sprite NineSlice(int size, int radius)
        {
            Texture2D texture = Make(size, (x, y) =>
            {
                float px = x * size, py = y * size;
                float cx = Mathf.Clamp(px, radius, size - radius);
                float cy = Mathf.Clamp(py, radius, size - radius);
                float d = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                return new Color(1f, 1f, 1f, Mathf.Clamp01(radius - d + 0.5f));
            });
            var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _softCircle = null; _roundedSquare = null; _panel = null; _pill = null; _circle = null; _ring = null;
            _star = null; _crest = null; _arrowIcon = null; _vase = null; _pause = null; _restart = null; _hand = null;
        }
    }
}
