using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// Imports JSON blueprints into level assets (04 §8 tooling): <c>W&lt;w&gt;_L&lt;nn&gt;.asset</c> (LevelData) and
    /// <c>Lvl_W&lt;w&gt;_L&lt;nn&gt;.prefab</c> (layout: Environment / Gameplay / Decor, library prefab instances only).
    /// Menu: Arrow Buster ▸ Levels ▸ Import Blueprints.
    /// </summary>
    public static class LevelBlueprintImporter
    {
        public const string BlueprintFolder = "Assets/_Project/Levels/Blueprints";

        [MenuItem("Arrow Buster/Levels/Import Blueprints", priority = 10)]
        public static void ImportAllMenu()
        {
            List<LevelData> levels = ImportAll();
            Debug.Log($"[Arrow Buster] Imported {levels.Count} level blueprint(s).");
        }

        public static List<LevelData> ImportAll()
        {
            var result = new List<LevelData>();
            if (!Directory.Exists(BlueprintFolder)) return result;
            string[] files = Directory.GetFiles(BlueprintFolder, "*.json");
            Array.Sort(files, StringComparer.Ordinal);
            foreach (string file in files)
            {
                LevelData level = Import(file.Replace('\\', '/'));
                if (level != null) result.Add(level);
            }
            AssetDatabase.SaveAssets();
            return result;
        }

        public static LevelData Import(string path)
        {
            LevelBlueprint bp = JsonUtility.FromJson<LevelBlueprint>(File.ReadAllText(path));
            if (bp == null)
            {
                Debug.LogError("[Arrow Buster] Could not parse " + path);
                return null;
            }
            string id = $"W{bp.world}_L{bp.number:00}";
            GameObject layoutPrefab = BuildLayout(bp, id);
            if (layoutPrefab == null) return null;

            string levelFolder = $"Assets/_Project/ScriptableObjects/Levels/World{bp.world}";
            Directory.CreateDirectory(levelFolder);
            string assetPath = $"{levelFolder}/{id}.asset";
            var level = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);
            if (level == null)
            {
                level = ScriptableObject.CreateInstance<LevelData>();
                AssetDatabase.CreateAsset(level, assetPath);
            }

            var quiver = new List<QuiverEntry>();
            foreach (BlueprintQuiver q in bp.quiver)
                quiver.Add(new QuiverEntry((ArrowType)Enum.Parse(typeof(ArrowType), q.type, true), Mathf.Max(1, q.count)));
            var prompt = new TutorialPromptData(bp.calloutText, bp.calloutAnchor, bp.calloutReshowAfterMisses);
            level.Configure(bp.world, bp.number, bp.name, bp.banner, layoutPrefab, quiver, bp.goldPar, prompt, bp.ghostHand, bp.setPiece, bp.difficulty);

            var so = new SerializedObject(level);
            so.FindProperty("_trajectoryPreviewScale").floatValue = Mathf.Clamp(bp.previewScale, 0.4f, 1f);
            so.ApplyModifiedPropertiesWithoutUndo();

            var shots = new List<IntendedShot>();
            foreach (BlueprintShot s in bp.shots) shots.Add(new IntendedShot(s.angle, s.power, s.delay));
            level.SetIntendedShots(shots);
            EditorUtility.SetDirty(level);
            return level;
        }

        private static GameObject BuildLayout(LevelBlueprint bp, string id)
        {
            var root = new GameObject("Lvl_" + id);
            root.AddComponent<LevelLayout>().Configure(bp.clearLine, bp.clearLineY, bp.extraHeight);
            Transform environment = new GameObject("Environment").transform;
            Transform gameplay = new GameObject("Gameplay").transform;
            Transform decor = new GameObject("Decor").transform;
            environment.SetParent(root.transform, false);
            gameplay.SetParent(root.transform, false);
            decor.SetParent(root.transform, false);

            foreach (BlueprintObject o in bp.objects)
            {
                GameObject prefab = FindPrefab(o.prefab);
                if (prefab == null)
                {
                    Debug.LogError($"[Arrow Buster] {id}: unknown prefab '{o.prefab}'.");
                    continue;
                }
                bool isEnvironment = prefab.layer == PhysicsLayers.Environment || prefab.layer == PhysicsLayers.KillZone;
                Transform parent = isEnvironment ? environment : gameplay;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                if (!string.IsNullOrEmpty(o.name)) instance.name = o.name;
                instance.transform.localPosition = new Vector3(o.x, o.y, 0f);
                instance.transform.localRotation = Quaternion.Euler(0f, 0f, o.rot);
                bool scaled = !Mathf.Approximately(o.sx, 1f) || !Mathf.Approximately(o.sy, 1f);
                if (scaled)
                {
                    if (prefab.GetComponent<Rigidbody>() != null)
                        Debug.LogWarning($"[Arrow Buster] {id}: '{o.prefab}' is dynamic; scaling it is not allowed (embedded arrows would distort). Ignored.");
                    else instance.transform.localScale = new Vector3(o.sx, o.sy, 1f);
                }
                if (!string.IsNullOrEmpty(o.anchorId))
                {
                    var anchor = instance.GetComponent<TutorialAnchor>();
                    if (anchor != null) anchor.SetId(o.anchorId);
                }
            }

            string folder = $"Assets/_Project/Prefabs/Levels/World{bp.world}";
            Directory.CreateDirectory(folder);
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, $"{folder}/Lvl_{id}.prefab");
            UnityEngine.Object.DestroyImmediate(root);
            return saved;
        }

        private static readonly Dictionary<string, GameObject> PrefabCache = new Dictionary<string, GameObject>();

        public static GameObject FindPrefab(string name)
        {
            if (PrefabCache.TryGetValue(name, out GameObject cached) && cached != null) return cached;
            foreach (string guid in AssetDatabase.FindAssets(name + " t:Prefab", new[] { PrefabFactory.Root }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(assetPath) != name) continue;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                PrefabCache[name] = prefab;
                return prefab;
            }
            return null;
        }

        public static void ClearCache() => PrefabCache.Clear();
    }
}
