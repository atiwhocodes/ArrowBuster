using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Durability for one body (03 §5): HP from the material and size class, a damaged state at ≤ 50 % HP and a
    /// deterministic break. Breaks requested during a physics step are flushed at the start of the next
    /// FixedUpdate in request order (<see cref="FlushPendingBreaks"/>). Broken bodies spawn cosmetic debris only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Breakable : MonoBehaviour
    {
        private static readonly List<Breakable> Pending = new List<Breakable>(16);

        [Tooltip("Optional HP override. 0 = profile max HP × size multiplier.")]
        [SerializeField, Min(0f)] private float _hpOverride;
        [Tooltip("Optional single-impact break threshold (Ns). 0 = profile value.")]
        [SerializeField, Min(0f)] private float _breakImpulseOverride;
        [Tooltip("Optional collision damage threshold (Ns). 0 = profile value.")]
        [SerializeField, Min(0f)] private float _damageThresholdOverride;
        [SerializeField] private bool _spawnDebris = true;

        private MaterialBody _materialBody;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _block;
        private DamageSource _pendingCause;
        private bool _damagedShown;

        /// <summary>Raised once when the body breaks (local wiring: objectives, ropes, embedded arrows).</summary>
        public event Action<Breakable, DamageSource> Broken;

        public float MaxHP { get; private set; }
        public float HP { get; private set; }
        public bool IsBroken { get; private set; }
        public bool IsPending { get; private set; }
        public MaterialKind Kind => _materialBody != null ? _materialBody.Kind : MaterialKind.Timber;
        public MaterialProfile Profile => _materialBody != null ? _materialBody.Profile : null;

        public float BreakImpulse => _breakImpulseOverride > 0f ? _breakImpulseOverride
            : Profile != null ? Profile.BreakImpulse : 25f;

        public float DamageThreshold => _damageThresholdOverride > 0f ? _damageThresholdOverride
            : Profile != null ? Profile.DamageThreshold : 2.5f;

        public bool Unbreakable => Profile != null && Profile.Unbreakable && _hpOverride <= 0f;

        public void Configure(float hpOverride, float breakImpulseOverride, float damageThresholdOverride)
        {
            _hpOverride = hpOverride;
            _breakImpulseOverride = breakImpulseOverride;
            _damageThresholdOverride = damageThresholdOverride;
        }

        private void Awake()
        {
            _materialBody = GetComponent<MaterialBody>();
            _renderers = GetComponentsInChildren<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        private void Start()
        {
            float cells = _materialBody != null ? _materialBody.Cells : 1f;
            float sizeMultiplier = cells <= 0.5f ? 0.4f : cells > 1.5f ? 1.8f : 1f;
            MaxHP = _hpOverride > 0f ? _hpOverride : (Profile != null ? Profile.MaxHP : 20f) * sizeMultiplier;
            HP = MaxHP;
        }

        /// <summary>Collision entry point (03 §5.2): instant break above the break impulse, else thresholded damage.</summary>
        public void ReceiveImpulse(float impulse)
        {
            if (IsBroken || IsPending || Unbreakable) return;
            if (impulse >= BreakImpulse)
            {
                RequestBreak(DamageSource.Collision);
                return;
            }
            float scale = Profile != null ? Profile.CollisionDamageScale : 0.5f;
            float damage = Mathf.Max(0f, impulse - DamageThreshold) * scale;
            if (damage > 0f) ApplyDamage(damage, DamageSource.Collision);
        }

        public void ApplyDamage(float damage, DamageSource source)
        {
            if (IsBroken || IsPending || Unbreakable || damage <= 0f) return;
            if (MaxHP <= 0f) Start();
            HP -= damage;
            if (HP <= 0f)
            {
                RequestBreak(source);
                return;
            }
            if (!_damagedShown && HP <= MaxHP * 0.5f) ShowDamaged();
        }

        /// <summary>Queues a break; it happens at the start of the next FixedUpdate (deterministic order).</summary>
        public void RequestBreak(DamageSource source)
        {
            if (IsBroken || IsPending) return;
            IsPending = true;
            _pendingCause = source;
            Pending.Add(this);
        }

        /// <summary>Called once per FixedUpdate by the gameplay controller before anything else.</summary>
        public static void FlushPendingBreaks()
        {
            for (int i = 0; i < Pending.Count; i++)
                if (Pending[i] != null) Pending[i].BreakNow(Pending[i]._pendingCause);
            Pending.Clear();
        }

        /// <summary>Drops queued breaks (level unload).</summary>
        public static void ClearPending() => Pending.Clear();

        private void BreakNow(DamageSource source)
        {
            if (IsBroken) return;
            IsBroken = true;
            IsPending = false;

            Vector3 position = transform.position;
            float size = _materialBody != null ? _materialBody.Cells : 1f;
            var body = GetComponent<Rigidbody>();
            Vector3 velocity = body != null && !body.isKinematic ? body.linearVelocity : Vector3.zero;

            Broken?.Invoke(this, source);
            GameEvents.RaiseObjectBroken(new BreakInfo(Kind, position, size, source));

            if (_spawnDebris && DebrisPool.Instance != null)
            {
                int count = Profile != null ? Profile.DebrisMedium : 4;
                if (size <= 0.5f) count = Mathf.Max(2, count - 1);
                else if (size > 1.5f) count += 2;
                Color color = Profile != null ? Profile.Color : Color.gray;
                float lifetime = Profile != null ? Profile.DebrisLifetime : 1.5f;
                DebrisPool.Instance.Burst(position, velocity, color, count, Mathf.Clamp(Mathf.Sqrt(size) * 0.35f, 0.12f, 0.45f), lifetime);
            }

            var planar = GetComponent<PlanarBody>();
            if (planar != null) planar.MarkRemoved();
            if (body != null)
            {
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
                body.isKinematic = true;
            }
            foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (Renderer r in _renderers) if (r != null) r.enabled = false;
        }

        private void ShowDamaged()
        {
            _damagedShown = true;
            foreach (Renderer r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_block);
                _block.SetColor("_BaseColor", Color.Lerp(r.sharedMaterial != null && r.sharedMaterial.HasProperty("_BaseColor")
                    ? r.sharedMaterial.GetColor("_BaseColor") : Color.white, Color.black, 0.28f));
                r.SetPropertyBlock(_block);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Pending.Clear();
    }
}
