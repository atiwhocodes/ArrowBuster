using System;
using System.Collections.Generic;

namespace ArrowBuster
{
    /// <summary>The level's curated quiver, consumed in authored order (D-023). Pure, unit-tested.</summary>
    public sealed class QuiverModel
    {
        private readonly List<ArrowType> _arrows = new List<ArrowType>(8);
        private int _next;

        public int Total => _arrows.Count;
        public int Used => _next;
        public int Remaining => _arrows.Count - _next;
        public bool BonusUsed { get; private set; }

        public event Action Changed;

        public void Load(IReadOnlyList<QuiverEntry> entries)
        {
            _arrows.Clear();
            _next = 0;
            BonusUsed = false;
            if (entries != null)
                foreach (QuiverEntry entry in entries)
                    for (int i = 0; i < entry.count; i++) _arrows.Add(entry.type);
            Changed?.Invoke();
        }

        /// <summary>The arrow that will be fired next, or null when empty.</summary>
        public ArrowType? Peek() => _next < _arrows.Count ? _arrows[_next] : (ArrowType?)null;

        public ArrowType TypeAt(int index) => _arrows[index];

        public bool TryConsume(out ArrowType type)
        {
            if (_next >= _arrows.Count)
            {
                type = default;
                return false;
            }
            type = _arrows[_next++];
            Changed?.Invoke();
            return true;
        }

        /// <summary>Rewarded +1 arrow (D-062): appended, and the clear is capped at 1★.</summary>
        public void AddBonus(ArrowType type)
        {
            _arrows.Add(type);
            BonusUsed = true;
            Changed?.Invoke();
        }
    }
}
