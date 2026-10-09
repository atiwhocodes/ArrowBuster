namespace ArrowBuster
{
    /// <summary>Arrow impact outcomes (02 §6.1).</summary>
    public enum ImpactKind
    {
        Embed = 0,
        Deflect = 1,
        Stop = 2,
        Ricochet = 3,
        PassThrough = 4,
        Shatter = 5,
        Teleport = 6,
        Ignore = 7
    }
}
