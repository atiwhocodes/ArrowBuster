using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Replays a level's recorded intended shots through the real game loop (07 §5.2 layer 2) and reports the
    /// result. Used by the dev overlay, by MCP-driven checks in the editor and by PlayMode solvability tests.
    /// </summary>
    public sealed class IntendedSolutionRunner : MonoBehaviour
    {
        public struct Report
        {
            public string LevelId;
            public bool Won;
            public int Stars;
            public int ArrowsUsed;
            public FailReason FailReason;
            public float SimSeconds;
        }

        private static IntendedSolutionRunner _instance;

        public static Report? Last { get; private set; }
        public static bool Running { get; private set; }

        /// <summary>Fires <paramref name="shots"/> (or the level's intended shots when null) on the current level.</summary>
        public static void Run(IReadOnlyList<IntendedShot> shots = null, bool restartFirst = true, float timeScale = 1f)
        {
            GameplayController controller = GameplayController.Instance;
            if (controller == null) return;
            if (_instance == null) _instance = new GameObject("IntendedSolutionRunner").AddComponent<IntendedSolutionRunner>();
            _instance.StopAllCoroutines();
            _instance.StartCoroutine(_instance.RunRoutine(controller, shots ?? controller.Level.IntendedShots, restartFirst, timeScale));
        }

        private IEnumerator RunRoutine(GameplayController controller, IReadOnlyList<IntendedShot> shots, bool restartFirst, float timeScale)
        {
            Running = true;
            Last = null;
            if (restartFirst) controller.Retry();
            LevelResultInfo? result = null;
            bool done = false;
            void OnWon(LevelResultInfo r) { result = r; done = true; }
            void OnFailed(LevelResultInfo r) { result = r; done = true; }
            GameEvents.LevelWon += OnWon;
            GameEvents.LevelFailed += OnFailed;
            Time.timeScale = timeScale;

            float lastShotAt = 0f;
            for (int i = 0; i < shots.Count && !done; i++)
            {
                while (!controller.CanDraw() && !done) yield return null;
                if (done) break;
                while (LevelClock.Now - lastShotAt < shots[i].delayAfterPrevious && !done) yield return new WaitForFixedUpdate();
                if (done) break;
                controller.FireExact(shots[i].angleDeg, shots[i].power01);
                lastShotAt = LevelClock.Now;
                yield return new WaitForFixedUpdate();
            }

            float timeout = Time.realtimeSinceStartup + 30f;
            while (!done && Time.realtimeSinceStartup < timeout) yield return null;
            GameEvents.LevelWon -= OnWon;
            GameEvents.LevelFailed -= OnFailed;
            Time.timeScale = 1f;

            Last = new Report
            {
                LevelId = controller.Level.LevelId,
                Won = result.HasValue && result.Value.FailReason == FailReason.None,
                Stars = result?.Stars ?? 0,
                ArrowsUsed = result?.ArrowsUsed ?? 0,
                FailReason = result?.FailReason ?? FailReason.None,
                SimSeconds = LevelClock.Now
            };
            Log.Info(LogCat.Level, $"[Bot] {Last.Value.LevelId} {(Last.Value.Won ? "WIN" : "FAIL " + Last.Value.FailReason)} " +
                                   $"stars {Last.Value.Stars} arrows {Last.Value.ArrowsUsed} sim {Last.Value.SimSeconds:0.0}s");
            Running = false;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            Last = null;
            Running = false;
        }
    }
}
