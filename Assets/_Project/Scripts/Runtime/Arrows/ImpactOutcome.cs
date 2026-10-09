using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Result of <see cref="ArrowImpactResolver.Resolve"/> (02 §6.1).</summary>
    public readonly struct ImpactOutcome
    {
        public readonly ImpactKind Kind;
        public readonly Vector3 VelocityAfter;
        public readonly float Impulse;
        public readonly float Damage;
        public readonly bool Meaningful;

        public ImpactOutcome(ImpactKind kind, Vector3 velocityAfter, float impulse, float damage, bool meaningful)
        {
            Kind = kind;
            VelocityAfter = velocityAfter;
            Impulse = impulse;
            Damage = damage;
            Meaningful = meaningful;
        }

        /// <summary>True when the arrow keeps flying after this impact.</summary>
        public bool Continues => Kind == ImpactKind.PassThrough || Kind == ImpactKind.Shatter || Kind == ImpactKind.Ricochet;
    }
}
