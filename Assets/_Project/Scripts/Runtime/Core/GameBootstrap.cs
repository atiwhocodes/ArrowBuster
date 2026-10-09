using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Global runtime defaults applied before the first scene loads; installs services (01 §4).</summary>
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Application.targetFrameRate = GameConstants.TargetFrameRate;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            TimeScaleController.ClearAll();
            ServiceInstaller.EnsureInstalled();
        }
    }
}
