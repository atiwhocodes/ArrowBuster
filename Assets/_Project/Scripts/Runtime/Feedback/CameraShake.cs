using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Micro camera shake (05 §9): offsets a child transform so framing math is unaffected. Position-only,
    /// ≤ 0.08 m, ≤ 0.25 s; disabled by Reduced Motion (D-057).
    /// </summary>
    public sealed class CameraShake : MonoBehaviour
    {
        private static CameraShake _instance;

        private float _amplitude;
        private float _duration;
        private float _elapsed;

        public static bool Disabled { get; set; }

        public static void Request(float amplitude, float duration)
        {
            if (_instance == null || Disabled) return;
            _instance._amplitude = Mathf.Min(0.08f, Mathf.Max(amplitude, _instance._amplitude * 0.5f));
            _instance._duration = Mathf.Min(0.25f, duration);
            _instance._elapsed = 0f;
        }

        private void OnEnable() => _instance = this;

        private void OnDisable()
        {
            if (_instance == this) _instance = null;
            transform.localPosition = Vector3.zero;
        }

        private void LateUpdate()
        {
            if (_elapsed >= _duration)
            {
                transform.localPosition = Vector3.zero;
                return;
            }
            _elapsed += Time.unscaledDeltaTime;
            float fade = 1f - Mathf.Clamp01(_elapsed / _duration);
            Vector3 offset = CosmeticRandom.InsideUnitSphere() * _amplitude * fade;
            offset.z = 0f;
            transform.localPosition = offset;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            Disabled = false;
        }
    }
}
