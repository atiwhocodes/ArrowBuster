using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Maps <see cref="SfxId"/> to clip variations with volume, pitch range and a per-event cooldown (05 §10).
    /// Art/Audio swaps placeholder clips here without code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Sound Library", fileName = "SoundLibrary")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public SfxId id;
            public AudioClip[] clips = Array.Empty<AudioClip>();
            [Range(0f, 1f)] public float volume = 0.8f;
            public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
            [Tooltip("Minimum seconds between two plays of this event (prevents machine-gun stacking).")]
            [Min(0f)] public float cooldown = 0.04f;
            [Tooltip("Higher wins when all voices are busy.")]
            public int priority = 1;
        }

        [SerializeField] private List<Entry> _entries = new List<Entry>();
        [SerializeField] private AudioClip _music;

        private Dictionary<SfxId, Entry> _lookup;

        public AudioClip Music => _music;

        public Entry Get(SfxId id)
        {
            if (_lookup == null)
            {
                _lookup = new Dictionary<SfxId, Entry>(_entries.Count);
                foreach (Entry entry in _entries)
                    if (entry != null) _lookup[entry.id] = entry;
            }
            return _lookup.TryGetValue(id, out Entry found) ? found : null;
        }

        /// <summary>Editor/tooling: replaces the entry list.</summary>
        public void SetEntries(List<Entry> entries, AudioClip music)
        {
            _entries = entries;
            _music = music;
            _lookup = null;
        }

        private void OnValidate() => _lookup = null;
    }
}
