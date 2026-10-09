using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Pooled 2D audio playback with per-event variation (05 §10).</summary>
    public interface IAudioService
    {
        float SfxVolume { get; set; }
        float MusicVolume { get; set; }
        void Play(SfxId id, float volumeScale = 1f, float pitchScale = 1f);
        void PlayMusic(AudioClip clip);
    }
}
