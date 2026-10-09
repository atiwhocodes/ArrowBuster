using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Per-arrow-type tuning and behaviour selection (02 §15). Assets: ScriptableObjects/Arrows/AD_&lt;Type&gt;.</summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Arrow Definition", fileName = "AD_Oak")]
    public sealed class ArrowDefinition : ScriptableObject
    {
        [SerializeField] private ArrowType _type = ArrowType.Oak;
        [SerializeField] private ArrowProjectile _prefab;
        [SerializeField] private Color _hudColor = new Color(0.96f, 0.88f, 0.7f);

        [Header("Flight")]
        [SerializeField, Min(0.1f)] private float _minSpeed = 4f;
        [SerializeField, Min(0.1f)] private float _maxSpeed = 13f;
        [SerializeField, Min(0f)] private float _gravityScale = 1f;
        [SerializeField, Min(0f)] private float _windResponse = 1f;

        [Header("Impact (02 §6.4)")]
        [Tooltip("Hit strength (Ns) at the reference speed.")]
        [SerializeField, Min(0f)] private float _impactImpulse = 6f;
        [SerializeField, Min(0f)] private float _maxImpulse = 10f;
        [SerializeField, Min(0.1f)] private float _referenceSpeed = 10f;
        [SerializeField, Min(0f)] private float _damageScale = 1f;
        [SerializeField, Range(0, 3)] private int _maxRicochets = 2;

        public ArrowType Type => _type;
        public ArrowProjectile Prefab => _prefab;
        public Color HudColor => _hudColor;
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;
        public float GravityScale => _gravityScale;
        public float WindResponse => _windResponse;
        public float ImpactImpulse => _impactImpulse;
        public float MaxImpulse => _maxImpulse;
        public float ReferenceSpeed => _referenceSpeed;
        public float DamageScale => _damageScale;
        public int MaxRicochets => _maxRicochets;

        /// <summary>Launch velocity for an aim (02 §2.2): direction × lerp(minSpeed, maxSpeed, power).</summary>
        public Vector3 LaunchVelocity(AimState aim) => aim.Direction * Mathf.Lerp(_minSpeed, _maxSpeed, aim.Power01);

        /// <summary>Tooling/tests.</summary>
        public void Configure(ArrowType type, ArrowProjectile prefab, float minSpeed, float maxSpeed, float gravityScale,
            float windResponse, float impactImpulse, float maxImpulse, float damageScale, int maxRicochets)
        {
            _type = type;
            _prefab = prefab;
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;
            _gravityScale = gravityScale;
            _windResponse = windResponse;
            _impactImpulse = impactImpulse;
            _maxImpulse = maxImpulse;
            _damageScale = damageScale;
            _maxRicochets = maxRicochets;
        }
    }
}
