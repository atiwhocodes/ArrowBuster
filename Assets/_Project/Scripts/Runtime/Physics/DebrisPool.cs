using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Cosmetic debris (D-008): pooled fragments on the Debris layer (collide with Environment only, never deal
    /// damage), unconstrained in Z for a 3D tumble, faded out after their lifetime. Oldest recycled on overflow.
    /// </summary>
    public sealed class DebrisPool : MonoBehaviour
    {
        private sealed class Piece
        {
            public GameObject GameObject;
            public Transform Transform;
            public Rigidbody Body;
            public MeshRenderer Renderer;
            public float Born;
            public float Lifetime;
            public float Size;
            public bool Active;
        }

        private readonly List<Piece> _pieces = new List<Piece>(GameConstants.MaxDebrisFragments);
        private MaterialPropertyBlock _block;
        private int _next;
        private int _cap = GameConstants.MaxDebrisFragments;

        public static DebrisPool Instance { get; private set; }

        /// <summary>Lowers the cap on the Low tier (20).</summary>
        public int Cap
        {
            get => _cap;
            set => _cap = Mathf.Clamp(value, 4, GameConstants.MaxDebrisFragments);
        }

        public int ActiveCount
        {
            get
            {
                int n = 0;
                foreach (Piece p in _pieces) if (p.Active) n++;
                return n;
            }
        }

        private void Awake()
        {
            Instance = this;
            _block = new MaterialPropertyBlock();
            AppConfig config = AppConfig.Load();
            Mesh mesh = config != null && config.DebrisMesh != null ? config.DebrisMesh : null;
            Material material = config != null ? config.DebrisMaterial : null;
            for (int i = 0; i < GameConstants.MaxDebrisFragments; i++) _pieces.Add(CreatePiece(i, mesh, material));
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Burst(Vector3 position, Vector3 inheritVelocity, Color color, int count, float size, float lifetime)
        {
            for (int i = 0; i < count; i++)
            {
                Piece piece = Acquire();
                Vector3 offset = CosmeticRandom.InsideUnitSphere() * size;
                Vector3 spread = CosmeticRandom.InsideUnitSphere();
                spread.y = Mathf.Abs(spread.y) + 0.3f;
                float pieceSize = size * CosmeticRandom.Range(0.55f, 1.05f);

                piece.Transform.position = position + offset;
                piece.Transform.rotation = Quaternion.Euler(CosmeticRandom.Range(0f, 360f), CosmeticRandom.Range(0f, 360f), CosmeticRandom.Range(0f, 360f));
                piece.Transform.localScale = Vector3.one * pieceSize;
                piece.Size = pieceSize;
                piece.GameObject.SetActive(true);
                piece.Body.linearVelocity = inheritVelocity * 0.6f + spread.normalized * CosmeticRandom.Range(1.5f, 3f);
                piece.Body.angularVelocity = CosmeticRandom.InsideUnitSphere() * 12f;
                piece.Born = Time.time;
                piece.Lifetime = lifetime * CosmeticRandom.Range(0.85f, 1.15f);
                piece.Active = true;

                piece.Renderer.GetPropertyBlock(_block);
                _block.SetColor("_BaseColor", Color.Lerp(color, Color.black, CosmeticRandom.Range(0f, 0.25f)));
                piece.Renderer.SetPropertyBlock(_block);
            }
        }

        public void ReleaseAll()
        {
            foreach (Piece p in _pieces)
            {
                p.Active = false;
                p.GameObject.SetActive(false);
            }
        }

        private void Update()
        {
            float now = Time.time;
            for (int i = 0; i < _pieces.Count; i++)
            {
                Piece p = _pieces[i];
                if (!p.Active) continue;
                float age = now - p.Born;
                if (age >= p.Lifetime + 0.25f || p.Transform.position.y < -10f)
                {
                    p.Active = false;
                    p.GameObject.SetActive(false);
                }
                else if (age > p.Lifetime)
                {
                    p.Transform.localScale = Vector3.one * p.Size * (1f - (age - p.Lifetime) / 0.25f);
                }
            }
        }

        private Piece Acquire()
        {
            for (int i = 0; i < _cap; i++)
            {
                Piece candidate = _pieces[(_next + i) % _cap];
                if (!candidate.Active)
                {
                    _next = (_next + i + 1) % _cap;
                    return candidate;
                }
            }
            Piece oldest = _pieces[0];
            for (int i = 1; i < _cap; i++) if (_pieces[i].Born < oldest.Born) oldest = _pieces[i];
            return oldest;
        }

        private Piece CreatePiece(int index, Mesh mesh, Material material)
        {
            var go = new GameObject("Debris_" + index) { layer = PhysicsLayers.Debris };
            go.transform.SetParent(transform, false);
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            if (mesh == null)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh = cube.GetComponent<MeshFilter>().sharedMesh;
                Destroy(cube);
            }
            filter.sharedMesh = mesh;
            if (material != null) renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var collider = go.AddComponent<BoxCollider>();
            collider.size = Vector3.one * 0.9f;
            var body = go.AddComponent<Rigidbody>();
            body.mass = 0.05f;
            body.linearDamping = 0.2f;
            body.angularDamping = 0.5f;
            body.interpolation = RigidbodyInterpolation.None;
            go.SetActive(false);
            return new Piece { GameObject = go, Transform = go.transform, Body = body, Renderer = renderer };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}
