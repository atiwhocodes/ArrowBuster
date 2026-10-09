using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Latches the first protected-object loss of the level (02 §8.2).</summary>
    public sealed class ProtectedTracker
    {
        private readonly List<ProtectedObject> _items = new List<ProtectedObject>(4);

        public bool LostThisLevel { get; private set; }
        public ProtectedInfo LossInfo { get; private set; }
        public IReadOnlyList<ProtectedObject> Items => _items;

        public void Rebuild(Transform root)
        {
            foreach (ProtectedObject p in _items) if (p != null) p.Lost -= OnLost;
            _items.Clear();
            root.GetComponentsInChildren(true, _items);
            foreach (ProtectedObject p in _items) p.Lost += OnLost;
            LostThisLevel = false;
        }

        public void Tick()
        {
            for (int i = 0; i < _items.Count; i++) _items[i].Tick();
        }

        private void OnLost(ProtectedObject item, ProtectedLossReason reason)
        {
            if (LostThisLevel) return;
            LostThisLevel = true;
            LossInfo = new ProtectedInfo(item.Kind, reason, item.transform.position);
            GameEvents.RaiseProtectedLost(LossInfo);
        }
    }
}
