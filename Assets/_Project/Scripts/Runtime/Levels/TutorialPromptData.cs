using System;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>At most one text callout per level (04 §7, D-076). The ghost hand is a separate LevelData flag.</summary>
    [Serializable]
    public struct TutorialPromptData
    {
        [Tooltip("Callout text, ≤ 32 characters (validator V-13). Empty = no callout.")]
        public string text;
        [Tooltip("TutorialAnchor id inside the layout the callout points at.")]
        public string anchorId;
        [Tooltip("Show again after this many misses (0 = never).")]
        [Min(0)] public int reshowAfterMisses;

        public bool HasCallout => !string.IsNullOrEmpty(text);

        public TutorialPromptData(string text, string anchorId, int reshowAfterMisses)
        {
            this.text = text;
            this.anchorId = anchorId;
            this.reshowAfterMisses = reshowAfterMisses;
        }
    }
}
