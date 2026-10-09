using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ArrowBuster
{
    /// <summary>Log categories (01 §16).</summary>
    public enum LogCat
    {
        Core, Bow, Arrow, Physics, Level, Save, Ads, IAP, Analytics, UI, Audio
    }

    /// <summary>Logging wrapper. <see cref="Info"/> is stripped outside dev builds (AB_DEV / editor).</summary>
    public static class Log
    {
        [Conditional("AB_DEV"), Conditional("UNITY_EDITOR")]
        public static void Info(LogCat cat, string message) => Debug.Log($"[{cat}] {message}");

        public static void Warn(LogCat cat, string message) => Debug.LogWarning($"[{cat}] {message}");

        public static void Error(LogCat cat, string message) => Debug.LogError($"[{cat}] {message}");
    }
}
