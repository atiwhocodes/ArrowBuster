using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Small flat-shaded mesh builder for the procedural "toy diorama" art kit (05 §1: bevel, don't detail).
    /// Supports submeshes so one mesh can carry a body and trim material.
    /// </summary>
    public sealed class MeshBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector3> _normals = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<List<int>> _submeshes = new List<List<int>>();
        private int _current;

        public MeshBuilder(int submeshCount = 1)
        {
            for (int i = 0; i < submeshCount; i++) _submeshes.Add(new List<int>());
        }

        public MeshBuilder Sub(int index)
        {
            while (_submeshes.Count <= index) _submeshes.Add(new List<int>());
            _current = index;
            return this;
        }

        /// <summary>Adds a flat polygon (convex, ordered); flips winding so the normal faces away from <paramref name="inside"/>.</summary>
        public void Polygon(IList<Vector3> points, Vector3 inside)
        {
            if (points.Count < 3) return;
            Vector3 centroid = Vector3.zero;
            foreach (Vector3 p in points) centroid += p;
            centroid /= points.Count;
            Vector3 normal = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;
            bool flip = Vector3.Dot(normal, centroid - inside) < 0f;
            if (flip) normal = -normal;

            int start = _vertices.Count;
            for (int i = 0; i < points.Count; i++)
            {
                _vertices.Add(points[i]);
                _normals.Add(normal);
                _uvs.Add(PlanarUv(points[i], normal));
            }
            List<int> tris = _submeshes[_current];
            for (int i = 1; i < points.Count - 1; i++)
            {
                if (flip)
                {
                    tris.Add(start); tris.Add(start + i + 1); tris.Add(start + i);
                }
                else
                {
                    tris.Add(start); tris.Add(start + i); tris.Add(start + i + 1);
                }
            }
        }

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 inside) => Polygon(new[] { a, b, c, d }, inside);

        public void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 inside) => Polygon(new[] { a, b, c }, inside);

        /// <summary>Chamfered box (flat-shaded bevels) centred at <paramref name="center"/>.</summary>
        public void BeveledBox(Vector3 center, Vector3 size, float bevel)
        {
            Vector3 h = size * 0.5f;
            bevel = Mathf.Min(bevel, Mathf.Min(h.x, Mathf.Min(h.y, h.z)) * 0.9f);
            Vector3 i = h - Vector3.one * bevel;
            Vector3 c = center;
            for (int sx = -1; sx <= 1; sx += 2)
            {
                Quad(c + new Vector3(sx * h.x, -i.y, -i.z), c + new Vector3(sx * h.x, i.y, -i.z), c + new Vector3(sx * h.x, i.y, i.z), c + new Vector3(sx * h.x, -i.y, i.z), c);
                Quad(c + new Vector3(-i.x, sx * h.y, -i.z), c + new Vector3(i.x, sx * h.y, -i.z), c + new Vector3(i.x, sx * h.y, i.z), c + new Vector3(-i.x, sx * h.y, i.z), c);
                Quad(c + new Vector3(-i.x, -i.y, sx * h.z), c + new Vector3(i.x, -i.y, sx * h.z), c + new Vector3(i.x, i.y, sx * h.z), c + new Vector3(-i.x, i.y, sx * h.z), c);
            }
            if (bevel <= 0.0001f) return;
            for (int sa = -1; sa <= 1; sa += 2)
                for (int sb = -1; sb <= 1; sb += 2)
                {
                    // Edges along Z (between X and Y faces), along X (Y/Z faces), along Y (X/Z faces).
                    Quad(c + new Vector3(sa * h.x, sb * i.y, -i.z), c + new Vector3(sa * i.x, sb * h.y, -i.z), c + new Vector3(sa * i.x, sb * h.y, i.z), c + new Vector3(sa * h.x, sb * i.y, i.z), c);
                    Quad(c + new Vector3(-i.x, sa * h.y, sb * i.z), c + new Vector3(-i.x, sa * i.y, sb * h.z), c + new Vector3(i.x, sa * i.y, sb * h.z), c + new Vector3(i.x, sa * h.y, sb * i.z), c);
                    Quad(c + new Vector3(sa * h.x, -i.y, sb * i.z), c + new Vector3(sa * i.x, -i.y, sb * h.z), c + new Vector3(sa * i.x, i.y, sb * h.z), c + new Vector3(sa * h.x, i.y, sb * i.z), c);
                }
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                    for (int sz = -1; sz <= 1; sz += 2)
                        Triangle(c + new Vector3(sx * h.x, sy * i.y, sz * i.z), c + new Vector3(sx * i.x, sy * h.y, sz * i.z), c + new Vector3(sx * i.x, sy * i.y, sz * h.z), c);
        }

        /// <summary>Prism around an axis (cylinder / cone / disc). Axis 0 = X, 1 = Y, 2 = Z.</summary>
        public void Cylinder(Vector3 center, float radiusBottom, float radiusTop, float height, int segments, int axis, bool caps = true, float angleOffset = 0f)
        {
            Vector3 up = axis == 0 ? Vector3.right : axis == 1 ? Vector3.up : Vector3.forward;
            Vector3 u = axis == 1 ? Vector3.right : Vector3.up;
            Vector3 v = Vector3.Cross(up, u);
            Vector3 bottom = center - up * height * 0.5f;
            Vector3 top = center + up * height * 0.5f;
            var ringBottom = new Vector3[segments];
            var ringTop = new Vector3[segments];
            for (int s = 0; s < segments; s++)
            {
                float a = angleOffset + s * Mathf.PI * 2f / segments;
                Vector3 dir = u * Mathf.Cos(a) + v * Mathf.Sin(a);
                ringBottom[s] = bottom + dir * radiusBottom;
                ringTop[s] = top + dir * radiusTop;
            }
            for (int s = 0; s < segments; s++)
            {
                int n = (s + 1) % segments;
                if (radiusTop <= 0.0001f) Triangle(ringBottom[s], ringBottom[n], top, center);
                else Quad(ringBottom[s], ringBottom[n], ringTop[n], ringTop[s], center);
            }
            if (!caps) return;
            if (radiusBottom > 0.0001f) Polygon(ringBottom, center + up * height);
            if (radiusTop > 0.0001f) Polygon(ringTop, center - up * height);
        }

        /// <summary>Flat annulus facing -Z (toward the camera) at depth z.</summary>
        public void Annulus(Vector3 center, float inner, float outer, int segments)
        {
            for (int s = 0; s < segments; s++)
            {
                float a0 = s * Mathf.PI * 2f / segments, a1 = (s + 1) * Mathf.PI * 2f / segments;
                Vector3 d0 = new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f), d1 = new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f);
                Vector3 behind = center + Vector3.forward;
                if (inner <= 0.0001f) Triangle(center, center + d0 * outer, center + d1 * outer, behind);
                else Quad(center + d0 * inner, center + d0 * outer, center + d1 * outer, center + d1 * inner, behind);
            }
        }

        /// <summary>Surface of revolution around Y from a (radius, height) profile, flat-shaded.</summary>
        public void Lathe(Vector3 center, IList<Vector2> profile, int segments)
        {
            for (int p = 0; p < profile.Count - 1; p++)
            {
                for (int s = 0; s < segments; s++)
                {
                    float a0 = s * Mathf.PI * 2f / segments, a1 = (s + 1) * Mathf.PI * 2f / segments;
                    Vector3 Point(Vector2 rp, float ang) => center + new Vector3(Mathf.Cos(ang) * rp.x, rp.y, Mathf.Sin(ang) * rp.x);
                    Vector3 inside = center + Vector3.up * ((profile[p].y + profile[p + 1].y) * 0.5f);
                    Vector3 a = Point(profile[p], a0), b = Point(profile[p], a1), c = Point(profile[p + 1], a1), d = Point(profile[p + 1], a0);
                    if (profile[p].x <= 0.0001f) Triangle(a, c, d, inside);
                    else if (profile[p + 1].x <= 0.0001f) Triangle(a, b, d, inside);
                    else Quad(a, b, c, d, inside);
                }
            }
        }

        /// <summary>Low-poly sphere (subdivided octahedron) for bushes, clouds, canopies.</summary>
        public void LowSphere(Vector3 center, Vector3 radii, int subdivisions = 1)
        {
            var tris = new List<Vector3[]>
            {
                new[] { Vector3.up, Vector3.forward, Vector3.right }, new[] { Vector3.up, Vector3.right, Vector3.back },
                new[] { Vector3.up, Vector3.back, Vector3.left }, new[] { Vector3.up, Vector3.left, Vector3.forward },
                new[] { Vector3.down, Vector3.right, Vector3.forward }, new[] { Vector3.down, Vector3.back, Vector3.right },
                new[] { Vector3.down, Vector3.left, Vector3.back }, new[] { Vector3.down, Vector3.forward, Vector3.left }
            };
            for (int d = 0; d < subdivisions; d++)
            {
                var next = new List<Vector3[]>(tris.Count * 4);
                foreach (Vector3[] t in tris)
                {
                    Vector3 ab = ((t[0] + t[1]) * 0.5f).normalized, bc = ((t[1] + t[2]) * 0.5f).normalized, ca = ((t[2] + t[0]) * 0.5f).normalized;
                    next.Add(new[] { t[0], ab, ca });
                    next.Add(new[] { ab, t[1], bc });
                    next.Add(new[] { ca, bc, t[2] });
                    next.Add(new[] { ab, bc, ca });
                }
                tris = next;
            }
            foreach (Vector3[] t in tris)
                Triangle(center + Vector3.Scale(t[0], radii), center + Vector3.Scale(t[1], radii), center + Vector3.Scale(t[2], radii), center);
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name };
            if (_vertices.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(_vertices);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            int used = 0;
            for (int i = 0; i < _submeshes.Count; i++) if (_submeshes[i].Count > 0) used = i + 1;
            mesh.subMeshCount = Mathf.Max(1, used);
            for (int i = 0; i < mesh.subMeshCount; i++) mesh.SetTriangles(_submeshes[i], i);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static Vector2 PlanarUv(Vector3 p, Vector3 n)
        {
            Vector3 a = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
            if (a.x >= a.y && a.x >= a.z) return new Vector2(p.z, p.y);
            if (a.y >= a.z) return new Vector2(p.x, p.z);
            return new Vector2(p.x, p.y);
        }
    }
}
