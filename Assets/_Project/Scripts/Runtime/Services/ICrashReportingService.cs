namespace ArrowBuster
{
    /// <summary>Crash/non-fatal reporting (D-090: Firebase Crashlytics in M7, consent-gated per D-096).</summary>
    public interface ICrashReportingService
    {
        void RecordNonFatal(string message);
        void Breadcrumb(string message);
    }
}
