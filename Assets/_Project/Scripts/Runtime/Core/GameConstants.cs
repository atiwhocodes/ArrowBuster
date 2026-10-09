namespace ArrowBuster
{
    /// <summary>Tuning targets from GDD §3 and §12. Change here, not as magic numbers in systems.</summary>
    public static class GameConstants
    {
        public const int TargetFrameRate = 60;
        public const int LowEndFrameRate = 30;

        public const float WinSettleSeconds = 0.75f;
        public const float SoftLockSettleSeconds = 2f;

        public const int MaxActiveArrows = 8;
        public const int MaxDebrisFragments = 40;

        public const float MinArrowFlightSeconds = 1.5f;
        public const float MaxArrowFlightSeconds = 2.5f;

        public const float HitStopMinSeconds = 0.04f;
        public const float HitStopMaxSeconds = 0.07f;
        public const float LodgedArrowVibrateSeconds = 0.2f;

        /// <summary>Gameplay happens on this Z plane (2.5D: 3D rigidbodies constrained to the plane).</summary>
        public const float PlayPlaneZ = 0f;

        public const int LevelsPerWorld = 20;
        public const int WorldCount = 3;
        public const int LevelsToUnlockNextWorld = 15;
    }
}
