using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Dev analytics sink (AB-024): logs every event and appends it to a per-session CSV in persistentDataPath,
    /// so playtest sessions can be checked against the event dictionary (06 §11).
    /// </summary>
    public sealed class DebugAnalyticsService : IAnalyticsService
    {
        private readonly string _csvPath;
        private readonly StringBuilder _line = new StringBuilder(256);

        public string SessionId { get; }

        public DebugAnalyticsService()
        {
            SessionId = System.Guid.NewGuid().ToString("N").Substring(0, 12);
            _csvPath = System.IO.Path.Combine(Application.persistentDataPath, "analytics_" + SessionId + ".csv");
        }

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            _line.Clear();
            _line.Append(System.DateTime.UtcNow.ToString("o")).Append(',').Append(SessionId).Append(',').Append(eventName);
            if (parameters != null)
                foreach (var pair in parameters)
                    _line.Append(',').Append(pair.Key).Append('=').Append(pair.Value);

            Log.Info(LogCat.Analytics, _line.ToString());
            try
            {
                System.IO.File.AppendAllText(_csvPath, _line.Append('\n').ToString());
            }
            catch (System.Exception e)
            {
                Log.Warn(LogCat.Analytics, "CSV write failed: " + e.Message);
            }
        }
    }
}
