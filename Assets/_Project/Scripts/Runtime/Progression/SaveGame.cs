using System;
using System.Collections.Generic;

namespace ArrowBuster
{
    /// <summary>
    /// Root save DTO, schema v1 (06 §10). JsonUtility-friendly: lists instead of dictionaries.
    /// Only the vertical-slice subset is populated today; new fields are additive.
    /// </summary>
    [Serializable]
    public sealed class SaveGame
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public string installId = Guid.NewGuid().ToString("N");
        public long firstLaunchUtc = DateTime.UtcNow.Ticks;
        public float lifetimePlaySeconds;
        public List<LevelProgress> levels = new List<LevelProgress>();
        public int coins;
        public SettingsData settings = new SettingsData();
        public List<string> seenPrompts = new List<string>();

        /// <summary>Returns the progress row for a level, creating it if missing.</summary>
        public LevelProgress GetOrCreate(string levelId)
        {
            for (int i = 0; i < levels.Count; i++)
                if (levels[i].levelId == levelId) return levels[i];
            var row = new LevelProgress { levelId = levelId };
            levels.Add(row);
            return row;
        }

        public LevelProgress Find(string levelId)
        {
            for (int i = 0; i < levels.Count; i++)
                if (levels[i].levelId == levelId) return levels[i];
            return null;
        }
    }
}
