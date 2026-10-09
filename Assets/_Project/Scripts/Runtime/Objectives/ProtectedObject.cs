using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Must not be lost (D-038): any arrow hit, breaking (fragile impulse), entering a kill zone, or — for the fox —
    /// being displaced or tipped, fails the level immediately. Blasts never touch it (D-092, Explosion mask).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProtectedObject : MonoBehaviour, IArrowHittable
    {
        [SerializeField] private ProtectedKind _kind = ProtectedKind.RoyalVase;
        [SerializeField, Min(0.05f)] private float _maxDisplacement = 0.6f;
        [SerializeField, Range(5f, 90f)] private float _maxTiltDeg = 45f;

        private Breakable _breakable;
        private PlanarBody _planar;
        private Vector3 _startPosition;

        public ProtectedKind Kind => _kind;
        public bool IsLost { get; private set; }

        public event Action<ProtectedObject, ProtectedLossReason> Lost;

        public void SetKind(ProtectedKind kind) => _kind = kind;

        private void Awake()
        {
            _breakable = GetComponent<Breakable>();
            _planar = GetComponent<PlanarBody>();
            if (_breakable != null) _breakable.Broken += OnBroken;
            if (_planar != null) _planar.KilledBy += OnKilled;
        }

        private void Start() => _startPosition = transform.position;

        private void OnDestroy()
        {
            if (_breakable != null) _breakable.Broken -= OnBroken;
            if (_planar != null) _planar.KilledBy -= OnKilled;
        }

        public ArrowHitReaction Evaluate(in ArrowHitInfo hit) =>
            new ArrowHitReaction(ArrowHitReactionKind.Stop, 0f, meaningful: true);

        public void Apply(in ArrowHitInfo hit)
        {
            if (hit.IsPreview) return;
            Lose(ProtectedLossReason.Hit);
            if (_breakable != null) _breakable.RequestBreak(DamageSource.Arrow);
        }

        /// <summary>Displacement / tilt check for the sleeping fox (D-038), run each step by the tracker.</summary>
        public void Tick()
        {
            if (IsLost || _kind != ProtectedKind.SleepingFox) return;
            if (Vector3.Distance(transform.position, _startPosition) > _maxDisplacement ||
                Vector3.Angle(transform.up, Vector3.up) > _maxTiltDeg)
                Lose(ProtectedLossReason.Displaced);
        }

        private void OnBroken(Breakable breakable, DamageSource source) => Lose(ProtectedLossReason.Broken);

        private void OnKilled(KillZoneKind kind) => Lose(ProtectedLossReason.KillZone);

        private void Lose(ProtectedLossReason reason)
        {
            if (IsLost) return;
            IsLost = true;
            Lost?.Invoke(this, reason);
        }
    }
}
