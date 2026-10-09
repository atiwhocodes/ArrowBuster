using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArrowBuster
{
    /// <summary>
    /// Rules for one hand-made level (04 §2, schema v2 subset). The physical layout lives in the layout prefab;
    /// this asset holds identity, the curated quiver (consumed in order, D-023), gold par, teaching data and the
    /// recorded intended solution used by the solvability bot.
    /// </summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Level Data", fileName = "W1_L01")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        [FormerlySerializedAs("worldId")]
        [SerializeField, Range(1, GameConstants.WorldCount)] private int _worldId = 1;
        [FormerlySerializedAs("levelNumber")]
        [SerializeField, Range(1, GameConstants.LevelsPerWorld)] private int _levelNumber = 1;
        [FormerlySerializedAs("displayName")]
        [SerializeField] private string _displayName;
        [Tooltip("Level-start banner, e.g. \"Break the target!\" or \"Protect the vase!\".")]
        [SerializeField] private string _banner;

        [Header("Layout")]
        [FormerlySerializedAs("layoutPrefab")]
        [Tooltip("Prefab with structures, objectives, props and hazards, positioned on the play plane (root has LevelLayout).")]
        [SerializeField] private GameObject _layoutPrefab;

        [Header("Quiver (fixed, curated, fired in order — D-023)")]
        [FormerlySerializedAs("quiver")]
        [SerializeField] private List<QuiverEntry> _quiver = new List<QuiverEntry> { new QuiverEntry(ArrowType.Oak, 3) };

        [Header("Stars")]
        [FormerlySerializedAs("goldPar")]
        [Tooltip("Gold par: arrows used for 3 stars. 2 stars = par + 1. 1 star = any clear (D-084 caps bonus-arrow clears).")]
        [SerializeField, Min(1)] private int _goldPar = 1;

        [Header("Teaching")]
        [SerializeField] private TutorialPromptData _tutorialPrompt;
        [SerializeField] private bool _showGhostHand;
        [FormerlySerializedAs("trajectoryPreviewScale")]
        [Tooltip("Trajectory preview length multiplier. Levels 1–15 stay at 1; never below 0.4 (D-073).")]
        [SerializeField, Range(0.4f, 1f)] private float _trajectoryPreviewScale = 1f;

        [Header("Intended solution (solvability bot)")]
        [SerializeField] private List<IntendedShot> _intendedShots = new List<IntendedShot>();

        [Header("Classification")]
        [SerializeField] private bool _isSetPiece;
        [SerializeField, Range(1, 5)] private int _difficulty = 1;
        [SerializeField, TextArea] private string _designerNotes;

        public int WorldId { get => _worldId; internal set => _worldId = value; }
        public int LevelNumber { get => _levelNumber; internal set => _levelNumber = value; }
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? LevelId : _displayName;
        public string Banner => _banner;
        public GameObject LayoutPrefab => _layoutPrefab;
        public IReadOnlyList<QuiverEntry> Quiver => _quiver;
        public int GoldPar { get => _goldPar; internal set => _goldPar = value; }
        public TutorialPromptData TutorialPrompt => _tutorialPrompt;
        public bool ShowGhostHand => _showGhostHand;
        public float TrajectoryPreviewScale => _trajectoryPreviewScale;
        public IReadOnlyList<IntendedShot> IntendedShots => _intendedShots;
        public bool IsSetPiece => _isSetPiece;
        public int Difficulty => _difficulty;

        /// <summary>Asset/analytics id, e.g. "W1_L04" (D-047).</summary>
        public string LevelId => $"W{_worldId}_L{_levelNumber:00}";

        /// <summary>Global level index 1..60 (D-047).</summary>
        public int GlobalIndex => (_worldId - 1) * GameConstants.LevelsPerWorld + _levelNumber;

        public int TotalArrows
        {
            get
            {
                int total = 0;
                foreach (QuiverEntry entry in _quiver) total += entry.count;
                return total;
            }
        }

        /// <summary>Stars for a clear using <paramref name="arrowsUsed"/> arrows (mvp.md §3).</summary>
        public int StarsFor(int arrowsUsed) => StarRules.Compute(arrowsUsed, _goldPar, bonusArrowUsed: false);

        /// <summary>Tests/tooling: replace the quiver.</summary>
        internal void SetQuiver(List<QuiverEntry> quiver) => _quiver = quiver;

        /// <summary>Tooling: records the intended solution (ShotRecorder).</summary>
        public void SetIntendedShots(List<IntendedShot> shots) => _intendedShots = shots;

        /// <summary>Tooling: configures every authoring field (content generator / level editor).</summary>
        public void Configure(int worldId, int levelNumber, string displayName, string banner, GameObject layoutPrefab,
            List<QuiverEntry> quiver, int goldPar, TutorialPromptData prompt, bool ghostHand, bool setPiece, int difficulty)
        {
            _worldId = worldId;
            _levelNumber = levelNumber;
            _displayName = displayName;
            _banner = banner;
            _layoutPrefab = layoutPrefab;
            _quiver = quiver;
            _goldPar = goldPar;
            _tutorialPrompt = prompt;
            _showGhostHand = ghostHand;
            _isSetPiece = setPiece;
            _difficulty = difficulty;
        }

        private void OnValidate()
        {
            if (_goldPar > TotalArrows && TotalArrows > 0)
                Debug.LogWarning($"[{name}] goldPar ({_goldPar}) exceeds total arrows ({TotalArrows}).", this);
        }
    }
}
