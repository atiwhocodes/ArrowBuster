using System;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// JSON level blueprint (04 §4 "create a level without new code"): level rules plus a list of library-prefab
    /// placements. <see cref="LevelBlueprintImporter"/> turns it into a LevelData asset and a layout prefab.
    /// Files: Assets/_Project/Levels/Blueprints/W&lt;w&gt;_L&lt;nn&gt;.json.
    /// </summary>
    [Serializable]
    public sealed class LevelBlueprint
    {
        public int world = 1;
        public int number = 1;
        public string name = "";
        public string banner = "";
        public BlueprintQuiver[] quiver = Array.Empty<BlueprintQuiver>();
        public int goldPar = 1;
        public bool ghostHand;
        public string calloutText = "";
        public string calloutAnchor = "";
        public int calloutReshowAfterMisses;
        public float previewScale = 1f;
        public bool setPiece;
        public int difficulty = 1;
        public bool clearLine;
        public float clearLineY;
        public float extraHeight;
        public BlueprintObject[] objects = Array.Empty<BlueprintObject>();
        public BlueprintShot[] shots = Array.Empty<BlueprintShot>();
    }

    [Serializable]
    public sealed class BlueprintQuiver
    {
        public string type = "Oak";
        public int count = 1;
    }

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

    [Serializable]
    public sealed class BlueprintShot
    {
        public float angle;
        public float power;
        public float delay;
    }
}
