using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Persistent root (01 §4): owns audio, VFX, feedback and the time-scale tick. Created once by
    /// <see cref="ServiceInstaller"/> and kept across scenes.
    /// </summary>
    public sealed class AppRoot : MonoBehaviour
    {
        public static AppRoot Instance { get; private set; }

        public AudioService Audio { get; private set; }
        public VfxService Vfx { get; private set; }
        public FeedbackDirector Feedback { get; private set; }

        internal void Initialise(AppConfig config, ISaveService save, IHapticsService haptics)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Audio = gameObject.AddComponent<AudioService>();
            if (config != null) Audio.SetLibrary(config.SoundLibrary);

            var vfxRoot = new GameObject("Vfx");
            vfxRoot.transform.SetParent(transform, false);
            Vfx = vfxRoot.AddComponent<VfxService>();

            Feedback = gameObject.AddComponent<FeedbackDirector>();
            gameObject.AddComponent<AnalyticsBridge>();

            var debrisRoot = new GameObject("Debris");
            debrisRoot.transform.SetParent(transform, false);
            debrisRoot.AddComponent<DebrisPool>();

            SettingsData settings = save?.Data.settings;
            if (settings != null)
            {
                Audio.SfxVolume = settings.sfx ? 1f : 0f;
                Audio.MusicVolume = settings.music ? 0.55f : 0f;
                haptics.Enabled = settings.haptics;
            }
        }

        private void Update() => TimeScaleController.Tick(Time.unscaledTime);

        private void OnApplicationPause(bool paused)
        {
            if (paused) Services.Save?.Save();
        }

        private void OnApplicationQuit() => Services.Save?.Save();

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;
    }
}
