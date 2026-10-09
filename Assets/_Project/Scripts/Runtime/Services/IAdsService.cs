using System;

namespace ArrowBuster
{
    /// <summary>Ads (Unity LevelPlay adapter in M7, D-090). Rules live in AdPolicy, never in the adapter.</summary>
    public interface IAdsService
    {
        bool IsRewardedReady { get; }
        void ShowRewarded(string placement, Action<bool> onComplete);
        void ShowInterstitial(string placement);
    }
}
