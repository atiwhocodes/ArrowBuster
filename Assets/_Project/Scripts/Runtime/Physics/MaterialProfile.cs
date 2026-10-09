using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Material tuning (03 §4): mass density, friction, bounce, durability, arrow response, debris and colour.
    /// One asset per material: ScriptableObjects/Materials/MP_&lt;Material&gt;.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Material Profile", fileName = "MP_Timber")]
    public sealed class MaterialProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private MaterialKind _kind = MaterialKind.Timber;
        [SerializeField] private Color _color = Color.white;

        [Header("Physics")]
        [Tooltip("kg per 1 m³ cell (03 §4.1).")]
        [SerializeField, Min(0.01f)] private float _massPerCell = 4f;
        [SerializeField, Range(0f, 1.5f)] private float _staticFriction = 0.65f;
        [SerializeField, Range(0f, 1.5f)] private float _dynamicFriction = 0.55f;
        [SerializeField] private PhysicsMaterialCombine _frictionCombine = PhysicsMaterialCombine.Average;
        [SerializeField, Range(0f, 1f)] private float _bounciness = 0.05f;

        [Header("Durability")]
        [SerializeField] private bool _unbreakable;
        [SerializeField, Min(0f)] private float _maxHP = 20f;
        [Tooltip("Collision impulse (Ns) below which no damage is dealt.")]
        [SerializeField, Min(0f)] private float _damageThreshold = 2.5f;
        [SerializeField, Min(0f)] private float _collisionDamageScale = 0.5f;
        [Tooltip("A single impact at or above this impulse (Ns) breaks the body instantly.")]
        [SerializeField, Min(0f)] private float _breakImpulse = 25f;

        [Header("Arrow response (02 §6.3)")]
        [SerializeField] private bool _arrowEmbeds = true;
        [SerializeField, Range(0f, 90f)] private float _embedMaxIncidenceDeg = 60f;
        [SerializeField, Min(0f)] private float _embedMinSpeed = 5f;
        [SerializeField, Range(0f, 1f)] private float _deflectFactor = 0.3f;
        [SerializeField, Range(0f, 90f)] private float _ricochetMaxGrazingDeg;
        [SerializeField, Range(0f, 1f)] private float _ricochetSpeedFactor = 0.6f;
        [SerializeField, Range(0f, 2f)] private float _impulseTransfer = 0.9f;
        [SerializeField, Range(0f, 3f)] private float _arrowDamageMultiplier = 1f;

        [Header("Debris (D-008)")]
        [SerializeField, Range(0, 8)] private int _debrisMedium = 4;
        [SerializeField, Min(0.1f)] private float _debrisLifetime = 1.6f;

        private PhysicsMaterial _physicsMaterial;

        public MaterialKind Kind => _kind;
        public Color Color => _color;
        public float MassPerCell => _massPerCell;
        public bool Unbreakable => _unbreakable;
        public float MaxHP => _maxHP;
        public float DamageThreshold => _damageThreshold;
        public float CollisionDamageScale => _collisionDamageScale;
        public float BreakImpulse => _breakImpulse;
        public bool ArrowEmbeds => _arrowEmbeds;
        public float EmbedMaxIncidenceDeg => _embedMaxIncidenceDeg;
        public float EmbedMinSpeed => _embedMinSpeed;
        public float DeflectFactor => _deflectFactor;
        public float RicochetMaxGrazingDeg => _ricochetMaxGrazingDeg;
        public float RicochetSpeedFactor => _ricochetSpeedFactor;
        public float ImpulseTransfer => _impulseTransfer;
        public float ArrowDamageMultiplier => _arrowDamageMultiplier;
        public int DebrisMedium => _debrisMedium;
        public float DebrisLifetime => _debrisLifetime;

        /// <summary>Shared runtime PhysicsMaterial built from this profile.</summary>
        public PhysicsMaterial PhysicsMaterial
        {
            get
            {
                if (_physicsMaterial == null)
                {
                    _physicsMaterial = new PhysicsMaterial("PM_" + _kind)
                    {
                        staticFriction = _staticFriction,
                        dynamicFriction = _dynamicFriction,
                        frictionCombine = _frictionCombine,
                        bounciness = _bounciness,
                        bounceCombine = PhysicsMaterialCombine.Minimum
                    };
                }
                return _physicsMaterial;
            }
        }

        /// <summary>Tooling: sets every value at once (used by the content generator).</summary>
        public void Configure(MaterialKind kind, Color color, float massPerCell, float staticFriction, float dynamicFriction,
            PhysicsMaterialCombine frictionCombine, float bounciness, bool unbreakable, float maxHP, float damageThreshold,
            float collisionDamageScale, float breakImpulse, bool arrowEmbeds, float embedMaxIncidenceDeg, float embedMinSpeed,
            float deflectFactor, float ricochetMaxGrazingDeg, float ricochetSpeedFactor, float impulseTransfer,
            float arrowDamageMultiplier, int debrisMedium, float debrisLifetime)
        {
            _kind = kind; _color = color; _massPerCell = massPerCell; _staticFriction = staticFriction;
            _dynamicFriction = dynamicFriction; _frictionCombine = frictionCombine; _bounciness = bounciness;
            _unbreakable = unbreakable; _maxHP = maxHP; _damageThreshold = damageThreshold;
            _collisionDamageScale = collisionDamageScale; _breakImpulse = breakImpulse; _arrowEmbeds = arrowEmbeds;
            _embedMaxIncidenceDeg = embedMaxIncidenceDeg; _embedMinSpeed = embedMinSpeed; _deflectFactor = deflectFactor;
            _ricochetMaxGrazingDeg = ricochetMaxGrazingDeg; _ricochetSpeedFactor = ricochetSpeedFactor;
            _impulseTransfer = impulseTransfer; _arrowDamageMultiplier = arrowDamageMultiplier;
            _debrisMedium = debrisMedium; _debrisLifetime = debrisLifetime;
            _physicsMaterial = null;
        }

        private void OnValidate() => _physicsMaterial = null;
    }
}
