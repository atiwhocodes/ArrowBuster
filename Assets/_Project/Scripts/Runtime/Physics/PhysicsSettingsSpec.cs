namespace ArrowBuster
{
    /// <summary>
    /// Project-wide physics and time settings from D-006. Applied by <c>PhysicsSetup</c> (editor) and
    /// verified by <c>PhysicsSettingsTests</c>; keep the two in sync through this class only.
    /// </summary>
    public static class PhysicsSettingsSpec
    {
        public const float FixedDeltaTime = 1f / 60f;
        public const float MaximumDeltaTime = 0.1f;
        public const int SolverIterations = 8;
        public const int SolverVelocityIterations = 2;
        public const bool EnhancedDeterminism = true;
        public const float BounceThreshold = 1f;
        public const float SleepThreshold = 0.005f;
        public const float DefaultMaxAngularSpeed = 25f;
        /// <summary>Clamps depenetration so overlapping stacks never explode on spawn (D-006).</summary>
        public const float DefaultMaxDepenetrationVelocity = 3f;
        public const bool AutoSyncTransforms = false;
    }
}
