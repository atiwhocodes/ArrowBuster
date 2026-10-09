using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Links a body or static collider to its <see cref="MaterialProfile"/>: applies the physics material and,
    /// for dynamic bodies, mass = density × collider volume (03 §4).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaterialBody : MonoBehaviour
    {
        [SerializeField] private MaterialProfile _profile;
        [Tooltip("Optional explicit mass (kg). 0 = derive from the profile density and collider volume.")]
        [SerializeField, Min(0f)] private float _massOverride;
        [Tooltip("Depth used for volume (the play plane is 2.5D; bodies are about 1 m deep).")]
        [SerializeField, Min(0.1f)] private float _depthForVolume = 1f;

        public MaterialProfile Profile => _profile;
        public MaterialKind Kind => _profile != null ? _profile.Kind : MaterialKind.Earth;

        /// <summary>Volume in cells (x × y × depth), used for size class and mass.</summary>
        public float Cells { get; private set; } = 1f;

        public void SetProfile(MaterialProfile profile) => _profile = profile;

        private void Awake()
        {
            if (_profile == null) return;
            PhysicsMaterial physicsMaterial = _profile.PhysicsMaterial;
            float area = 0f;
            foreach (Collider c in GetComponentsInChildren<Collider>(true))
            {
                if (c.isTrigger) continue;
                c.sharedMaterial = physicsMaterial;
                area += FaceArea(c);
            }
            // Collider.bounds can be stale right after Instantiate (auto-sync transforms is off), so use shapes.
            if (area > 0f) Cells = Mathf.Max(0.05f, area * _depthForVolume);

            var body = GetComponent<Rigidbody>();
            if (body != null && !body.isKinematic)
                body.mass = _massOverride > 0f ? _massOverride : Mathf.Max(0.05f, _profile.MassPerCell * Cells);
        }

        /// <summary>Approximate XY face area of a collider in world units.</summary>
        private static float FaceArea(Collider c)
        {
            Vector3 s = c.transform.lossyScale;
            switch (c)
            {
                case BoxCollider box:
                    return Mathf.Abs(box.size.x * s.x * box.size.y * s.y);
                case SphereCollider sphere:
                {
                    float r = sphere.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
                    return Mathf.PI * r * r;
                }
                case CapsuleCollider capsule:
                {
                    float r = capsule.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.z));
                    float h = Mathf.Max(capsule.height * Mathf.Abs(s.y), 2f * r);
                    return 2f * r * (h - 2f * r) + Mathf.PI * r * r;
                }
                default:
                {
                    Vector3 size = c.bounds.size;
                    return size.x * size.y;
                }
            }
        }
    }
}
