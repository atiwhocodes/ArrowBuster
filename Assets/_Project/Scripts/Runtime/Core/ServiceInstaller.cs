using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Composition root (D-029). Installs services exactly once per play session, from Boot or — when a scene is
    /// played directly in the editor — from <see cref="GameBootstrap"/>. Vendor SDK adapters replace the debug
    /// implementations in M7 (D-090).
    /// </summary>
    public static class ServiceInstaller
    {
        public static void EnsureInstalled()
        {
            if (Services.IsInstalled && AppRoot.Instance != null) return;

            AppConfig config = AppConfig.Load();
            var save = new JsonSaveService();
            var haptics = new HapticsService();

            var root = new GameObject("AppRoot");
            var appRoot = root.AddComponent<AppRoot>();
            appRoot.Initialise(config, save, haptics);

            Services.Install(new DebugAnalyticsService(), save, haptics, appRoot.Audio);
            Log.Info(LogCat.Core, "Services installed.");
        }
    }
}
