using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// A required objective (D-037). Clears when broken, when it enters a kill zone, or (crest / crate / dummy)
    /// when its centre drops below the level clear line. Crest targets also stop arrows and break on a direct hit;
    /// cursed orbs and lanterns shatter and let the arrow continue.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Objective : MonoBehaviour, IArrowHittable
    {
        [SerializeField] private ObjectiveKind _kind = ObjectiveKind.CrestTarget;
        [Tooltip("Tilt (degrees from upright) that counts as knocked over (TrainingDummy).")]
        [SerializeField, Range(10f, 90f)] private float _knockOverAngle = 70f;

        private Breakable _breakable;
        private PlanarBody _planar;

        public ObjectiveKind Kind => _kind;
        public bool IsCleared { get; private set; }
        public int Index { get; set; }

        /// <summary>Raised once when the objective clears.</summary>
        public event Action<Objective> Cleared;

        public void SetKind(ObjectiveKind kind) => _kind = kind;

        private void Awake()
        {
            _breakable = GetComponent<Breakable>();
            _planar = GetComponent<PlanarBody>();
            if (_breakable != null) _breakable.Broken += OnBroken;
            if (_planar != null) _planar.KilledBy += OnKilled;
        }

        private void OnDestroy()
        {
            if (_breakable != null) _breakable.Broken -= OnBroken;
            if (_planar != null) _planar.KilledBy -= OnKilled;
        }

        public ArrowHitReaction Evaluate(in ArrowHitInfo hit)
        {
            if (IsCleared) return ArrowHitReaction.UseMaterial;
            switch (_kind)
            {
                case ObjectiveKind.CrestTarget: return new ArrowHitReaction(ArrowHitReactionKind.Stop, 0f, meaningful: true);
                case ObjectiveKind.CursedOrb:
                case ObjectiveKind.HangingLantern: return new ArrowHitReaction(ArrowHitReactionKind.Shatter, 0.7f, meaningful: true);
                default: return ArrowHitReaction.UseMaterial;
            }
        }

        public void Apply(in ArrowHitInfo hit)
        {
            if (IsCleared || hit.IsPreview) return;
            if (_kind == ObjectiveKind.CrestTarget || _kind == ObjectiveKind.CursedOrb || _kind == ObjectiveKind.HangingLantern)
            {
                if (_breakable != null) _breakable.RequestBreak(DamageSource.Arrow);
                else Clear();
            }
        }

        /// <summary>Per-step rule checks run by the tracker (clear line, knocked over).</summary>
        public void CheckRules(bool clearLineEnabled, float clearLineY)
        {
            if (IsCleared) return;
            bool lineKind = _kind == ObjectiveKind.CrestTarget || _kind == ObjectiveKind.SupplyCrate || _kind == ObjectiveKind.TrainingDummy;
            if (clearLineEnabled && lineKind && transform.position.y < clearLineY)
            {
                Clear();
                return;
            }
            if (_kind == ObjectiveKind.TrainingDummy && Vector3.Angle(transform.up, Vector3.up) > _knockOverAngle) Clear();
        }

        public void Clear()
        {
            if (IsCleared) return;
            IsCleared = true;
            Cleared?.Invoke(this);
        }

        private void OnBroken(Breakable breakable, DamageSource source) => Clear();

        private void OnKilled(KillZoneKind kind) => Clear();
    }
}
