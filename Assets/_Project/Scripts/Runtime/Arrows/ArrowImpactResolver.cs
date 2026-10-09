using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Pure impact rules (02 §6.2–6.4): picks the outcome for an arrow hitting a body and computes the clamped
    /// impulse and damage. Has no side effects, so the trajectory preview uses it too.
    /// </summary>
    public static class ArrowImpactResolver
    {
        public static ImpactOutcome Resolve(ArrowDefinition def, GameplayTuning tuning, Vector3 velocity, Vector3 normal,
            BodyEntry entry, ArrowHitReaction reaction, int ricochetsLeft)
        {
            float speed = velocity.magnitude;
            Vector3 dir = speed > 1e-4f ? velocity / speed : Vector3.down;
            float incidence = Vector3.Angle(-dir, normal);   // 0 = head-on
            float grazing = 90f - incidence;
            MaterialProfile profile = entry != null ? entry.Profile : null;

            ImpactKind kind;
            Vector3 after;
            float factorOverride = -1f;

            switch (reaction.Kind)
            {
                case ArrowHitReactionKind.PassThrough:
                    kind = ImpactKind.PassThrough;
                    after = velocity * reaction.SpeedFactor;
                    break;
                case ArrowHitReactionKind.Shatter:
                    kind = ImpactKind.Shatter;
                    after = velocity * reaction.SpeedFactor;
                    break;
                case ArrowHitReactionKind.Stop:
                    kind = ImpactKind.Stop;
                    after = Vector3.Reflect(velocity, normal) * 0.15f;
                    break;
                default:
                    kind = MaterialRule(def, entry, profile, speed, incidence, grazing, ricochetsLeft, out float speedFactor);
                    after = kind == ImpactKind.Ricochet || kind == ImpactKind.Deflect || kind == ImpactKind.Stop
                        ? Vector3.Reflect(velocity, normal) * speedFactor
                        : kind == ImpactKind.Shatter ? velocity * speedFactor : Vector3.zero;
                    break;
            }

            bool isStatic = entry == null || entry.Body == null || entry.Body.isKinematic;
            float transfer = profile != null ? profile.ImpulseTransfer : 0f;
            float j = isStatic ? 0f : def.ImpactImpulse * (speed / def.ReferenceSpeed) * transfer * OutcomeFactor(kind);
            if (factorOverride >= 0f) j *= factorOverride;
            j = Mathf.Clamp(j, 0f, def.MaxImpulse);
            float damage = j * def.DamageScale * (profile != null ? profile.ArrowDamageMultiplier : 0f);
            if (kind == ImpactKind.Shatter && entry != null && entry.Breakable != null)
                damage = Mathf.Max(damage, entry.Breakable.HP + 1f);

            float threshold = tuning != null ? tuning.MeaningfulImpulseThreshold : 3f;
            bool meaningful = reaction.Meaningful
                || (j >= threshold && (kind == ImpactKind.Embed || kind == ImpactKind.Stop || kind == ImpactKind.Shatter));
            return new ImpactOutcome(kind, after, j, damage, meaningful);
        }

        private static ImpactKind MaterialRule(ArrowDefinition def, BodyEntry entry, MaterialProfile profile, float speed,
            float incidence, float grazing, int ricochetsLeft, out float speedFactor)
        {
            speedFactor = 0f;
            if (profile == null || profile.Kind == MaterialKind.Earth)
                return entry != null && entry.Kind == BodyKind.Environment ? ImpactKind.Embed : ImpactKind.Stop;

            if (profile.RicochetMaxGrazingDeg > 0f && grazing <= profile.RicochetMaxGrazingDeg && ricochetsLeft > 0)
            {
                speedFactor = profile.RicochetSpeedFactor;
                return ImpactKind.Ricochet;
            }

            if (profile.ArrowEmbeds && incidence <= profile.EmbedMaxIncidenceDeg && speed >= profile.EmbedMinSpeed)
                return ImpactKind.Embed;

            Breakable breakable = entry != null ? entry.Breakable : null;
            if (breakable != null && !breakable.Unbreakable && breakable.HP > 0f)
            {
                float j = Mathf.Min(def.ImpactImpulse * (speed / def.ReferenceSpeed) * profile.ImpulseTransfer, def.MaxImpulse);
                if (j * def.DamageScale * profile.ArrowDamageMultiplier >= breakable.HP && profile.Kind == MaterialKind.Ice)
                {
                    speedFactor = 0.5f;
                    return ImpactKind.Shatter;
                }
            }

            if (profile.RicochetMaxGrazingDeg > 0f) return ImpactKind.Stop; // metal clank
            speedFactor = profile.DeflectFactor;
            return ImpactKind.Deflect;
        }

        /// <summary>02 §6.4 outcome factors.</summary>
        public static float OutcomeFactor(ImpactKind kind)
        {
            switch (kind)
            {
                case ImpactKind.Embed:
                case ImpactKind.Stop: return 1f;
                case ImpactKind.Deflect: return 0.6f;
                case ImpactKind.Shatter: return 0.5f;
                case ImpactKind.Ricochet: return 0.3f;
                case ImpactKind.PassThrough: return 0.2f;
                default: return 0f;
            }
        }
    }
}
