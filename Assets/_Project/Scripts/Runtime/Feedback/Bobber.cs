using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Cosmetic bob-and-spin for markers (e.g. the purple "protected" gem above the vase). No gameplay effect.</summary>
    public sealed class Bobber : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 0.08f;
        [SerializeField] private float _speed = 2.2f;
        [SerializeField] private float _spinDegPerSecond = 70f;

        private Vector3 _base;

        private void Awake() => _base = transform.localPosition;

        private void Update()
        {
            transform.localPosition = _base + Vector3.up * Mathf.Sin(Time.time * _speed) * _amplitude;
            transform.Rotate(0f, _spinDegPerSecond * Time.deltaTime, 0f, Space.Self);
        }
    }
}
