using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Purely visual bow (02 §2.3): rotates toward the aim, pulls the string back with power, glows at full draw,
    /// snaps on release and shows the next nocked arrow. No gameplay data flows back from here.
    /// </summary>
    public sealed class BowView : MonoBehaviour
    {
        private const float MaxPull = 0.35f;
        private const float MaxVisualRotation = 82f;
        private const float SnapSeconds = 0.12f;

        [SerializeField] private Transform _rotator;
        [SerializeField] private Transform _nock;
        [SerializeField] private Transform _topTip;
        [SerializeField] private Transform _bottomTip;
        [SerializeField] private LineRenderer _string;
        [SerializeField] private GameObject _nockedArrow;
        [SerializeField] private Color _stringColor = new Color(0.95f, 0.92f, 0.82f);
        [SerializeField] private Color _stringGlow = new Color(1f, 0.85f, 0.35f);

        private Vector3 _nockRest;
        private float _pull;
        private float _snapStart = -1f;
        private float _snapFrom;
        private bool _glow;

        private void Awake()
        {
            if (_nock != null) _nockRest = _nock.localPosition;
            if (_string != null) _string.positionCount = 3;
        }

        public void ShowAim(AimState aim)
        {
            if (_rotator != null)
                _rotator.localRotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(aim.AngleDeg - 90f, -MaxVisualRotation, MaxVisualRotation));
            _snapStart = -1f;
            _pull = aim.Power01 * MaxPull;
            _glow = aim.IsFullDraw;
        }

        public void Relax()
        {
            _snapFrom = _pull;
            _snapStart = Time.unscaledTime;
            _glow = false;
        }

        public void PlayRelease()
        {
            Relax();
            if (_nockedArrow != null) _nockedArrow.SetActive(false);
        }

        public void SetNocked(ArrowDefinition next)
        {
            if (_nockedArrow != null) _nockedArrow.SetActive(next != null);
        }

        private void LateUpdate()
        {
            if (_snapStart >= 0f)
            {
                float t = (Time.unscaledTime - _snapStart) / SnapSeconds;
                // Overshoot past rest, then settle (release "snap").
                _pull = t >= 1f ? 0f : _snapFrom * (1f - t) - Mathf.Sin(t * Mathf.PI) * 0.06f;
                if (t >= 1f) _snapStart = -1f;
            }
            if (_nock != null) _nock.localPosition = _nockRest + Vector3.down * _pull;

            if (_string == null || _topTip == null || _bottomTip == null || _nock == null) return;
            _string.SetPosition(0, _topTip.position);
            _string.SetPosition(1, _nock.position);
            _string.SetPosition(2, _bottomTip.position);
            Color c = _glow ? _stringGlow : _stringColor;
            _string.startColor = c;
            _string.endColor = c;
            _string.widthMultiplier = _glow ? 0.05f : 0.03f;
        }
    }
}
