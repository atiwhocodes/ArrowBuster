using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Maps <see cref="GameEvents"/> to the analytics event dictionary (06 §11, mvp.md §13) with the common
    /// parameters world_id, level_id, global_level, attempt_number, arrows_start, arrows_used, result, session_id.
    /// </summary>
    public sealed class AnalyticsBridge : MonoBehaviour
    {
        private LevelSessionInfo _session;
        private bool _firstShotLogged;
        private int _shotsThisAttempt;
        private string _sessionId;

        private void OnEnable()
        {
            _sessionId = System.Guid.NewGuid().ToString("N").Substring(0, 12);
            GameEvents.LevelStarted += OnLevelStarted;
            GameEvents.ArrowFired += OnArrowFired;
            GameEvents.PropTriggered += OnPropTriggered;
            GameEvents.LevelWon += OnLevelWon;
            GameEvents.LevelFailed += OnLevelFailed;
            GameEvents.LevelRestarted += OnLevelRestarted;
            GameEvents.LevelQuit += OnLevelQuit;
        }

        // Start runs after ServiceInstaller has installed the analytics sink.
        private void Start() => Track("session_start", new Dictionary<string, object> { ["session_id"] = _sessionId });

        private void OnDisable()
        {
            GameEvents.LevelStarted -= OnLevelStarted;
            GameEvents.ArrowFired -= OnArrowFired;
            GameEvents.PropTriggered -= OnPropTriggered;
            GameEvents.LevelWon -= OnLevelWon;
            GameEvents.LevelFailed -= OnLevelFailed;
            GameEvents.LevelRestarted -= OnLevelRestarted;
            GameEvents.LevelQuit -= OnLevelQuit;
        }

        private void OnLevelStarted(LevelSessionInfo info)
        {
            _session = info;
            _shotsThisAttempt = 0;
            Track("level_started", Common(info));
        }

        private void OnArrowFired(ArrowFiredInfo info)
        {
            _shotsThisAttempt++;
            if (!_firstShotLogged)
            {
                _firstShotLogged = true;
                var first = Common(_session);
                first["seconds_since_launch"] = Mathf.Round(Time.realtimeSinceStartup * 10f) / 10f;
                Track("first_shot", first);
            }
            var p = Common(_session);
            p["arrow_type"] = info.ArrowType.ToString().ToLowerInvariant();
            p["arrow_index"] = info.ArrowIndex;
            p["angle_deg"] = Mathf.Round(info.AngleDeg * 10f) / 10f;
            p["power"] = Mathf.Round(info.Power01 * 100f) / 100f;
            Track("arrow_fired", p);
        }

        private void OnPropTriggered(PropTriggerInfo info)
        {
            var p = Common(_session);
            p["object_type"] = info.Kind == PropTriggerKind.RopeCut ? "rope_cut" : info.Kind.ToString().ToLowerInvariant();
            Track("object_triggered", p);
        }

        private void OnLevelWon(LevelResultInfo info)
        {
            var p = Result(info, "win");
            p["stars"] = info.Stars;
            p["gold_par"] = info.GoldPar;
            Track("level_completed", p);
        }

        private void OnLevelFailed(LevelResultInfo info)
        {
            var p = Result(info, info.FailReason == FailReason.ProtectedLost ? "protected_lost" : "out_of_arrows");
            p["objectives_remaining"] = info.ObjectivesRemaining;
            Track("level_failed", p);
        }

        private void OnLevelRestarted(LevelSessionInfo info)
        {
            var p = Common(info);
            p["shots_this_attempt"] = _shotsThisAttempt;
            Track("level_restarted", p);
        }

        private void OnLevelQuit(LevelSessionInfo info)
        {
            var p = Common(info);
            p["before_first_shot"] = _shotsThisAttempt == 0;
            Track("level_quit", p);
        }

        private Dictionary<string, object> Result(LevelResultInfo info, string result)
        {
            var p = Common(info.Session);
            p["arrows_used"] = info.ArrowsUsed;
            p["result"] = result;
            p["duration_s"] = Mathf.Round(info.DurationSeconds * 10f) / 10f;
            p["bonus_arrow_used"] = info.BonusArrowUsed;
            return p;
        }

        private Dictionary<string, object> Common(LevelSessionInfo info) => new Dictionary<string, object>
        {
            ["world_id"] = info.WorldId,
            ["level_id"] = info.LevelId,
            ["global_level"] = info.GlobalLevel,
            ["attempt_number"] = info.Attempt,
            ["arrows_start"] = info.ArrowsStart,
            ["session_id"] = _sessionId
        };

        private static void Track(string name, Dictionary<string, object> parameters) => Services.Analytics.Track(name, parameters);
    }
}
