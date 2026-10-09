using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>One slot in a level's curated quiver, e.g. "2 Oak".</summary>
    [Serializable]
    public struct QuiverEntry
    {
        public ArrowType type;
        [Min(1)] public int count;

        public QuiverEntry(ArrowType type, int count)
        {
            this.type = type;
            this.count = count;
        }
    }
}
