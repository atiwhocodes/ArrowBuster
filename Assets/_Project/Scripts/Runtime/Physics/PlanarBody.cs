using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Locks a gameplay rigidbody to the play plane (D-004): z = PlayPlaneZ, no X/Y rotation. Also carries the
    /// "removed" and "ambient" flags used by settle detection and the latched kill-zone notification (D-039).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public sealed class PlanarBody : MonoBehaviour
    {
        private Rigidbody _body;
        private float _ambientUntil = -1f;

        /// <summary>Raised once when the body enters a kill zone or leaves the play bounds.</summary>
        public event Action<KillZoneKind> KilledBy;

        public Rigidbody Body => _body != null ? _body : (_body = GetComponent<Rigidbody>());
        public bool Removed { get; private set; }

        /// <summary>Ambient bodies (on movers, hanging from balloons) never block "calm" (02 §8.1).</summary>
        public bool IsAmbient => LevelClock.Now < _ambientUntil;

        public void MarkAmbient(float untilLevelTime) => _ambientUntil = Mathf.Max(_ambientUntil, untilLevelTime);

        private void Awake()
        {
            Rigidbody body = Body;
            body.constraints |= RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            body.maxDepenetrationVelocity = PhysicsSettingsSpec.DefaultMaxDepenetrationVelocity;
            Vector3 p = transform.position;
            if (!Mathf.Approximately(p.z, GameConstants.PlayPlaneZ))
                transform.position = new Vector3(p.x, p.y, GameConstants.PlayPlaneZ);
        }

        /// <summary>Latched kill: raises <see cref="KilledBy"/> once, then hides and parks the body.</summary>
        public void Kill(KillZoneKind kind)
        {
            if (Removed) return;
            Removed = true;
            KilledBy?.Invoke(kind);
            Park();
        }

        /// <summary>Marks the body removed (e.g. broken) without the kill notification.</summary>
        public void MarkRemoved() => Removed = true;

        private void Park()
        {
            Rigidbody body = Body;
            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.isKinematic = true;
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
    }
}
