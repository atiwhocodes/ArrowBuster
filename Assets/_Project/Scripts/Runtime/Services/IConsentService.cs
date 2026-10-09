namespace ArrowBuster
{
    /// <summary>Consent (UMP + ATT in M7). Collection stays off until consent resolves where required (D-096).</summary>
    public interface IConsentService
    {
        bool AnalyticsAllowed { get; }
        bool CrashReportingAllowed { get; }
        bool PersonalisedAdsAllowed { get; }
    }
}
