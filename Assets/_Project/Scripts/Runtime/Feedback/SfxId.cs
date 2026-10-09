namespace ArrowBuster
{
    /// <summary>Sound events (05 §10.2). Append-only (D-070): values are serialized in <see cref="SoundLibrary"/>.</summary>
    public enum SfxId
    {
        None = 0,
        BowDraw = 1,
        BowFullDraw = 2,
        BowRelease = 3,
        DrawCancel = 4,
        ArrowWhoosh = 5,
        ImpactTimber = 6,
        ImpactStraw = 7,
        ImpactStone = 8,
        ImpactIce = 9,
        ImpactMetal = 10,
        ImpactEarth = 11,
        EmbedWood = 12,
        BreakTimber = 13,
        BreakStraw = 14,
        BreakStone = 15,
        BreakIce = 16,
        BreakCrest = 17,
        BreakVase = 18,
        RopeCut = 19,
        Splash = 20,
        ObjectiveCleared = 21,
        ChainHit = 22,
        WinSting = 23,
        FailSting = 24,
        ProtectedAlarm = 25,
        UiTap = 26,
        UiStar = 27,
        BodyThud = 28
    }
}
