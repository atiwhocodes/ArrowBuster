using UnityEngine;

namespace ArrowBuster
{
    /// <summary>
    /// Loads a level into the already-open Gameplay scene (D-011, 01 §4): return pooled objects, destroy the
    /// previous layout, instantiate the new one, rebuild registries, put bodies to sleep, reset the level clock.
    /// Restart = Load again (target ≤ 300 ms).
    /// </summary>
    public sealed class LevelLoader
    {
        private readonly Transform _levelRoot;
        private GameObject _instance;

        public LevelLayout Layout { get; private set; }
        public GameObject Instance => _instance;

        public LevelLoader(Transform levelRoot) => _levelRoot = levelRoot;

        public LevelLayout Load(LevelData level, ArrowSpawner spawner)
        {
            if (spawner != null) spawner.ReleaseAll();
            if (DebrisPool.Instance != null) DebrisPool.Instance.ReleaseAll();
            Breakable.ClearPending();
            Unload();

            LevelClock.Reset();
            TimeScaleController.ClearAll();
            CosmeticRandom.Seed(level != null ? level.GlobalIndex * 7919 : 1);

            if (level == null || level.LayoutPrefab == null)
            {
                Log.Error(LogCat.Level, "Level has no layout prefab: " + (level != null ? level.name : "null"));
                return null;
            }

            _instance = Object.Instantiate(level.LayoutPrefab, Vector3.zero, Quaternion.identity, _levelRoot);
            _instance.name = level.LayoutPrefab.name;
            Physics.SyncTransforms();
            Layout = _instance.GetComponent<LevelLayout>();
            if (Layout == null) Log.Warn(LogCat.Level, level.LevelId + ": layout root has no LevelLayout component.");

            PhysicsBodyRegistry.Rebuild(_instance.transform);
            PhysicsBodyRegistry.SleepAll();
            return Layout;
        }

        public void Unload()
        {
            PhysicsBodyRegistry.Clear();
            if (_instance != null)
            {
                _instance.SetActive(false);
                Object.Destroy(_instance);
            }
            _instance = null;
            Layout = null;
        }
    }
}
