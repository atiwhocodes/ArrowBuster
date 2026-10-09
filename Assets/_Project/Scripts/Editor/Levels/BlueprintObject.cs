using System;

namespace ArrowBuster.Editor
{
    /// <summary>One library-prefab placement in a <see cref="LevelBlueprint"/> (position, rotation, scale, optional name and tutorial anchor id).</summary>
    [Serializable]
    public sealed class BlueprintObject
    {
        public string prefab = "";
        public float x;
        public float y;
        public float rot;
        public float sx = 1f;
        public float sy = 1f;
        public string name = "";
        public string anchorId = "";
    }
}
