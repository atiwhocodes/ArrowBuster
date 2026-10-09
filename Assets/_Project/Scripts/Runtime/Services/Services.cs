using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Static service locator (D-029). Populated by <see cref="ServiceInstaller"/>; never null thanks to
    /// <see cref="NullServices"/>. Reset on SubsystemRegistration because domain reload is disabled.
    /// </summary>
    public static class Services
    {
        public static IAnalyticsService Analytics { get; private set; } = NullServices.Instance;
        public static ICrashReportingService Crash { get; private set; } = NullServices.Instance;
        public static IRemoteConfigService RemoteConfig { get; private set; } = NullServices.Instance;
        public static IAdsService Ads { get; private set; } = NullServices.Instance;
        public static IIapService Iap { get; private set; } = NullServices.Instance;
        public static IConsentService Consent { get; private set; } = NullServices.Instance;
        public static IHapticsService Haptics { get; private set; } = NullServices.Instance;
        public static IAudioService Audio { get; private set; } = NullServices.Instance;
        public static ISaveService Save { get; private set; }

        public static bool IsInstalled { get; private set; }

        public static void Install(IAnalyticsService analytics, ISaveService save, IHapticsService haptics, IAudioService audio)
        {
            Analytics = analytics ?? NullServices.Instance;
            Save = save;
            Haptics = haptics ?? NullServices.Instance;
            Audio = audio ?? NullServices.Instance;
            IsInstalled = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            Analytics = NullServices.Instance;
            Crash = NullServices.Instance;
            RemoteConfig = NullServices.Instance;
            Ads = NullServices.Instance;
            Iap = NullServices.Instance;
            Consent = NullServices.Instance;
            Haptics = NullServices.Instance;
            Audio = NullServices.Instance;
            Save = null;
            IsInstalled = false;
        }
    }
}
