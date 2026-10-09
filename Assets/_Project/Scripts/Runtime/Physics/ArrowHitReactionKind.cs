namespace ArrowBuster
{
    /// <summary>How an <see cref="IArrowHittable"/> wants the arrow to react (02 §6.2 rows 1–6).</summary>
    public enum ArrowHitReactionKind
    {
        /// <summary>Fall through to the material rule table (02 §6.3).</summary>
        UseMaterial = 0,
        PassThrough = 1,
        Stop = 2,
        Shatter = 3
    }
}
