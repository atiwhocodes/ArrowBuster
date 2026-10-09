using System;

namespace ArrowBuster
{
    /// <summary>Per-level save row (06 §10).</summary>
    [Serializable]
    public sealed class LevelProgress
    {
        public string levelId;
        public bool cleared;
        public int bestStars;
        public int bestArrowsUsed;
        public int attempts;
        public bool bullseye;
    }
}
