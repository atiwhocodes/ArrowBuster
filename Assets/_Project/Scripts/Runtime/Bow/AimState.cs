using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Aim snapshot from the bow (02 §2.2).</summary>
    public readonly struct AimState
    {
        public readonly float AngleDeg;
        public readonly float Power01;
        public readonly bool IsFireable;
        public readonly bool IsFullDraw;
        public readonly float DragPixels;

        public AimState(float angleDeg, float power01, bool isFireable, bool isFullDraw, float dragPixels)
        {
            AngleDeg = angleDeg;
            Power01 = power01;
            IsFireable = isFireable;
            IsFullDraw = isFullDraw;
            DragPixels = dragPixels;
        }

        /// <summary>Unit aim direction in the XY play plane.</summary>
        public Vector3 Direction
        {
            get
            {
                float rad = AngleDeg * Mathf.Deg2Rad;
                return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            }
        }
    }
}
