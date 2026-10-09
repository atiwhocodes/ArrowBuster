using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// The sphere sweep shared by flying arrows and the trajectory preview (02 §3.2, §5.1), so both always see the
    /// same colliders. Triggers count only on Rope/Portal layers or when they carry an <see cref="IArrowHittable"/>.
    /// </summary>
    public static class ArrowSweep
    {
        private static readonly RaycastHit[] Buffer = new RaycastHit[24];

        public static bool Cast(Vector3 from, Vector3 to, float radius, HashSet<Collider> ignore, out RaycastHit best)
        {
            best = default;
            Vector3 delta = to - from;
            float distance = delta.magnitude;
            if (distance < 1e-5f) return false;
            Vector3 dir = delta / distance;

            int count = Physics.SphereCastNonAlloc(from, radius, dir, Buffer, distance, PhysicsLayers.ArrowCastMask,
                QueryTriggerInteraction.Collide);
            float bestDistance = float.MaxValue;
            bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Buffer[i];
                Collider c = hit.collider;
                if (c == null || !c.enabled) continue;
                if (ignore != null && ignore.Contains(c)) continue;
                if (c.isTrigger && !AcceptsTrigger(c)) continue;

                if (hit.distance <= 0f && hit.point == Vector3.zero)
                {
                    // Started inside: report at the sweep origin, facing back along the path.
                    hit.point = from;
                    hit.normal = -dir;
                }
                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    best = hit;
                    found = true;
                }
            }
            return found;
        }

        /// <summary>True if any kill zone overlaps the point (KillZone is not in the cast mask on purpose).</summary>
        public static bool InKillZone(Vector3 point, float radius, out Collider zone)
        {
            zone = null;
            int count = Physics.OverlapSphereNonAlloc(point, radius, Overlaps, PhysicsLayers.KillZoneMask, QueryTriggerInteraction.Collide);
            if (count > 0) zone = Overlaps[0];
            return count > 0;
        }

        private static readonly Collider[] Overlaps = new Collider[4];

        private static bool AcceptsTrigger(Collider c)
        {
            int layer = c.gameObject.layer;
            if (layer == PhysicsLayers.Rope || layer == PhysicsLayers.Portal) return true;
            BodyEntry entry = PhysicsBodyRegistry.Get(c);
            return entry != null && entry.Hittable != null;
        }
    }
}
