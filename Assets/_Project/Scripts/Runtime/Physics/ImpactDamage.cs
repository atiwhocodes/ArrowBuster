using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Turns collision impulses into <see cref="Breakable"/> damage (03 §5.2) with the §5.3 guard rails:
    /// 0.5 s spawn grace, resting contacts ignored, one hit per body pair per 0.1 s.
    /// </summary>
    [RequireComponent(typeof(Breakable))]
    [DisallowMultipleComponent]
    public sealed class ImpactDamage : MonoBehaviour
    {
        private const float SpawnGraceSeconds = 0.5f;
        private const float MinRelativeSpeed = 0.5f;
        private const float PairCooldownSeconds = 0.1f;

        private readonly Dictionary<Collider, float> _lastHitByCollider = new Dictionary<Collider, float>(8);
        private Breakable _breakable;

        /// <summary>Largest impulse received since level load (diagnostics and tests).</summary>
        public float PeakImpulse { get; private set; }

        private void Awake() => _breakable = GetComponent<Breakable>();

        private void OnCollisionEnter(Collision collision) => Handle(collision);

        private void Handle(Collision collision)
        {
            if (LevelClock.Now < SpawnGraceSeconds) return;
            if (collision.relativeVelocity.magnitude < MinRelativeSpeed) return;

            Collider other = collision.collider;
            float now = LevelClock.Now;
            if (_lastHitByCollider.TryGetValue(other, out float last) && now - last < PairCooldownSeconds) return;
            _lastHitByCollider[other] = now;

            float impulse = collision.impulse.magnitude;
            if (impulse > PeakImpulse) PeakImpulse = impulse;
            _breakable.ReceiveImpulse(impulse);
        }
    }
}
