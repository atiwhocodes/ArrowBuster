using System;

namespace ArrowBuster.Editor
{
    /// <summary>One quiver slot in a <see cref="LevelBlueprint"/>, e.g. {"type": "Oak", "count": 2}.</summary>
    [Serializable]
    public sealed class BlueprintQuiver
    {
        public string type = "Oak";
        public int count = 1;
    }
}
