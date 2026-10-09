using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>Tracks the level's required objectives (02 §8.2) and raises <see cref="GameEvents.ObjectiveCleared"/>.</summary>
    public sealed class ObjectiveTracker
    {
        private readonly List<Objective> _objectives = new List<Objective>(8);

        public int Total => _objectives.Count;
        public int Remaining { get; private set; }
        public bool AllCleared => Total > 0 && Remaining == 0;
        public IReadOnlyList<Objective> Objectives => _objectives;

        public void Rebuild(Transform root)
        {
            foreach (Objective o in _objectives) if (o != null) o.Cleared -= OnCleared;
            _objectives.Clear();
            root.GetComponentsInChildren(true, _objectives);
            for (int i = 0; i < _objectives.Count; i++)
            {
                _objectives[i].Index = i;
                _objectives[i].Cleared += OnCleared;
            }
            Remaining = _objectives.Count;
        }

        public void Tick(bool clearLineEnabled, float clearLineY)
        {
            for (int i = 0; i < _objectives.Count; i++) _objectives[i].CheckRules(clearLineEnabled, clearLineY);
        }

        private void OnCleared(Objective objective)
        {
            Remaining = Mathf.Max(0, Remaining - 1);
            GameEvents.RaiseObjectiveCleared(new ObjectiveInfo(objective.Kind, objective.Index, Remaining, objective.transform.position));
        }
    }
}
