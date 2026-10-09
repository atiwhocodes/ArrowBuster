using System.Collections.Generic;
using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Pools arrows per type and enforces the active-arrow cap (02 §5): at <see cref="GameConstants.MaxActiveArrows"/>
    /// the oldest embedded arrow becomes Decor (or the oldest spent one is recycled). Survives restarts (D-011).
    /// </summary>
    public sealed class ArrowSpawner : MonoBehaviour
    {
        [SerializeField] private ArrowDefinition[] _definitions = new ArrowDefinition[0];
        [SerializeField, Min(1)] private int _prewarmPerType = 8;

        private readonly Dictionary<ArrowType, ArrowDefinition> _defs = new Dictionary<ArrowType, ArrowDefinition>();
        private readonly Dictionary<ArrowType, Stack<ArrowProjectile>> _pools = new Dictionary<ArrowType, Stack<ArrowProjectile>>();
        private readonly List<ArrowProjectile> _active = new List<ArrowProjectile>(16);
        private readonly List<ArrowProjectile> _decor = new List<ArrowProjectile>(16);
        private int _nextId;

        public int FlyingCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < _active.Count; i++) if (_active[i].IsFlying) n++;
                return n;
            }
        }

        public int ActiveCount => _active.Count;

        public ArrowDefinition Definition(ArrowType type) => _defs.TryGetValue(type, out ArrowDefinition def) ? def : null;

        public void SetDefinitions(ArrowDefinition[] definitions) => _definitions = definitions;

        private void Awake()
        {
            foreach (ArrowDefinition def in _definitions)
            {
                if (def == null || def.Prefab == null) continue;
                _defs[def.Type] = def;
                var pool = new Stack<ArrowProjectile>(_prewarmPerType);
                _pools[def.Type] = pool;
                for (int i = 0; i < _prewarmPerType; i++) pool.Push(Create(def));
            }
        }

        public ArrowProjectile Fire(ArrowType type, GameplayTuning tuning, IFlightEnvironment env, Vector3 tip, Vector3 velocity, Rect bounds)
        {
            if (!_defs.TryGetValue(type, out ArrowDefinition def))
            {
                Log.Error(LogCat.Arrow, "No ArrowDefinition for " + type);
                return null;
            }
            EnforceCap();
            Stack<ArrowProjectile> pool = _pools[type];
            ArrowProjectile arrow = pool.Count > 0 ? pool.Pop() : Create(def);
            _active.Add(arrow);
            arrow.Fire(++_nextId, def, tuning, env, tip, velocity, bounds);
            return arrow;
        }

        /// <summary>Returns every arrow (active and decor) to the pools. Called by the level loader first.</summary>
        public void ReleaseAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) _active[i].Release();
            for (int i = _decor.Count - 1; i >= 0; i--) _decor[i].Release();
            _active.Clear();
            _decor.Clear();
        }

        private void EnforceCap()
        {
            if (_active.Count < GameConstants.MaxActiveArrows) return;
            ArrowProjectile victim = null;
            foreach (ArrowProjectile a in _active)
                if (a.State == ArrowProjectile.Phase.Embedded && (victim == null || a.FiredAt < victim.FiredAt)) victim = a;
            if (victim != null)
            {
                victim.BecomeDecor();
                _active.Remove(victim);
                _decor.Add(victim);
                return;
            }
            foreach (ArrowProjectile a in _active)
                if (a.State == ArrowProjectile.Phase.Spent && (victim == null || a.FiredAt < victim.FiredAt)) victim = a;
            if (victim != null) victim.Release();
        }

        private ArrowProjectile Create(ArrowDefinition def)
        {
            ArrowProjectile arrow = Instantiate(def.Prefab, transform);
            arrow.gameObject.SetActive(false);
            arrow.Released += OnReleased;
            return arrow;
        }

        private void OnReleased(ArrowProjectile arrow)
        {
            _active.Remove(arrow);
            _decor.Remove(arrow);
            arrow.transform.SetParent(transform, false);
            if (_pools.TryGetValue(arrow.Type, out Stack<ArrowProjectile> pool) && !pool.Contains(arrow)) pool.Push(arrow);
        }
    }
}
