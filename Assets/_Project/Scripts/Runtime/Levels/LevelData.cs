using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Authoring data for one hand-made level (GDD §7, §12 "Level configuration data").
    /// The physical layout lives in <see cref="layoutPrefab"/>; this asset holds the rules.
    /// </summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Level Data", fileName = "W1_L01")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        [Range(1, GameConstants.WorldCount)] public int worldId = 1;
        [Range(1, GameConstants.LevelsPerWorld)] public int levelNumber = 1;
        public string displayName;

        [Header("Layout")]
        [Tooltip("Prefab with structures, objectives, props and hazards, positioned on the play plane.")]
        public GameObject layoutPrefab;

        [Header("Quiver (fixed, curated — no player loadout in MVP)")]
        public List<QuiverEntry> quiver = new List<QuiverEntry> { new QuiverEntry(ArrowType.Oak, 3) };

        [Header("Stars")]
        [Tooltip("Gold par: arrows used for 3 stars. 2 stars = par + 1. 1 star = any clear.")]
        [Min(1)] public int goldPar = 1;

        [Header("Teaching")]
        [Tooltip("Optional callout for teaching levels only, e.g. \"Aim for the rope\".")]
        public string tutorialPrompt;
        [Tooltip("Trajectory preview length multiplier. Levels 1-15 should stay at 1.")]
        [Range(0.3f, 1f)] public float trajectoryPreviewScale = 1f;

        /// <summary>Global level index 1..60.</summary>
        public int GlobalIndex => (worldId - 1) * GameConstants.LevelsPerWorld + levelNumber;

        public int TotalArrows
        {
            get
            {
                int total = 0;
                foreach (var entry in quiver) total += entry.count;
                return total;
            }
        }

        /// <summary>Stars for a clear using <paramref name="arrowsUsed"/> arrows (GDD §3 Scoring).</summary>
        public int StarsFor(int arrowsUsed)
        {
            if (arrowsUsed <= goldPar) return 3;
            if (arrowsUsed == goldPar + 1) return 2;
            return 1;
        }

        private void OnValidate()
        {
            if (goldPar > TotalArrows && TotalArrows > 0)
                Debug.LogWarning($"[{name}] goldPar ({goldPar}) exceeds total arrows ({TotalArrows}).", this);
        }
    }
}
