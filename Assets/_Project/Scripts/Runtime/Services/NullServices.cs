using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Null-object service implementations so <see cref="Services"/> is never null (01 §9 rule 10).</summary>
    public sealed class NullServices : IAnalyticsService, ICrashReportingService, IRemoteConfigService, IAdsService,
        IIapService, IConsentService, IHapticsService, IAudioService
    {
        public static readonly NullServices Instance = new NullServices();

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters) { }
        public void RecordNonFatal(string message) { }
        public void Breadcrumb(string message) { }
        public int GetInt(string key, int fallback) => fallback;
        public float GetFloat(string key, float fallback) => fallback;
        public bool GetBool(string key, bool fallback) => fallback;
        public bool IsRewardedReady => false;
        public void ShowRewarded(string placement, Action<bool> onComplete) => onComplete?.Invoke(false);
        public void ShowInterstitial(string placement) { }
        public bool IsOwned(string productId) => false;
        public void Purchase(string productId, Action<bool> onComplete) => onComplete?.Invoke(false);
        public void Restore(Action<bool> onComplete) => onComplete?.Invoke(false);
        public bool AnalyticsAllowed => false;
        public bool CrashReportingAllowed => false;
        public bool PersonalisedAdsAllowed => false;
        public bool Enabled { get; set; }
        public void Play(HapticKind kind) { }
        public float SfxVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public void Play(SfxId id, float volumeScale = 1f, float pitchScale = 1f) { }
        public void PlayMusic(AudioClip clip) { }
    }
}
