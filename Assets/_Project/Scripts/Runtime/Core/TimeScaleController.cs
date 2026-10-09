using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// The only writer of <see cref="Time.timeScale"/> (D-061). Priority: pause &gt; slow-mo &gt; hit-stop.
    /// Timers run on unscaled time; <see cref="Tick"/> is driven by <see cref="AppRoot"/>.
    /// </summary>
    public static class TimeScaleController
    {
        private static bool _paused;
        private static float _slowMoScale = 1f;
        private static float _slowMoUntil;
        private static float _hitStopScale = 1f;
        private static float _hitStopUntil;

        public static bool IsPaused => _paused;

        public static void SetPaused(bool paused)
        {
            _paused = paused;
            Apply(Time.unscaledTime);
        }

        public static void RequestSlowMo(float scale, float seconds)
        {
            _slowMoScale = Mathf.Clamp(scale, 0.01f, 1f);
            _slowMoUntil = Time.unscaledTime + seconds;
            Apply(Time.unscaledTime);
        }

        public static void RequestHitStop(float seconds, float scale)
        {
            float until = Time.unscaledTime + seconds;
            if (until <= _hitStopUntil) return; // never stacks
            _hitStopScale = Mathf.Clamp(scale, 0.01f, 1f);
            _hitStopUntil = until;
            Apply(Time.unscaledTime);
        }

        /// <summary>Clears every request (level load, scene change).</summary>
        public static void ClearAll()
        {
            _paused = false;
            _slowMoUntil = 0f;
            _hitStopUntil = 0f;
            Time.timeScale = 1f;
        }

        public static void Tick(float unscaledNow) => Apply(unscaledNow);

        /// <summary>Pure resolution of the current scale (unit-tested).</summary>
        public static float Resolve(bool paused, float now, float slowScale, float slowUntil, float stopScale, float stopUntil)
        {
            if (paused) return 0f;
            if (now < slowUntil) return slowScale;
            if (now < stopUntil) return stopScale;
            return 1f;
        }

        private static void Apply(float now)
        {
            float scale = Resolve(_paused, now, _slowMoScale, _slowMoUntil, _hitStopScale, _hitStopUntil);
            if (!Mathf.Approximately(Time.timeScale, scale)) Time.timeScale = scale;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _paused = false;
            _slowMoScale = 1f;
            _slowMoUntil = 0f;
            _hitStopScale = 1f;
            _hitStopUntil = 0f;
        }
    }
}
