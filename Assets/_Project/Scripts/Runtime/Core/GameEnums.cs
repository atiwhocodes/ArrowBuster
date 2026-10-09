namespace ArrowBuster
{
    /// <summary>Arrow types from GDD §5. Drill is post-MVP and intentionally absent.</summary>
    public enum ArrowType
    {
        Oak = 0,
        Heavyhead = 1,
        Split = 2,
        Fire = 3,
        Bounce = 4
    }

    /// <summary>Structural materials from GDD §4.</summary>
    public enum MaterialKind
    {
        Straw = 0,
        Timber = 1,
        Stone = 2,
        Ice = 3,
        Metal = 4
    }

    /// <summary>Required objective object types from GDD §4.</summary>
    public enum ObjectiveKind
    {
        CrestTarget = 0,
        SupplyCrate = 1,
        HangingLantern = 2,
        CursedOrb = 3,
        TrainingDummy = 4,
        BannerRope = 5
    }

    /// <summary>Protected objects: breaking / losing one fails the level (GDD §4).</summary>
    public enum ProtectedKind
    {
        RoyalVase = 0,
        SleepingFox = 1,
        RoyalRelic = 2
    }

    public enum LevelResult
    {
        InProgress = 0,
        Won = 1,
        FailedOutOfArrows = 2,
        FailedProtectedLost = 3
    }
}
