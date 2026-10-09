using UnityEngine;

namespace ArrowBuster
{
    /// <summary>One pointer reading from <see cref="BowInputReader"/> (02 §2.1).</summary>
    public readonly struct PointerSample
    {
        public readonly PointerPhase Phase;
        public readonly Vector2 ScreenPosition;
        public readonly bool StartedOverUi;

        public PointerSample(PointerPhase phase, Vector2 screenPosition, bool startedOverUi)
        {
            Phase = phase;
            ScreenPosition = screenPosition;
            StartedOverUi = startedOverUi;
        }
    }
}
