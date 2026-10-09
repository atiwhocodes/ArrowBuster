using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Cross-cutting gameplay event bus (D-030, 01 §10.2). Gameplay raises; Feedback, UI, Analytics and Tutorial
    /// listen. Gameplay code never calls audio/VFX/haptics/analytics directly.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<LevelSessionInfo> LevelStarted;
        public static event Action<AimState> DrawStarted;
        public static event Action<AimState> DrawCancelled;
        public static event Action<AimState> DrawThresholdReached;
        public static event Action<ArrowFiredInfo> ArrowFired;
        public static event Action<ArrowImpactInfo> ArrowImpact;
        public static event Action<ArrowResolvedInfo> ArrowResolved;
        public static event Action<BreakInfo> ObjectBroken;
        public static event Action<PropTriggerInfo> PropTriggered;
        public static event Action<ObjectiveInfo> ObjectiveCleared;
        public static event Action<ProtectedInfo> ProtectedLost;
        public static event Action<int> SoftLockPrompt;
        public static event Action<GameplayState> GameplayStateChanged;
        public static event Action<LevelResultInfo> LevelWon;
        public static event Action<LevelResultInfo> LevelFailed;
        public static event Action<LevelSessionInfo> LevelRestarted;
        public static event Action<LevelSessionInfo> LevelQuit;

        public static void RaiseLevelStarted(LevelSessionInfo e) => LevelStarted?.Invoke(e);
        public static void RaiseDrawStarted(AimState e) => DrawStarted?.Invoke(e);
        public static void RaiseDrawCancelled(AimState e) => DrawCancelled?.Invoke(e);
        public static void RaiseDrawThresholdReached(AimState e) => DrawThresholdReached?.Invoke(e);
        public static void RaiseArrowFired(ArrowFiredInfo e) => ArrowFired?.Invoke(e);
        public static void RaiseArrowImpact(ArrowImpactInfo e) => ArrowImpact?.Invoke(e);
        public static void RaiseArrowResolved(ArrowResolvedInfo e) => ArrowResolved?.Invoke(e);
        public static void RaiseObjectBroken(BreakInfo e) => ObjectBroken?.Invoke(e);
        public static void RaisePropTriggered(PropTriggerInfo e) => PropTriggered?.Invoke(e);
        public static void RaiseObjectiveCleared(ObjectiveInfo e) => ObjectiveCleared?.Invoke(e);
        public static void RaiseProtectedLost(ProtectedInfo e) => ProtectedLost?.Invoke(e);
        public static void RaiseSoftLockPrompt(int arrowsRemaining) => SoftLockPrompt?.Invoke(arrowsRemaining);
        public static void RaiseGameplayStateChanged(GameplayState e) => GameplayStateChanged?.Invoke(e);
        public static void RaiseLevelWon(LevelResultInfo e) => LevelWon?.Invoke(e);
        public static void RaiseLevelFailed(LevelResultInfo e) => LevelFailed?.Invoke(e);
        public static void RaiseLevelRestarted(LevelSessionInfo e) => LevelRestarted?.Invoke(e);
        public static void RaiseLevelQuit(LevelSessionInfo e) => LevelQuit?.Invoke(e);

        /// <summary>Removes every subscriber. Enter Play Mode runs without domain reload, so this must run first.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStatics()
        {
            LevelStarted = null;
            DrawStarted = null;
            DrawCancelled = null;
            DrawThresholdReached = null;
            ArrowFired = null;
            ArrowImpact = null;
            ArrowResolved = null;
            ObjectBroken = null;
            PropTriggered = null;
            ObjectiveCleared = null;
            ProtectedLost = null;
            SoftLockPrompt = null;
            GameplayStateChanged = null;
            LevelWon = null;
            LevelFailed = null;
            LevelRestarted = null;
            LevelQuit = null;
        }
    }
}
