using UnityEngine;

namespace ArrowBuster
{
    /// <summary>What an arrow hit, passed to <see cref="IArrowHittable"/> (02 §6.2).</summary>
    public readonly struct ArrowHitInfo
    {
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly Vector3 Velocity;
        public readonly ArrowType ArrowType;
        public readonly bool IsPreview;

        public ArrowHitInfo(Vector3 point, Vector3 normal, Vector3 velocity, ArrowType arrowType, bool isPreview)
        {
            Point = point;
            Normal = normal;
            Velocity = velocity;
            ArrowType = arrowType;
            IsPreview = isPreview;
        }
    }
}
