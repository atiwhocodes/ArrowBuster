using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Original procedural art kit for Greenwood Range (05 §1–3): a saturated-but-controlled palette as URP
    /// materials and low-poly meshes saved as assets. Everything is generated here — no imported or AI art (D-089).
    /// </summary>
    public static class ArtKit
    {
        public const string MaterialFolder = "Assets/_Project/Art/Materials";
        public const string MeshFolder = "Assets/_Project/Art/Models/Generated";
        public const string TextureFolder = "Assets/_Project/Art/Textures";

        // Greenwood palette (D-051: materials keep identity colours, accents reserved for gameplay meaning).
        public static readonly Color Timber = Hex("9A6B3F");
        public static readonly Color TimberDark = Hex("6E4A2A");
        public static readonly Color TimberLight = Hex("C08A55");
        public static readonly Color Straw = Hex("E8D27A");
        public static readonly Color Stone = Hex("8C97AB");
        public static readonly Color StoneDark = Hex("6B7588");
        public static readonly Color CrestRed = Hex("E23B3B");
        public static readonly Color CrestCream = Hex("FFF1DA");
        public static readonly Color Gold = Hex("F2B83A");
        public static readonly Color VasePurple = Hex("8E4FD8");
        public static readonly Color VaseGold = Hex("F5C95A");
        public static readonly Color Grass = Hex("7CB342");
        public static readonly Color GrassDark = Hex("5E8F32");
        public static readonly Color Earth = Hex("8D6E4C");
        public static readonly Color EarthDark = Hex("6B5038");
        public static readonly Color Leaf = Hex("4E9A47");
        public static readonly Color LeafLight = Hex("79BD5E");
        public static readonly Color Bark = Hex("7A5534");
        public static readonly Color HillFar = Hex("A9CF97");
        public static readonly Color HillNear = Hex("8CC06E");
        public static readonly Color Ruin = Hex("BDB6A8");
        public static readonly Color Water = new Color(0.31f, 0.66f, 0.88f, 0.78f);
        public static readonly Color Rope = Hex("D9B98A");
        public static readonly Color BowWood = Hex("8A5A2B");
        public static readonly Color ArrowShaft = Hex("EAD7AA");
        public static readonly Color ArrowHead = Hex("A7B0BA");
        public static readonly Color Fletch = Hex("E86B3A");
        public static readonly Color Cloud = Hex("FFFFFF");
        public static readonly Color Awning = Hex("E9A23B");
        public static readonly Color SkyTop = Hex("4FA3E8");
        public static readonly Color SkyBottom = Hex("E4F4FF");

        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
        private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }

        // ------------------------------------------------------------------ materials

        public static Material Lit(string name, Color color, float smoothness = 0.12f, bool transparent = false, Color? emission = null)
        {
            string key = "M_" + name;
            if (Materials.TryGetValue(key, out Material cached) && cached != null) return cached;
            string path = $"{MaterialFolder}/{key}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", 0f);
            material.enableInstancing = true;
            if (transparent) MakeTransparent(material);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }
            EditorUtility.SetDirty(material);
            Materials[key] = material;
            return material;
        }

        public static Material Unlit(string name, Color color, bool transparent, Texture texture = null)
        {
            string key = "M_" + name;
            if (Materials.TryGetValue(key, out Material cached) && cached != null) return cached;
            string path = $"{MaterialFolder}/{key}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            if (texture != null) material.SetTexture("_BaseMap", texture);
            if (transparent) MakeTransparent(material);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            Materials[key] = material;
            return material;
        }

        public static Material Particles()
        {
            const string key = "M_Particles";
            string path = $"{MaterialFolder}/{key}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(material, path);
            }
            MakeTransparent(material);
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void MakeTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        /// <summary>Vertical gradient texture saved as an asset (sky).</summary>
        public static Texture2D Gradient(string name, Color bottom, Color top)
        {
            string path = $"{TextureFolder}/T_{name}.asset";
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            bool created = texture == null;
            if (created) texture = new Texture2D(4, 128, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 128; y++)
            {
                Color c = Color.Lerp(bottom, top, Mathf.SmoothStep(0f, 1f, y / 127f));
                for (int x = 0; x < 4; x++) texture.SetPixel(x, y, c);
            }
            texture.Apply();
            if (created) AssetDatabase.CreateAsset(texture, path);
            else EditorUtility.SetDirty(texture);
            return texture;
        }

        // ------------------------------------------------------------------ meshes

        public static Mesh Save(Mesh mesh)
        {
            string key = mesh.name;
            mesh.name = "SM_" + key;
            string path = $"{MeshFolder}/SM_{key}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                existing.Clear();
                EditorUtility.CopySerialized(mesh, existing);
                Meshes[key] = existing;
                return existing;
            }
            AssetDatabase.CreateAsset(mesh, path);
            Meshes[key] = mesh;
            return mesh;
        }

        public static Mesh Get(string name) => Meshes.TryGetValue(name, out Mesh m) ? m : AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshFolder}/SM_{name}.asset");

        /// <summary>Wooden crate: chamfered body (sub 0) + darker edge frame and diagonal brace (sub 1).</summary>
        public static Mesh Crate(string name, Vector2 size)
        {
            var b = new MeshBuilder(2);
            b.Sub(0).BeveledBox(Vector3.zero, new Vector3(size.x, size.y, 0.96f), 0.06f);
            b.Sub(1);
            float hx = size.x * 0.5f, hy = size.y * 0.5f, z = -0.49f, t = 0.11f;
            Vector3 inside = new Vector3(0f, 0f, 1f);
            // Frame on the camera-facing side.
            b.BeveledBox(new Vector3(0f, hy - t * 0.5f, z), new Vector3(size.x - 0.02f, t, 0.06f), 0.02f);
            b.BeveledBox(new Vector3(0f, -hy + t * 0.5f, z), new Vector3(size.x - 0.02f, t, 0.06f), 0.02f);
            b.BeveledBox(new Vector3(hx - t * 0.5f, 0f, z), new Vector3(t, size.y - 0.02f, 0.06f), 0.02f);
            b.BeveledBox(new Vector3(-hx + t * 0.5f, 0f, z), new Vector3(t, size.y - 0.02f, 0.06f), 0.02f);
            // Diagonal brace.
            Vector2 a = new Vector2(-hx + t, -hy + t), c = new Vector2(hx - t, hy - t);
            Vector2 n = new Vector2(-(c - a).y, (c - a).x).normalized * (t * 0.45f);
            b.Quad(new Vector3(a.x + n.x, a.y + n.y, z - 0.035f), new Vector3(c.x + n.x, c.y + n.y, z - 0.035f),
                new Vector3(c.x - n.x, c.y - n.y, z - 0.035f), new Vector3(a.x - n.x, a.y - n.y, z - 0.035f), inside);
            return Save(b.Build(name));
        }

        public static Mesh Box(string name, Vector3 size, float bevel)
        {
            var b = new MeshBuilder();
            b.BeveledBox(Vector3.zero, size, bevel);
            return Save(b.Build(name));
        }

        /// <summary>Grass-topped earth block: earth body (sub 0) + grass cap (sub 1). Pivot at top-centre.</summary>
        public static Mesh GrassBlock(string name, Vector3 size)
        {
            var b = new MeshBuilder(2);
            b.Sub(0).BeveledBox(new Vector3(0f, -size.y * 0.5f - 0.06f, 0f), new Vector3(size.x - 0.04f, size.y - 0.12f, size.z - 0.04f), 0.08f);
            b.Sub(1).BeveledBox(new Vector3(0f, -0.1f, 0f), new Vector3(size.x, 0.22f, size.z), 0.06f);
            return Save(b.Build(name));
        }

        /// <summary>Crest target: cream/red rings facing the camera on a timber stand (subs: 0 red, 1 cream, 2 wood).</summary>
        public static Mesh Crest(string name)
        {
            var b = new MeshBuilder(3);
            float z = -0.1f;
            b.Sub(0).Cylinder(new Vector3(0f, 0.08f, 0.02f), 0.46f, 0.46f, 0.2f, 20, axis: 2);
            b.Sub(1).Annulus(new Vector3(0f, 0.08f, z - 0.005f), 0.3f, 0.4f, 20);
            b.Sub(0).Annulus(new Vector3(0f, 0.08f, z - 0.01f), 0.14f, 0.3f, 20);
            b.Sub(1).Annulus(new Vector3(0f, 0.08f, z - 0.015f), 0f, 0.14f, 20);
            b.Sub(2).BeveledBox(new Vector3(0f, -0.37f, 0.02f), new Vector3(0.62f, 0.12f, 0.5f), 0.03f);
            b.Sub(2).BeveledBox(new Vector3(0f, -0.22f, 0.1f), new Vector3(0.1f, 0.3f, 0.1f), 0.02f);
            return Save(b.Build(name));
        }

        /// <summary>Royal vase (lathe): purple body (sub 0) with gold rim and foot (sub 1). Pivot at centre.</summary>
        public static Mesh Vase(string name)
        {
            var b = new MeshBuilder(2);
            var body = new List<Vector2>
            {
                new Vector2(0f, -0.45f), new Vector2(0.17f, -0.45f), new Vector2(0.24f, -0.3f), new Vector2(0.32f, -0.1f),
                new Vector2(0.3f, 0.1f), new Vector2(0.2f, 0.25f), new Vector2(0.12f, 0.33f)
            };
            b.Sub(0).Lathe(Vector3.zero, body, 12);
            var rim = new List<Vector2> { new Vector2(0.12f, 0.33f), new Vector2(0.18f, 0.4f), new Vector2(0.19f, 0.45f), new Vector2(0f, 0.45f) };
            b.Sub(1).Lathe(Vector3.zero, rim, 12);
            var foot = new List<Vector2> { new Vector2(0f, -0.47f), new Vector2(0.2f, -0.47f), new Vector2(0.19f, -0.43f), new Vector2(0f, -0.43f) };
            b.Sub(1).Lathe(Vector3.zero, foot, 12);
            b.Sub(1).Cylinder(new Vector3(0f, 0f, 0f), 0.325f, 0.325f, 0.06f, 12, axis: 1, caps: false);
            return Save(b.Build(name));
        }

        /// <summary>Arrow with the TIP at the origin pointing +Z (subs: 0 shaft, 1 head, 2 fletching).</summary>
        public static Mesh Arrow(string name, float length = 0.95f)
        {
            var b = new MeshBuilder(3);
            b.Sub(0).Cylinder(new Vector3(0f, 0f, -length * 0.5f - 0.05f), 0.028f, 0.028f, length - 0.1f, 6, axis: 2);
            b.Sub(1).Cylinder(new Vector3(0f, 0f, -0.07f), 0.07f, 0f, 0.14f, 6, axis: 2);
            b.Sub(2);
            float zb = -length + 0.02f, zf = -length + 0.24f;
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2f / 3f + Mathf.PI / 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                Vector3 p0 = new Vector3(0f, 0f, zb) + dir * 0.02f, p1 = new Vector3(0f, 0f, zf) + dir * 0.02f;
                Vector3 p2 = new Vector3(0f, 0f, zf - 0.08f) + dir * 0.1f, p3 = new Vector3(0f, 0f, zb) + dir * 0.1f;
                b.Quad(p0, p1, p2, p3, p0 + Vector3.Cross(dir, Vector3.forward) * 0.1f);
                b.Quad(p0, p1, p2, p3, p0 - Vector3.Cross(dir, Vector3.forward) * 0.1f);
            }
            return Save(b.Build(name));
        }

        /// <summary>Recurve bow in its local XY plane; the arrow points +Y. Subs: 0 wood, 1 gold inlays, 2 grip wrap.</summary>
        public static Mesh Bow(string name)
        {
            var b = new MeshBuilder(3);
            const int segments = 14;
            Vector3 Center(float t) => new Vector3(0.78f * t, 0.2f * (1f - t * t) - 0.1f + 0.06f * Mathf.Pow(Mathf.Abs(t), 6f), 0f);
            float Width(float t) => Mathf.Lerp(0.11f, 0.045f, Mathf.Abs(t));
            const float depth = 0.09f;
            for (int i = 0; i < segments; i++)
            {
                float t0 = -1f + 2f * i / segments, t1 = -1f + 2f * (i + 1) / segments;
                Vector3 c0 = Center(t0), c1 = Center(t1);
                Vector3 tangent = (c1 - c0).normalized;
                Vector3 normal = new Vector3(-tangent.y, tangent.x, 0f);
                float w0 = Width(t0) * 0.5f, w1 = Width(t1) * 0.5f;
                Vector3 inside = (c0 + c1) * 0.5f;
                Vector3[] r0 = { c0 + normal * w0 + Vector3.back * depth * 0.5f, c0 - normal * w0 + Vector3.back * depth * 0.5f, c0 - normal * w0 + Vector3.forward * depth * 0.5f, c0 + normal * w0 + Vector3.forward * depth * 0.5f };
                Vector3[] r1 = { c1 + normal * w1 + Vector3.back * depth * 0.5f, c1 - normal * w1 + Vector3.back * depth * 0.5f, c1 - normal * w1 + Vector3.forward * depth * 0.5f, c1 + normal * w1 + Vector3.forward * depth * 0.5f };
                bool gripZone = Mathf.Abs(t0) < 0.16f;
                b.Sub(gripZone ? 2 : 0);
                for (int k = 0; k < 4; k++) b.Quad(r0[k], r0[(k + 1) % 4], r1[(k + 1) % 4], r1[k], inside);
                if (i == 0) b.Polygon(r0, inside + tangent * 0.1f);
                if (i == segments - 1) b.Polygon(r1, inside - tangent * 0.1f);
                if (!gripZone && i % 3 == 1)
                {
                    b.Sub(1).BeveledBox((c0 + c1) * 0.5f + Vector3.back * depth * 0.52f, new Vector3(0.05f, 0.05f, 0.02f), 0.008f);
                }
            }
            b.Sub(1).Cylinder(Center(-1f), 0.03f, 0.03f, 0.1f, 6, axis: 2);
            b.Sub(1).Cylinder(Center(1f), 0.03f, 0.03f, 0.1f, 6, axis: 2);
            return Save(b.Build(name));
        }

        public static Mesh Tree(string name, float height, int seedShape)
        {
            var b = new MeshBuilder(2);
            b.Sub(0).Cylinder(new Vector3(0f, height * 0.2f, 0f), 0.22f, 0.16f, height * 0.4f, 7, axis: 1);
            b.Sub(1);
            if (seedShape % 2 == 0)
            {
                b.Cylinder(new Vector3(0f, height * 0.55f, 0f), 1.3f, 0f, height * 0.55f, 8, axis: 1);
                b.Cylinder(new Vector3(0f, height * 0.78f, 0f), 1.0f, 0f, height * 0.45f, 8, axis: 1, angleOffset: 0.4f);
            }
            else
            {
                b.LowSphere(new Vector3(0f, height * 0.62f, 0f), new Vector3(1.25f, 1.05f, 1.25f), 1);
                b.LowSphere(new Vector3(0.55f, height * 0.5f, 0.3f), new Vector3(0.8f, 0.7f, 0.8f), 1);
            }
            return Save(b.Build(name));
        }

        public static Mesh Mound(string name, Vector3 radii)
        {
            var b = new MeshBuilder();
            var profile = new List<Vector2>();
            for (int i = 0; i <= 5; i++)
            {
                float a = i / 5f * Mathf.PI * 0.5f;
                profile.Add(new Vector2(Mathf.Cos(a) * radii.x, Mathf.Sin(a) * radii.y));
            }
            b.Lathe(Vector3.zero, profile, 10);
            Mesh mesh = b.Build(name);
            Vector3[] v = mesh.vertices;
            for (int i = 0; i < v.Length; i++) v[i].z *= radii.z / Mathf.Max(0.01f, radii.x);
            mesh.vertices = v;
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        public static Mesh Blob(string name, Vector3 radii)
        {
            var b = new MeshBuilder();
            b.LowSphere(Vector3.zero, radii, 1);
            return Save(b.Build(name));
        }

        public static Mesh Cylinder(string name, float radius, float height, int segments, int axis = 1)
        {
            var b = new MeshBuilder();
            b.Cylinder(Vector3.zero, radius, radius, height, segments, axis);
            return Save(b.Build(name));
        }

        public static Mesh Quad(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, -0.5f, 0f) };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return Save(mesh);
        }

        public static Mesh Ring(string name)
        {
            var b = new MeshBuilder();
            b.Annulus(Vector3.zero, 0.36f, 0.5f, 24);
            return Save(b.Build(name));
        }

        /// <summary>Octahedron "gem" used as the floating protected marker.</summary>
        public static Mesh Gem(string name)
        {
            var b = new MeshBuilder();
            b.LowSphere(Vector3.zero, new Vector3(0.14f, 0.2f, 0.14f), 0);
            return Save(b.Build(name));
        }

        public static void ClearCache()
        {
            Materials.Clear();
            Meshes.Clear();
        }
    }
}
