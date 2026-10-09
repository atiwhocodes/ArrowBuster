namespace ArrowBuster
{
    /// <summary>Reaction returned by <see cref="IArrowHittable.Evaluate"/>; pure, also used by the preview.</summary>
    public readonly struct ArrowHitReaction
    {
        public readonly ArrowHitReactionKind Kind;
        public readonly float SpeedFactor;
        public readonly bool Meaningful;

        public ArrowHitReaction(ArrowHitReactionKind kind, float speedFactor = 1f, bool meaningful = false)
        {
            Kind = kind;
            SpeedFactor = speedFactor;
            Meaningful = meaningful;
        }

        public static ArrowHitReaction UseMaterial => new ArrowHitReaction(ArrowHitReactionKind.UseMaterial);
    }
}
