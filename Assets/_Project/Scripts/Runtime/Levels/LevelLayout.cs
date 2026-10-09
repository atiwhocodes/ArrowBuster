using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Root component of every level layout prefab (04 §2): framing override, optional clear line, and the layout
    /// hierarchy convention Environment / Gameplay / Decor.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelLayout : MonoBehaviour
    {
        [Tooltip("Objectives whose centre drops below this line clear (D-039).")]
        [SerializeField] private bool _clearLineEnabled;
        [SerializeField] private float _clearLineY = 2.5f;
        [Tooltip("Extra world height to keep in frame above the design height (set pieces).")]
        [SerializeField, Min(0f)] private float _extraHeight;

        public bool ClearLineEnabled => _clearLineEnabled;
        public float ClearLineY => _clearLineY;
        public float ExtraHeight => _extraHeight;

        public void Configure(bool clearLineEnabled, float clearLineY, float extraHeight)
        {
            _clearLineEnabled = clearLineEnabled;
            _clearLineY = clearLineY;
            _extraHeight = extraHeight;
        }

        public TutorialAnchor FindAnchor(string id)
        {
            foreach (TutorialAnchor anchor in GetComponentsInChildren<TutorialAnchor>(true))
                if (anchor.Id == id) return anchor;
            return null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.25f);
            float w = GameConstants.PlayAreaWidth, h = GameConstants.PlayAreaHeight;
            Gizmos.DrawWireCube(new Vector3(0f, h * 0.5f, 0f), new Vector3(w, h, 0.01f));
            if (_clearLineEnabled)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.6f);
                Gizmos.DrawLine(new Vector3(-w * 0.5f, _clearLineY, 0f), new Vector3(w * 0.5f, _clearLineY, 0f));
            }
        }
#endif
    }
}
