using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// A pooled arrow (02 §5): kinematic swept flight in FixedUpdate driven by <see cref="BallisticSolver"/>,
    /// impacts resolved by <see cref="ArrowImpactResolver"/>, then Embedded / Spent / resolved. The root pivot is
    /// the arrow TIP; the mesh points along +Z.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArrowProjectile : MonoBehaviour
    {
        public enum Phase { Pooled, Flying, Embedded, Spent, Decor }

        private const int MaxSubCasts = 4;
        private const float EmbedPenetration = 0.08f;
        private const float VibrateAmplitudeDeg = 6f;
        private const float VibrateHz = 18f;

        [SerializeField] private Collider _spentCollider;
        [SerializeField] private Transform _visual;

        private readonly HashSet<Collider> _passed = new HashSet<Collider>();
        private Rigidbody _body;
        private ArrowDefinition _def;
        private GameplayTuning _tuning;
        private IFlightEnvironment _env;
        private ArrowFlightState _state;
        private Vector3 _prevPosition;
        private Vector3 _prevVelocity;
        private Breakable _hostBreakable;
        private PlanarBody _hostPlanar;
        private float _phaseStart;
        private Rect _bounds;

        public Phase State { get; private set; } = Phase.Pooled;
        public int Id { get; private set; }
        public ArrowType Type => _def != null ? _def.Type : ArrowType.Oak;
        public float FiredAt { get; private set; }
        public bool IsFlying => State == Phase.Flying;

        /// <summary>Raised when flight ends (once per arrow).</summary>
        public event Action<ArrowProjectile, ArrowResolution> Resolved;

        /// <summary>Raised when the arrow should go back to its pool.</summary>
        public event Action<ArrowProjectile> Released;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _body.isKinematic = true;
            _body.interpolation = RigidbodyInterpolation.None;
            if (_spentCollider != null) _spentCollider.enabled = false;
            gameObject.layer = PhysicsLayers.Arrow;
        }

        public void Fire(int id, ArrowDefinition def, GameplayTuning tuning, IFlightEnvironment env, Vector3 tip, Vector3 velocity, Rect bounds)
        {
            Id = id;
            _def = def;
            _tuning = tuning;
            _env = env ?? NullFlightEnvironment.Instance;
            _bounds = bounds;
            _passed.Clear();
            DetachHost();
            transform.SetParent(null, true);
            transform.localScale = Vector3.one;
            if (_visual != null) _visual.localRotation = Quaternion.identity;

            _state = BallisticSolver.Launch(tip, velocity, def.MaxRicochets);
            _prevPosition = _state.Position;
            _prevVelocity = _state.Velocity;
            ApplyPose(_state.Position, _state.Velocity);

            _body.isKinematic = true;
            if (_spentCollider != null) _spentCollider.enabled = false;
            FiredAt = LevelClock.Now;
            _phaseStart = Time.time;
            State = Phase.Flying;
            gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            switch (State)
            {
                case Phase.Flying: StepFlight(Time.fixedDeltaTime); break;
                case Phase.Spent:
                    if (Time.time - _phaseStart > (_tuning != null ? _tuning.SpentArrowFadeSeconds : 1.5f)) Release();
                    break;
            }
        }

        private void Update()
        {
            if (State == Phase.Flying)
            {
                float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
                ApplyPose(Vector3.Lerp(_prevPosition, _state.Position, alpha), Vector3.Lerp(_prevVelocity, _state.Velocity, alpha));
            }
            else if (State == Phase.Embedded && _visual != null)
            {
                float t = Time.time - _phaseStart;
                if (t < GameConstants.LodgedArrowVibrateSeconds)
                {
                    float decay = Mathf.Exp(-t * 18f);
                    float angle = Mathf.Sin(t * VibrateHz * Mathf.PI * 2f) * VibrateAmplitudeDeg * decay;
                    _visual.localRotation = Quaternion.Euler(angle, 0f, 0f);
                }
                else _visual.localRotation = Quaternion.identity;
            }
            else if (State == Phase.Spent)
            {
                float fade = _tuning != null ? _tuning.SpentArrowFadeSeconds : 1.5f;
                float t = (Time.time - _phaseStart) / fade;
                if (t > 0.6f) transform.localScale = Vector3.one * Mathf.Clamp01(1f - (t - 0.6f) / 0.4f);
            }
        }

        private void StepFlight(float dt)
        {
            _prevPosition = _state.Position;
            _prevVelocity = _state.Velocity;
            ArrowFlightState next = BallisticSolver.Step(_state, dt, _tuning.ArrowGravity, _def.GravityScale, _env, _def.WindResponse);

            Vector3 from = _state.Position;
            Vector3 to = next.Position;
            Vector3 velocity = next.Velocity;
            int ricochets = _state.RicochetsLeft;

            for (int cast = 0; cast < MaxSubCasts; cast++)
            {
                if (!ArrowSweep.Cast(from, to, _tuning.SweepRadius, _passed, out RaycastHit hit)) break;

                ImpactOutcome outcome = HandleHit(hit, velocity, ricochets);
                if (State != Phase.Flying) return;

                float remaining = Mathf.Max(0f, (to - from).magnitude - hit.distance);
                Vector3 hitPoint = from + (to - from).normalized * hit.distance;
                if (outcome.Kind == ImpactKind.Ricochet)
                {
                    ricochets--;
                    velocity = outcome.VelocityAfter;
                    from = hitPoint + hit.normal * (_tuning.SweepRadius + 0.01f);
                    to = from + velocity.normalized * remaining;
                }
                else
                {
                    _passed.Add(hit.collider);
                    velocity = outcome.VelocityAfter;
                    from = hitPoint;
                    to = from + velocity.normalized * remaining;
                }
            }

            _state = new ArrowFlightState(to, velocity, next.Time, ricochets);

            if (ArrowSweep.InKillZone(_state.Position, _tuning.SweepRadius, out _))
            {
                Resolve(ArrowResolution.KillZone);
                State = Phase.Pooled;
                Release();
                return;
            }
            Vector3 p = _state.Position;
            bool outOfBounds = p.x < _bounds.xMin || p.x > _bounds.xMax || p.y < _bounds.yMin || p.y > _bounds.yMax;
            if (outOfBounds || _state.Time > _tuning.ArrowMaxLifetime)
            {
                Resolve(ArrowResolution.OutOfBounds);
                State = Phase.Pooled;
                Release();
            }
        }

        private ImpactOutcome HandleHit(RaycastHit hit, Vector3 velocity, int ricochets)
        {
            BodyEntry entry = PhysicsBodyRegistry.Get(hit.collider);
            var info = new ArrowHitInfo(hit.point, hit.normal, velocity, _def.Type, isPreview: false);
            ArrowHitReaction reaction = entry != null && entry.Hittable != null ? entry.Hittable.Evaluate(info) : ArrowHitReaction.UseMaterial;
            ImpactOutcome outcome = ArrowImpactResolver.Resolve(_def, _tuning, velocity, hit.normal, entry, reaction, ricochets);

            if (outcome.Impulse > 0f && entry != null && entry.Body != null && !entry.Body.isKinematic)
            {
                Vector3 dir = velocity;
                dir.z = 0f;
                entry.Body.WakeUp();
                entry.Body.AddForceAtPosition(dir.normalized * outcome.Impulse, hit.point, ForceMode.Impulse);
            }
            if (outcome.Damage > 0f && entry != null && entry.Breakable != null)
                entry.Breakable.ApplyDamage(outcome.Damage, DamageSource.Arrow);
            if (entry != null && entry.Hittable != null) entry.Hittable.Apply(info);

            GameEvents.RaiseArrowImpact(new ArrowImpactInfo(_def.Type, entry != null ? entry.MaterialKind : MaterialKind.Earth,
                entry != null ? entry.Kind : BodyKind.Unknown, hit.point, hit.normal, velocity.magnitude, outcome.Kind, outcome.Meaningful));

            switch (outcome.Kind)
            {
                case ImpactKind.Embed:
                    Embed(hit, velocity, entry);
                    break;
                case ImpactKind.Stop:
                case ImpactKind.Deflect:
                case ImpactKind.Ignore:
                    BecomeSpent(hit.point, velocity, outcome.VelocityAfter);
                    break;
            }
            return outcome;
        }

        private void Embed(RaycastHit hit, Vector3 velocity, BodyEntry entry)
        {
            Vector3 dir = velocity.sqrMagnitude > 1e-6f ? velocity.normalized : Vector3.down;
            ApplyPose(hit.point + dir * EmbedPenetration, velocity);
            Transform host = entry != null && entry.Body != null ? entry.Body.transform : hit.collider.transform;
            transform.SetParent(host, true);

            if (entry != null)
            {
                _hostBreakable = entry.Breakable;
                _hostPlanar = entry.Planar;
                if (_hostBreakable != null) _hostBreakable.Broken += OnHostBroken;
                if (_hostPlanar != null) _hostPlanar.KilledBy += OnHostKilled;
            }
            State = Phase.Embedded;
            _phaseStart = Time.time;
            Resolve(ArrowResolution.Embedded);
        }

        private void BecomeSpent(Vector3 point, Vector3 velocity, Vector3 after)
        {
            DetachHost();
            transform.SetParent(null, true);
            ApplyPose(point - velocity.normalized * 0.02f, velocity);
            State = Phase.Spent;
            _phaseStart = Time.time;
            _body.isKinematic = false;
            _body.constraints = RigidbodyConstraints.FreezePositionZ;
            _body.mass = 0.05f;
            _body.linearVelocity = after;
            _body.angularVelocity = new Vector3(0f, 0f, CosmeticRandom.Range(-8f, 8f));
            if (_spentCollider != null) _spentCollider.enabled = true;
            Resolve(ArrowResolution.Spent);
        }

        /// <summary>Cap eviction (02 §5.1): stays as a visual, script logic stops.</summary>
        public void BecomeDecor()
        {
            if (State != Phase.Embedded) return;
            DetachHost();
            State = Phase.Decor;
            if (_visual != null) _visual.localRotation = Quaternion.identity;
        }

        public void Release()
        {
            if (State == Phase.Flying) Resolve(ArrowResolution.OutOfBounds);
            DetachHost();
            if (_spentCollider != null) _spentCollider.enabled = false;
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
            _body.isKinematic = true;
            State = Phase.Pooled;
            gameObject.SetActive(false);
            Released?.Invoke(this);
        }

        private void OnHostBroken(Breakable breakable, DamageSource source)
        {
            Vector3 v = Vector3.down * 2f;
            BecomeSpentFromEmbedded(v);
        }

        private void OnHostKilled(KillZoneKind kind) => Release();

        private void BecomeSpentFromEmbedded(Vector3 velocity)
        {
            if (State != Phase.Embedded && State != Phase.Decor) return;
            DetachHost();
            transform.SetParent(null, true);
            State = Phase.Spent;
            _phaseStart = Time.time;
            _body.isKinematic = false;
            _body.constraints = RigidbodyConstraints.FreezePositionZ;
            _body.linearVelocity = velocity;
            if (_spentCollider != null) _spentCollider.enabled = true;
        }

        private void Resolve(ArrowResolution resolution)
        {
            Resolved?.Invoke(this, resolution);
            GameEvents.RaiseArrowResolved(new ArrowResolvedInfo(Id, resolution));
        }

        private void DetachHost()
        {
            if (_hostBreakable != null) _hostBreakable.Broken -= OnHostBroken;
            if (_hostPlanar != null) _hostPlanar.KilledBy -= OnHostKilled;
            _hostBreakable = null;
            _hostPlanar = null;
        }

        private void ApplyPose(Vector3 position, Vector3 velocity)
        {
            transform.position = position;
            if (velocity.sqrMagnitude > 1e-6f) transform.rotation = Quaternion.LookRotation(velocity.normalized, Vector3.back);
        }
    }
}
