using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Fixed-step time since level start (02 §0). Advances only in FixedUpdate, so hit-stop and pause never
    /// change gameplay outcomes. Reset on every level load.
    /// </summary>
    public static class LevelClock
    {
        /// <summary>Seconds of simulated gameplay since the level loaded.</summary>
        public static float Now { get; private set; }

        /// <summary>Number of fixed steps since the level loaded.</summary>
        public static int Steps { get; private set; }

        public static void Reset()
        {
            Now = 0f;
            Steps = 0;
        }

        public static void Tick(float dt)
        {
            Now += dt;
            Steps++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Reset();
    }
}
