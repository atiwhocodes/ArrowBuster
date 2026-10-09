using System.Collections.Generic;

namespace ArrowBuster
{
    /// <summary>Analytics sink (06 §11). Vendor adapter (Firebase) arrives in M7 behind this interface (D-090).</summary>
    public interface IAnalyticsService
    {
        /// <summary>Records one event. Implementations must respect consent (D-096).</summary>
        void Track(string eventName, IReadOnlyDictionary<string, object> parameters);
    }
}
