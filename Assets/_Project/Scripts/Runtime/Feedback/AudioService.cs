using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Pooled 2D audio (05 §10): 24 voices, per-event cooldown, voice stealing by priority, one looping music source.
    /// Uses <see cref="CosmeticRandom"/> for clip/pitch variation so gameplay randomness is untouched.
    /// </summary>
    public sealed class AudioService : MonoBehaviour, IAudioService
    {
        private const int VoiceCount = 24;

        [SerializeField] private SoundLibrary _library;

        private readonly List<AudioSource> _voices = new List<AudioSource>(VoiceCount);
        private readonly int[] _voicePriority = new int[VoiceCount];
        private readonly Dictionary<SfxId, float> _lastPlayed = new Dictionary<SfxId, float>();
        private AudioSource _music;
        private float _sfxVolume = 1f;
        private float _musicVolume = 0.55f;

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (_music != null) _music.volume = _musicVolume;
            }
        }

        public void SetLibrary(SoundLibrary library) => _library = library;

        private void Awake()
        {
            for (int i = 0; i < VoiceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                _voices.Add(source);
            }
            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;
            _music.volume = _musicVolume;
        }

        public void Play(SfxId id, float volumeScale = 1f, float pitchScale = 1f)
        {
            if (id == SfxId.None || _library == null || _sfxVolume <= 0f) return;
            SoundLibrary.Entry entry = _library.Get(id);
            if (entry == null || entry.clips == null || entry.clips.Length == 0) return;

            float now = Time.unscaledTime;
            if (_lastPlayed.TryGetValue(id, out float last) && now - last < entry.cooldown) return;
            _lastPlayed[id] = now;

            int voice = FindVoice(entry.priority);
            if (voice < 0) return;

            AudioClip clip = entry.clips[Mathf.Min(entry.clips.Length - 1, (int)(CosmeticRandom.Value * entry.clips.Length))];
            AudioSource source = _voices[voice];
            source.clip = clip;
            source.volume = entry.volume * volumeScale * _sfxVolume;
            source.pitch = CosmeticRandom.Range(entry.pitchRange.x, entry.pitchRange.y) * pitchScale;
            source.Play();
            _voicePriority[voice] = entry.priority;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (_music == null || clip == null || _music.clip == clip) return;
            _music.clip = clip;
            _music.volume = _musicVolume;
            _music.Play();
        }

        private int FindVoice(int priority)
        {
            int lowest = -1;
            for (int i = 0; i < _voices.Count; i++)
            {
                if (!_voices[i].isPlaying) return i;
                if (_voicePriority[i] <= priority && (lowest < 0 || _voicePriority[i] < _voicePriority[lowest])) lowest = i;
            }
            return lowest;
        }
    }
}
