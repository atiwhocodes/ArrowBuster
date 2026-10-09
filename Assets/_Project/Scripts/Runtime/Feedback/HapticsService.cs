using System.Runtime.InteropServices;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Thin native haptics bridge (D-032): iOS UIImpact/UINotificationFeedbackGenerator via Plugins/iOS/ABHaptics.mm,
    /// Android VibrationEffect via AndroidJavaObject, editor no-op. Rate-limited to one pulse per 50 ms.
    /// </summary>
    public sealed class HapticsService : IHapticsService
    {
        private const float MinInterval = 0.05f;
        private float _last = -1f;

        public bool Enabled { get; set; } = true;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ABHaptics_Play(int kind);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _vibrator;
        private int _sdkInt;
#endif

        public HapticsService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var version = new AndroidJavaClass("android.os.Build$VERSION")) _sdkInt = version.GetStatic<int>("SDK_INT");
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }
            catch (System.Exception e)
            {
                Log.Warn(LogCat.Core, "Haptics unavailable: " + e.Message);
            }
#endif
        }

        public void Play(HapticKind kind)
        {
            if (!Enabled) return;
            float now = Time.unscaledTime;
            if (now - _last < MinInterval) return;
            _last = now;

#if UNITY_IOS && !UNITY_EDITOR
            ABHaptics_Play((int)kind);
#elif UNITY_ANDROID && !UNITY_EDITOR
            PlayAndroid(kind);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void PlayAndroid(HapticKind kind)
        {
            if (_vibrator == null) return;
            long ms;
            int amplitude;
            switch (kind)
            {
                case HapticKind.Light: ms = 12; amplitude = 60; break;
                case HapticKind.Selection: ms = 8; amplitude = 40; break;
                case HapticKind.Medium: ms = 22; amplitude = 140; break;
                case HapticKind.Heavy: ms = 35; amplitude = 230; break;
                case HapticKind.Success: ms = 40; amplitude = 180; break;
                default: ms = 60; amplitude = 120; break;
            }
            try
            {
                if (_sdkInt >= 26)
                {
                    using (var effectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    using (var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude))
                        _vibrator.Call("vibrate", effect);
                }
                else
                {
                    _vibrator.Call("vibrate", ms);
                }
            }
            catch (System.Exception e)
            {
                Log.Warn(LogCat.Core, "Haptic failed: " + e.Message);
            }
        }
#endif
    }
}
