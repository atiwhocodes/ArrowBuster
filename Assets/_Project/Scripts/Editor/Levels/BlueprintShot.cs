using System;

namespace ArrowBuster.Editor
{
    /// <summary>One recorded intended shot (aim angle in degrees, power 0..1, delay before firing) used by the solvability bot.</summary>
    [Serializable]
    public sealed class BlueprintShot
    {
        public float angle;
        public float power;
        public float delay;
    }
}
