using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Registry record for one collider (02 §6.1): resolved once on level load, never at hit time.</summary>
    public sealed class BodyEntry
    {
        public Collider Collider;
        public Rigidbody Body;
        public BodyKind Kind;
        public MaterialBody Material;
        public Breakable Breakable;
        public PlanarBody Planar;
        public IArrowHittable Hittable;

        public MaterialKind MaterialKind => Material != null ? Material.Kind : MaterialKind.Earth;
        public MaterialProfile Profile => Material != null ? Material.Profile : null;
    }
}
