using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Per-level lookup of gameplay colliders and tracked bodies (02 §6.1, §8). Rebuilt by the level loader after
    /// the layout is instantiated; colliders created later (e.g. rope triggers) register themselves.
    /// </summary>
    public static class PhysicsBodyRegistry
    {
        private static readonly Dictionary<Collider, BodyEntry> Entries = new Dictionary<Collider, BodyEntry>(128);
        private static readonly List<PlanarBody> Tracked = new List<PlanarBody>(64);

        public static IReadOnlyList<PlanarBody> TrackedBodies => Tracked;

        public static void Clear()
        {
            Entries.Clear();
            Tracked.Clear();
        }

        /// <summary>Indexes every collider under <paramref name="root"/> and collects tracked bodies.</summary>
        public static void Rebuild(Transform root)
        {
            Clear();
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true)) Register(c);
            foreach (PlanarBody body in root.GetComponentsInChildren<PlanarBody>(true))
                if ((PhysicsLayers.TrackedBodyLayers & (1 << body.gameObject.layer)) != 0 && !Tracked.Contains(body))
                    Tracked.Add(body);
        }

        public static BodyEntry Register(Collider c)
        {
            if (c == null) return null;
            if (Entries.TryGetValue(c, out BodyEntry existing)) return existing;
            var entry = new BodyEntry
            {
                Collider = c,
                Body = c.attachedRigidbody,
                Kind = KindForLayer(c.gameObject.layer),
                Material = c.GetComponentInParent<MaterialBody>(),
                Breakable = c.GetComponentInParent<Breakable>(),
                Planar = c.GetComponentInParent<PlanarBody>(),
                Hittable = c.GetComponentInParent<IArrowHittable>()
            };
            Entries[c] = entry;
            return entry;
        }

        public static BodyEntry Get(Collider c)
        {
            if (c == null) return null;
            return Entries.TryGetValue(c, out BodyEntry entry) ? entry : Register(c);
        }

        public static BodyKind KindForLayer(int layer)
        {
            switch (layer)
            {
                case PhysicsLayers.Environment: return BodyKind.Environment;
                case PhysicsLayers.Structure: return BodyKind.Structure;
                case PhysicsLayers.Objective: return BodyKind.Objective;
                case PhysicsLayers.Protected: return BodyKind.Protected;
                case PhysicsLayers.Prop: return BodyKind.Prop;
                case PhysicsLayers.Rope: return BodyKind.Rope;
                case PhysicsLayers.Portal: return BodyKind.Portal;
                default: return BodyKind.Unknown;
            }
        }

        /// <summary>Puts every tracked dynamic body to sleep (D-006: bodies start asleep).</summary>
        public static void SleepAll()
        {
            foreach (PlanarBody planar in Tracked)
            {
                if (planar == null) continue;
                Rigidbody body = planar.Body;
                if (body.isKinematic) continue;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.Sleep();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Clear();
    }
}
