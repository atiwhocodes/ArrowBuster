using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>An ordered playlist of levels (04 §2): LC_Main, LC_VerticalSlice, LC_DebugAll.</summary>
    [CreateAssetMenu(menuName = "Arrow Buster/Level Catalog", fileName = "LC_VerticalSlice")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private string _title = "Vertical Slice";
        [SerializeField] private List<LevelData> _levels = new List<LevelData>();

        public string Title => _title;
        public IReadOnlyList<LevelData> Levels => _levels;
        public int Count => _levels.Count;

        public LevelData Get(int index) => index >= 0 && index < _levels.Count ? _levels[index] : null;

        public int IndexOf(LevelData level) => _levels.IndexOf(level);

        public void SetLevels(string title, List<LevelData> levels)
        {
            _title = title;
            _levels = levels;
        }
    }
}
