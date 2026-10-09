using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Marks a point in a layout that a tutorial callout can point at (04 §7). Prefab: Marker_TutorialAnchor.</summary>
    public sealed class TutorialAnchor : MonoBehaviour
    {
        [SerializeField] private string _id = "anchor";

        public string Id => _id;

        public void SetId(string id) => _id = id;
    }
}
