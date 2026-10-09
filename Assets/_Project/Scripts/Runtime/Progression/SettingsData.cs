using System;

namespace ArrowBuster
{
    /// <summary>Player settings (05 §16, 06 §10).</summary>
    [Serializable]
    public sealed class SettingsData
    {
        public bool music = true;
        public bool sfx = true;
        public bool haptics = true;
        public bool reducedParticles;
        public bool reducedMotion;
        public bool colorAssist;
    }
}
