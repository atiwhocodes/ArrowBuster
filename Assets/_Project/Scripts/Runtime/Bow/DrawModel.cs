using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Pure drag-to-aim mapping (02 §2.2, D-018): pull back from the press point to shoot forward. Screen-space so
    /// the gesture feels the same on every aspect ratio. No smoothing (aim must be reproducible).
    /// </summary>
    public sealed class DrawModel
    {
        private readonly float _fullDrawFraction;
        private readonly float _deadzoneFraction;
        private readonly float _minFirePower;
        private readonly float _minAngle;
        private readonly float _maxAngle;
        private readonly float _exponent;
        private readonly float _fullDrawPower;

        private Vector2 _origin;
        private float _lastAngle = 90f;

        public bool Active { get; private set; }
        public AimState Current { get; private set; }

        public DrawModel(GameplayTuning t)
            : this(t.FullDrawScreenFraction, t.DragDeadzoneScreenFraction, t.MinFirePower, t.MinAimAngleDeg,
                t.MaxAimAngleDeg, t.PowerCurveExponent, t.FullDrawHapticPower) { }

        public DrawModel(float fullDrawFraction, float deadzoneFraction, float minFirePower, float minAngle, float maxAngle,
            float exponent, float fullDrawPower)
        {
            _fullDrawFraction = fullDrawFraction;
            _deadzoneFraction = deadzoneFraction;
            _minFirePower = minFirePower;
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            _exponent = exponent;
            _fullDrawPower = fullDrawPower;
        }

        public void Begin(Vector2 pressPosition)
        {
            _origin = pressPosition;
            _lastAngle = 90f;
            Active = true;
            Current = new AimState(90f, 0f, false, false, 0f);
        }

        public AimState Update(Vector2 position, float screenHeight)
        {
            Vector2 pull = _origin - position;
            float length = pull.magnitude;
            if (length >= _deadzoneFraction * screenHeight)
            {
                float angle = Mathf.Atan2(pull.y, pull.x) * Mathf.Rad2Deg;
                if (angle < 0f) angle = pull.x >= 0f ? _minAngle : _maxAngle;
                _lastAngle = Mathf.Clamp(angle, _minAngle, _maxAngle);
            }
            float raw = Mathf.Clamp01(length / (_fullDrawFraction * screenHeight));
            float power = Mathf.Pow(raw, _exponent);
            Current = new AimState(_lastAngle, power, power >= _minFirePower, power >= _fullDrawPower, length);
            return Current;
        }

        public AimState End()
        {
            Active = false;
            return Current;
        }

        public void Cancel() => Active = false;
    }
}
