namespace ArrowBuster
{
    // Enum values are append-only (D-070): never renumber or remove members; they are serialized as integers.

    /// <summary>Arrow types from mvp.md §5. Drill is post-MVP and intentionally absent.</summary>
    public enum ArrowType
    {
        Oak = 0,
        Heavyhead = 1,
        Split = 2,
        Fire = 3,
        Bounce = 4
    }

    /// <summary>Structural materials from mvp.md §4, plus environment ground (D-059).</summary>
    public enum MaterialKind
    {
        Straw = 0,
        Timber = 1,
        Stone = 2,
        Ice = 3,
        Metal = 4,
        Earth = 5
    }

    /// <summary>Required objective object types from mvp.md §4.</summary>
    public enum ObjectiveKind
    {
        CrestTarget = 0,
        SupplyCrate = 1,
        HangingLantern = 2,
        CursedOrb = 3,
        TrainingDummy = 4,
        BannerRope = 5
    }

    /// <summary>Protected objects: breaking / losing one fails the level (mvp.md §4).</summary>
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

    /// <summary>Why a level failed (D-015, D-038).</summary>
    public enum FailReason
    {
        None = 0,
        OutOfArrows = 1,
        ProtectedLost = 2
    }

    /// <summary>Why a protected object was lost (D-038).</summary>
    public enum ProtectedLossReason
    {
        Broken = 0,
        Hit = 1,
        Displaced = 2,
        KillZone = 3
    }

    /// <summary>Source of damage applied to a <c>Breakable</c>.</summary>
    public enum DamageSource
    {
        Arrow = 0,
        Collision = 1,
        Explosion = 2,
        Fire = 3,
        KillZone = 4
    }

    /// <summary>Kill-zone flavours (D-039).</summary>
    public enum KillZoneKind
    {
        Water = 0,
        Spikes = 1,
        Pit = 2
    }

    /// <summary>Interactive-prop events reported to analytics as <c>object_triggered</c>.</summary>
    public enum PropTriggerKind
    {
        RopeCut = 0,
        BalloonPop = 1,
        BarrelBlast = 2,
        OilIgnite = 3,
        PortalEnter = 4,
        BoulderRelease = 5
    }

    /// <summary>How a fired arrow finished (02 §5).</summary>
    public enum ArrowResolution
    {
        Embedded = 0,
        Spent = 1,
        OutOfBounds = 2,
        KillZone = 3
    }

    /// <summary>What kind of gameplay body a collider belongs to (02 §6.1).</summary>
    public enum BodyKind
    {
        Unknown = 0,
        Environment = 1,
        Structure = 2,
        Objective = 3,
        Protected = 4,
        Prop = 5,
        Rope = 6,
        Portal = 7
    }
}
