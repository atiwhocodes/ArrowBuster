using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Global gameplay feel tuning (02 §0.1). One asset: ScriptableObjects/Config/GameplayTuning.asset (CORE-owned).
    /// Two presets exist until the M1 feel gate (D-036): "Spec" and "Snappy".
    /// </summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Gameplay Tuning", fileName = "GameplayTuning")]
    public sealed class GameplayTuning : ScriptableObject
    {
        [Header("Draw (D-018)")]
        [SerializeField, Range(0.2f, 1f)] private float _aimZoneScreenFraction = 0.55f;
        [SerializeField, Range(0.05f, 0.5f)] private float _fullDrawScreenFraction = 0.22f;
        [SerializeField, Range(0f, 0.1f)] private float _dragDeadzoneScreenFraction = 0.015f;
        [SerializeField, Range(0f, 0.5f)] private float _minFirePower = 0.15f;
        [SerializeField, Range(0f, 90f)] private float _minAimAngleDeg = 8f;
        [SerializeField, Range(90f, 180f)] private float _maxAimAngleDeg = 172f;
        [SerializeField, Range(0.5f, 2f)] private float _powerCurveExponent = 1f;
        [SerializeField, Range(0.5f, 1f)] private float _fullDrawHapticPower = 0.98f;

        [Header("Flow (D-083)")]
        [SerializeField, Min(0f)] private float _renockCooldown = 0.35f;
        [SerializeField, Min(0f)] private float _introDurationSeconds = 0.6f;
        [SerializeField, Min(0f)] private float _outOfArrowsToastSeconds = 0.6f;
        [SerializeField, Min(0f)] private float _protectedFocusSeconds = 0.5f;
        [SerializeField, Range(0.05f, 1f)] private float _protectedFocusTimeScale = 0.3f;

        [Header("Flight (D-005, D-036)")]
        [SerializeField, Min(0.1f)] private float _arrowGravity = 5f;
        [SerializeField, Min(0.01f)] private float _sweepRadius = 0.06f;
        [SerializeField, Min(1f)] private float _arrowMaxLifetime = 6f;
        [SerializeField, Min(0f)] private float _nockOffset = 0.45f;
        [SerializeField] private Vector3 _bowPivot = new Vector3(0f, 1.5f, 0f);
        [SerializeField, Min(0f)] private float _playBoundsMargin = 2f;

        [Header("Preview (D-085)")]
        [SerializeField, Min(0.1f)] private float _previewBaseSeconds = 1.6f;
        [SerializeField, Min(0.01f)] private float _previewDotSpacingSeconds = 0.05f;
        [SerializeField, Range(10, 300)] private int _previewMaxSteps = 150;

        [Header("Impacts and feedback")]
        [SerializeField, Min(0f)] private float _meaningfulImpulseThreshold = 3f;
        [SerializeField, Range(0.01f, 1f)] private float _hitStopTimeScale = 0.05f;
        [SerializeField, Min(0f)] private float _hitStopCooldown = 0.3f;
        [SerializeField, Min(0f)] private float _spentArrowFadeSeconds = 1.5f;

        [Header("Settle (D-015)")]
        [SerializeField, Min(0f)] private float _calmLinearSpeed = 0.05f;
        [SerializeField, Min(0f)] private float _calmAngularSpeedDeg = 5f;

        public float AimZoneScreenFraction => _aimZoneScreenFraction;
        public float FullDrawScreenFraction => _fullDrawScreenFraction;
        public float DragDeadzoneScreenFraction => _dragDeadzoneScreenFraction;
        public float MinFirePower => _minFirePower;
        public float MinAimAngleDeg => _minAimAngleDeg;
        public float MaxAimAngleDeg => _maxAimAngleDeg;
        public float PowerCurveExponent => _powerCurveExponent;
        public float FullDrawHapticPower => _fullDrawHapticPower;
        public float RenockCooldown => _renockCooldown;
        public float IntroDurationSeconds => _introDurationSeconds;
        public float OutOfArrowsToastSeconds => _outOfArrowsToastSeconds;
        public float ProtectedFocusSeconds => _protectedFocusSeconds;
        public float ProtectedFocusTimeScale => _protectedFocusTimeScale;
        public float ArrowGravity => _arrowGravity;
        public float SweepRadius => _sweepRadius;
        public float ArrowMaxLifetime => _arrowMaxLifetime;
        public float NockOffset => _nockOffset;
        public Vector3 BowPivot => _bowPivot;
        public float PlayBoundsMargin => _playBoundsMargin;
        public float PreviewBaseSeconds => _previewBaseSeconds;
        public float PreviewDotSpacingSeconds => _previewDotSpacingSeconds;
        public int PreviewMaxSteps => _previewMaxSteps;
        public float MeaningfulImpulseThreshold => _meaningfulImpulseThreshold;
        public float HitStopTimeScale => _hitStopTimeScale;
        public float HitStopCooldown => _hitStopCooldown;
        public float SpentArrowFadeSeconds => _spentArrowFadeSeconds;
        public float CalmLinearSpeed => _calmLinearSpeed;
        public float CalmAngularSpeedDeg => _calmAngularSpeedDeg;

        /// <summary>Tooling/tests: switch between the D-036 presets.</summary>
        public void ApplyPreset(bool snappy)
        {
            _arrowGravity = snappy ? 12f : 5f;
            _previewBaseSeconds = snappy ? 1.0f : 1.6f;
        }

        /// <summary>Tests: a default instance without an asset.</summary>
        public static GameplayTuning CreateDefault() => CreateInstance<GameplayTuning>();
    }
}
