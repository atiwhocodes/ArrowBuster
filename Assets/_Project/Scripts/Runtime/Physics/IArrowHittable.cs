namespace ArrowBuster
{
    /// <summary>
    /// Contract that Props/Objectives implement so Arrows and Physics never reference their types (01 §10.3).
    /// <see cref="Evaluate"/> must be pure (the trajectory preview calls it); <see cref="Apply"/> runs side effects.
    /// </summary>
    public interface IArrowHittable
    {
        ArrowHitReaction Evaluate(in ArrowHitInfo hit);
        void Apply(in ArrowHitInfo hit);
    }
}
