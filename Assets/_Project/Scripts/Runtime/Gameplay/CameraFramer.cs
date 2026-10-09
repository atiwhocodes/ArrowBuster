using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Fixed 3/4 camera (D-041): low-FOV perspective, pitched slightly down, that always shows the full 10 m play
    /// width on 9:16–9:21 screens (extra height shows sky/ground) and fits the height on wider screens (tablets).
    /// </summary>
    public sealed class CameraFramer : MonoBehaviour
    {
        [SerializeField, Range(15f, 60f)] private float _verticalFov = 30f;
        [SerializeField, Range(0f, 20f)] private float _pitchDeg = 8f;
        [SerializeField, Min(0f)] private float _sideMargin = 0.35f;
        [Tooltip("World y that should sit at the bottom edge of the screen.")]
        [SerializeField] private float _bottomY = -0.6f;

        private Camera _camera;
        private float _extraHeight;
        private int _lastWidth;
        private int _lastHeight;

        /// <summary>World rectangle visible on the play plane (approximate, ignores pitch).</summary>
        public Rect VisibleRect { get; private set; } = new Rect(-5f, 0f, 10f, 18f);

        /// <summary>The framed camera (a child, so <see cref="CameraShake"/> can offset it locally).</summary>
        public Camera Camera => _camera;

        private void Awake()
        {
            _camera = GetComponentInChildren<Camera>();
            Apply();
        }

        public void Frame(LevelLayout layout)
        {
            _extraHeight = layout != null ? layout.ExtraHeight : 0f;
            Apply();
        }

        private void LateUpdate()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight) Apply();
        }

        /// <summary>Pure framing math (unit-tested): distance and visible height for an aspect ratio.</summary>
        public static void Compute(float aspect, float verticalFov, float halfWidth, float minHeight, out float distance, out float height)
        {
            float tanV = Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad);
            float tanH = tanV * aspect;
            distance = halfWidth / tanH;
            height = 2f * distance * tanV;
            if (height < minHeight)
            {
                distance = minHeight * 0.5f / tanV;
                height = minHeight;
            }
        }

        private void Apply()
        {
            if (_camera == null) _camera = GetComponentInChildren<Camera>();
            if (_camera == null) return;
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            float aspect = _lastHeight > 0 ? (float)_lastWidth / _lastHeight : 9f / 16f;

            Compute(aspect, _verticalFov, GameConstants.PlayAreaWidth * 0.5f + _sideMargin,
                GameConstants.PlayAreaHeight + _extraHeight, out float distance, out float height);

            _camera.fieldOfView = _verticalFov;
            _camera.nearClipPlane = 0.3f;
            _camera.farClipPlane = distance + 80f;
            float centerY = _bottomY + height * 0.5f;
            Vector3 target = new Vector3(0f, centerY, 0f);
            Quaternion rotation = Quaternion.Euler(_pitchDeg, 0f, 0f);
            transform.SetPositionAndRotation(target + rotation * new Vector3(0f, 0f, -distance), rotation);
            float width = height * aspect;
            VisibleRect = new Rect(-width * 0.5f, _bottomY, width, height);
        }
    }
}
