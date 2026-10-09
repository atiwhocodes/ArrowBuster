using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>One recorded shot of a level's intended solution (04 §2, solvability bot input).</summary>
    [Serializable]
    public struct IntendedShot
    {
        [Range(0f, 180f)] public float angleDeg;
        [Range(0f, 1f)] public float power01;
        [Tooltip("Seconds to wait after the previous shot (level clock).")]
        [Min(0f)] public float delayAfterPrevious;

        public IntendedShot(float angleDeg, float power01, float delayAfterPrevious)
        {
            this.angleDeg = angleDeg;
            this.power01 = power01;
            this.delayAfterPrevious = delayAfterPrevious;
        }
    }
}
