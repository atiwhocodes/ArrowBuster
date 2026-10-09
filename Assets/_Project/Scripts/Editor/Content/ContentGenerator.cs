using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// One-click content build for the vertical slice: procedural art kit, material profiles (03 §4), prefab
    /// library, arrow definition (02 §15), synthesised audio + sound library, tuning, UI font, levels from
    /// blueprints, the LC_VerticalSlice playlist, AppConfig, the gameplay root prefab and the Boot/Gameplay scenes.
    /// Menu: Arrow Buster ▸ Build ▸ Generate Content. Safe to re-run; profile/tuning assets are created once and
    /// then left to designers.
    /// </summary>
    public static class ContentGenerator
    {
        private const string SoRoot = "Assets/_Project/ScriptableObjects";
        private const string SfxFolder = "Assets/_Project/Audio/SFX";
        private const string MusicFolder = "Assets/_Project/Audio/Music";
        private const string FontPath = "Assets/_Project/UI/Fonts/LilitaOne-Regular.ttf";
        private const string FontAssetPath = "Assets/_Project/UI/Fonts/LilitaOne SDF.asset";
        private const string ScenesFolder = "Assets/_Project/Scenes";

        private static readonly string[] SliceOrder = { "W1_L01", "W1_L04", "W1_L07", "W1_L10", "W1_L16" };

        [MenuItem("Arrow Buster/Build/Generate Content", priority = 1)]
        public static void GenerateAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ArtKit.ClearCache();
            LevelBlueprintImporter.ClearCache();
            EnsureFolders();
            EnsureTag("Ground");

            PrefabFactory.Profiles profiles = MaterialProfiles();
            Material dot = ArtKit.Unlit("PreviewDot", Color.white, transparent: true);
            Material ring = ArtKit.Unlit("PreviewRing", new Color(1f, 0.85f, 0.4f, 0.9f), transparent: true);
            ArtKit.Quad("Quad");
            ArtKit.Ring("Ring");
            Texture2D sky = ArtKit.Gradient("SkyGreenwood", ArtKit.SkyBottom, ArtKit.SkyTop);

            BuildLibrary(profiles);
            ArrowProjectile arrowPrefab = PrefabFactory.ArrowOak();
            ArrowDefinition oak = ArrowDefinitionAsset(arrowPrefab);
            GameObject bow = PrefabFactory.Bow(dot, ring);
            GameObject backdrop = PrefabFactory.Backdrop(sky);
            AssetDatabase.SaveAssets();

            SoundLibrary sounds = Audio();
            GameplayTuning tuning = Tuning();
            TMP_FontAsset font = Font();

            List<LevelData> levels = LevelBlueprintImporter.ImportAll();
            LevelCatalog slice = SliceCatalog(levels);

            Material debris = ArtKit.Lit("Debris", Color.white, 0.1f);
            Mesh debrisMesh = ArtKit.Box("DebrisChunk", Vector3.one, 0.12f);
            AppConfig config = AppConfigAsset(sounds, tuning, slice, ArtKit.Particles(), debris, debrisMesh, font);

            GameObject root = GameplayRoot(bow, backdrop, oak);
            Scenes(root);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Arrow Buster] Content generated: {levels.Count} levels, config {AssetDatabase.GetAssetPath(config)}.");
        }

        // ------------------------------------------------------------------ setup

        private static void EnsureFolders()
        {
            foreach (string folder in new[]
                     {
                         ArtKit.MaterialFolder, ArtKit.MeshFolder, ArtKit.TextureFolder, SfxFolder, MusicFolder,
                         $"{SoRoot}/Materials", $"{SoRoot}/Arrows", $"{SoRoot}/Config", $"{SoRoot}/Audio", $"{SoRoot}/Levels",
                         "Assets/_Project/Resources", "Assets/_Project/Prefabs/Roots", LevelBlueprintImporter.BlueprintFolder
                     })
                Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        private static void EnsureTag(string tag)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tags = tagManager.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T LoadOrCreate<T>(string path, out bool created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            created = asset == null;
            if (created)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        // ------------------------------------------------------------------ data assets

        /// <summary>03 §4.1 graybox values. Created once; afterwards the assets are the source of truth.</summary>
        private static PrefabFactory.Profiles MaterialProfiles()
        {
            MaterialProfile Make(string name, System.Action<MaterialProfile> configure)
            {
                var profile = LoadOrCreate<MaterialProfile>($"{SoRoot}/Materials/MP_{name}.asset", out bool created);
                if (created)
                {
                    configure(profile);
                    EditorUtility.SetDirty(profile);
                }
                return profile;
            }

            return new PrefabFactory.Profiles
            {
                Straw = Make("Straw", p => p.Configure(MaterialKind.Straw, MaterialPalette.Straw, 1.2f, 0.6f, 0.5f, PhysicsMaterialCombine.Average, 0.05f,
                    false, 12f, 0.8f, 1f, 8f, true, 80f, 0f, 0.3f, 0f, 0.6f, 1f, 1.5f, 3, 1.2f)),
                Timber = Make("Timber", p => p.Configure(MaterialKind.Timber, MaterialPalette.Timber, 4f, 0.65f, 0.55f, PhysicsMaterialCombine.Average, 0.05f,
                    false, 20f, 2.5f, 0.5f, 25f, true, 60f, 5f, 0.3f, 0f, 0.6f, 0.9f, 1f, 4, 1.6f)),
                Stone = Make("Stone", p => p.Configure(MaterialKind.Stone, MaterialPalette.Stone, 12f, 0.8f, 0.7f, PhysicsMaterialCombine.Average, 0.02f,
                    false, 80f, 6f, 0.3f, 120f, false, 0f, 0f, 0.25f, 0f, 0.6f, 0.7f, 0.5f, 3, 2f)),
                Ice = Make("Ice", p => p.Configure(MaterialKind.Ice, MaterialPalette.Ice, 5f, 0.08f, 0.04f, PhysicsMaterialCombine.Minimum, 0.1f,
                    false, 10f, 1.5f, 1.2f, 14f, false, 0f, 0f, 0.4f, 0f, 0.6f, 1f, 1f, 5, 1.2f)),
                Metal = Make("Metal", p => p.Configure(MaterialKind.Metal, MaterialPalette.Metal, 16f, 0.4f, 0.3f, PhysicsMaterialCombine.Average, 0.15f,
                    true, 0f, 0f, 0f, 0f, false, 0f, 0f, 0.3f, 25f, 0.6f, 0.5f, 0f, 0, 1f)),
                Earth = Make("Earth", p => p.Configure(MaterialKind.Earth, MaterialPalette.Earth, 1f, 0.8f, 0.7f, PhysicsMaterialCombine.Average, 0f,
                    true, 0f, 0f, 0f, 0f, true, 89f, 0f, 0.3f, 0f, 0.6f, 0f, 0f, 0, 1f)),
            };
        }

        private static ArrowDefinition ArrowDefinitionAsset(ArrowProjectile prefab)
        {
            var oak = LoadOrCreate<ArrowDefinition>($"{SoRoot}/Arrows/AD_Oak.asset", out bool created);
            if (created) oak.Configure(ArrowType.Oak, prefab, 4f, 13f, 1f, 1f, 15f, 20f, 1f, 2);
            else
            {
                var so = new SerializedObject(oak);
                so.FindProperty("_prefab").objectReferenceValue = prefab;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(oak);
            return oak;
        }

        private static GameplayTuning Tuning()
        {
            var tuning = LoadOrCreate<GameplayTuning>($"{SoRoot}/Config/GameplayTuning.asset", out _);
            EditorUtility.SetDirty(tuning);
            return tuning;
        }

        private static LevelCatalog SliceCatalog(List<LevelData> imported)
        {
            var catalog = LoadOrCreate<LevelCatalog>($"{SoRoot}/Levels/LC_VerticalSlice.asset", out _);
            var ordered = new List<LevelData>();
            foreach (string id in SliceOrder)
            {
                LevelData level = imported.Find(l => l.LevelId == id);
                if (level != null) ordered.Add(level);
            }
            catalog.SetLevels("Vertical Slice", ordered);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static AppConfig AppConfigAsset(SoundLibrary sounds, GameplayTuning tuning, LevelCatalog catalog, Material particles,
            Material debris, Mesh debrisMesh, TMP_FontAsset font)
        {
            var config = LoadOrCreate<AppConfig>("Assets/_Project/Resources/AppConfig.asset", out _);
            config.Configure(sounds, tuning, catalog, particles, debris, debrisMesh);
            config.SetUiFont(font);
            EditorUtility.SetDirty(config);
            return config;
        }

        // ------------------------------------------------------------------ library

        private static void BuildLibrary(PrefabFactory.Profiles p)
        {
            PrefabFactory.Crate("Struct_Crate_Timber_1x1", new Vector2(1f, 1f), p);
            PrefabFactory.Crate("Struct_Crate_Timber_2x1", new Vector2(2f, 1f), p);
            PrefabFactory.TimberPiece("Struct_Post_Timber_0.5x2", new Vector2(0.5f, 2f), p);
            PrefabFactory.TimberPiece("Struct_Pole_Timber_0.25x2", new Vector2(0.25f, 2f), p, hpOverride: 4f);
            PrefabFactory.TimberPiece("Struct_Plank_Timber_4x0.25", new Vector2(4f, 0.25f), p);
            PrefabFactory.StoneBlock("Struct_Block_Stone_1x1", new Vector2(1f, 1f), p);
            PrefabFactory.CrestTarget(p);
            PrefabFactory.RoyalVase(p);
            PrefabFactory.Rope();
            PrefabFactory.Ground(p);
            PrefabFactory.Ledge(p);
            PrefabFactory.Stump(p);
            PrefabFactory.Beam(p);
            PrefabFactory.Pedestal(p);
            PrefabFactory.Awning(p);
            PrefabFactory.WaterPit();
            PrefabFactory.Pit();
            PrefabFactory.TutorialAnchor();
        }

        // ------------------------------------------------------------------ audio

        private static readonly (SfxId id, string clip, float volume, float cooldown, int priority)[] SoundMap =
        {
            (SfxId.BowDraw, "SFX_Bow_Draw", 0.5f, 0.1f, 1), (SfxId.BowFullDraw, "SFX_Bow_FullDraw", 0.5f, 0.2f, 1),
            (SfxId.BowRelease, "SFX_Bow_Release", 0.9f, 0.05f, 3), (SfxId.DrawCancel, "SFX_Bow_Cancel", 0.5f, 0.1f, 1),
            (SfxId.ArrowWhoosh, "SFX_Arrow_Whoosh", 0.55f, 0.05f, 2), (SfxId.ImpactTimber, "SFX_Impact_Timber", 0.85f, 0.03f, 2),
            (SfxId.ImpactStraw, "SFX_Impact_Straw", 0.8f, 0.03f, 2), (SfxId.ImpactStone, "SFX_Impact_Stone", 0.85f, 0.03f, 2),
            (SfxId.ImpactIce, "SFX_Impact_Ice", 0.8f, 0.03f, 2), (SfxId.ImpactMetal, "SFX_Impact_Metal", 0.8f, 0.03f, 2),
            (SfxId.ImpactEarth, "SFX_Impact_Earth", 0.7f, 0.03f, 1), (SfxId.EmbedWood, "SFX_Arrow_EmbedWood", 0.9f, 0.03f, 2),
            (SfxId.BreakTimber, "SFX_Break_Timber", 0.9f, 0.04f, 3), (SfxId.BreakStraw, "SFX_Break_Straw", 0.8f, 0.04f, 3),
            (SfxId.BreakStone, "SFX_Break_Stone", 0.9f, 0.04f, 3), (SfxId.BreakIce, "SFX_Break_Ice", 0.85f, 0.04f, 3),
            (SfxId.BreakCrest, "SFX_Break_Crest", 0.9f, 0.04f, 4), (SfxId.BreakVase, "SFX_Break_Vase", 1f, 0.1f, 5),
            (SfxId.RopeCut, "SFX_Rope_Cut", 0.95f, 0.05f, 4), (SfxId.Splash, "SFX_Splash", 0.8f, 0.08f, 3),
            (SfxId.ObjectiveCleared, "SFX_Objective_Cleared", 0.7f, 0.02f, 4), (SfxId.ChainHit, "SFX_Chain_Hit", 0.7f, 0.05f, 3),
            (SfxId.WinSting, "SFX_Win_Sting", 0.85f, 0.5f, 5), (SfxId.FailSting, "SFX_Fail_Sting", 0.75f, 0.5f, 5),
            (SfxId.ProtectedAlarm, "SFX_Protected_Alarm", 0.6f, 0.5f, 5), (SfxId.UiTap, "SFX_UI_Tap", 0.6f, 0.03f, 2),
            (SfxId.UiStar, "SFX_UI_Star", 0.8f, 0.05f, 4), (SfxId.BodyThud, "SFX_Body_Thud", 0.6f, 0.06f, 1),
        };

        private static SoundLibrary Audio()
        {
            SfxSynth.WriteAll(SfxFolder);
            string musicPath = $"{MusicFolder}/MUS_GW_Loop.wav";
            SfxSynth.WriteMusicLoop(musicPath);
            AssetDatabase.Refresh();

            foreach (string guid in AssetDatabase.FindAssets("t:AudioClip", new[] { SfxFolder, MusicFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                bool music = path.StartsWith(MusicFolder);
                importer.forceToMono = true;
                importer.loadInBackground = music;
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = music ? 0.5f : 0.7f;
                settings.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }

            var library = LoadOrCreate<SoundLibrary>($"{SoRoot}/Audio/SoundLibrary.asset", out _);
            var entries = new List<SoundLibrary.Entry>();
            foreach (var (id, clip, volume, cooldown, priority) in SoundMap)
            {
                var clips = new List<AudioClip>();
                for (int v = 1; v <= 3; v++)
                {
                    var c = AssetDatabase.LoadAssetAtPath<AudioClip>($"{SfxFolder}/{clip}_{v:00}.wav");
                    if (c != null) clips.Add(c);
                }
                entries.Add(new SoundLibrary.Entry { id = id, clips = clips.ToArray(), volume = volume, cooldown = cooldown, priority = priority });
            }
            library.SetEntries(entries, AssetDatabase.LoadAssetAtPath<AudioClip>(musicPath));
            EditorUtility.SetDirty(library);
            return library;
        }

        // ------------------------------------------------------------------ font

        private static TMP_FontAsset Font()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null) return existing;
            var font = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>(FontPath);
            if (font == null)
            {
                Debug.LogWarning("[Arrow Buster] UI font missing at " + FontPath + "; TMP default will be used.");
                return null;
            }
            TMP_FontAsset asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            asset.name = "LilitaOne SDF";
            AssetDatabase.CreateAsset(asset, FontAssetPath);
            foreach (Texture2D atlas in asset.atlasTextures)
            {
                atlas.name = "LilitaOne Atlas";
                AssetDatabase.AddObjectToAsset(atlas, asset);
            }
            asset.material.name = "LilitaOne Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);
            asset.ReadFontAssetDefinition();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        // ------------------------------------------------------------------ gameplay root + scenes

        private static GameObject GameplayRoot(GameObject bowPrefab, GameObject backdropPrefab, ArrowDefinition oak)
        {
            var root = new GameObject("GameplayRoot");
            var controller = root.AddComponent<GameplayController>();
            var tutorial = root.AddComponent<TutorialPromptController>();

            var rig = new GameObject("CameraRig");
            rig.transform.SetParent(root.transform, false);
            var framer = rig.AddComponent<CameraFramer>();
            var camGo = new GameObject("MainCamera") { tag = "MainCamera" };
            camGo.transform.SetParent(rig.transform, false);
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = ArtKit.SkyTop;
            cam.fieldOfView = 30f;
            camGo.AddComponent<AudioListener>();
            camGo.AddComponent<CameraShake>();

            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(root.transform, false);
            sunGo.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.95f, 0.86f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Hard;
            sun.shadowStrength = 0.55f;

            var backdrop = (GameObject)PrefabUtility.InstantiatePrefab(backdropPrefab, root.transform);
            backdrop.name = "Backdrop";

            var bow = (GameObject)PrefabUtility.InstantiatePrefab(bowPrefab, root.transform);
            bow.name = "Bow";

            var arrows = new GameObject("Arrows");
            arrows.transform.SetParent(root.transform, false);
            var spawner = arrows.AddComponent<ArrowSpawner>();
            spawner.SetDefinitions(new[] { oak });

            var levelRoot = new GameObject("LevelRoot");
            levelRoot.transform.SetParent(root.transform, false);

            var uiGo = new GameObject("UI");
            uiGo.transform.SetParent(root.transform, false);
            var ui = uiGo.AddComponent<GameplayUI>();

            var cso = new SerializedObject(controller);
            cso.FindProperty("_bow").objectReferenceValue = bow.GetComponent<BowController>();
            cso.FindProperty("_spawner").objectReferenceValue = spawner;
            cso.FindProperty("_levelRoot").objectReferenceValue = levelRoot.transform;
            cso.FindProperty("_framer").objectReferenceValue = framer;
            cso.ApplyModifiedPropertiesWithoutUndo();

            var uso = new SerializedObject(ui);
            uso.FindProperty("_controller").objectReferenceValue = controller;
            uso.FindProperty("_tutorial").objectReferenceValue = tutorial;
            uso.ApplyModifiedPropertiesWithoutUndo();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/_Project/Prefabs/Roots/GameplayRoot.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void Scenes(GameObject gameplayRoot)
        {
            // Gameplay
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PrefabUtility.InstantiatePrefab(gameplayRoot);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ArtKit.Hex("CFE6FF");
            RenderSettings.ambientEquatorColor = ArtKit.Hex("E6DCC2");
            RenderSettings.ambientGroundColor = ArtKit.Hex("7E6E52");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = ArtKit.SkyBottom;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 230f;
            RenderSettings.skybox = null;
            EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/Gameplay.unity");

            // Boot
            var boot = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootGo = new GameObject("Boot");
            bootGo.AddComponent<BootLoader>();
            var bootCam = new GameObject("BootCamera").AddComponent<Camera>();
            bootCam.clearFlags = CameraClearFlags.SolidColor;
            bootCam.backgroundColor = ArtKit.Hex("2B1E12");
            EditorSceneManager.SaveScene(boot, $"{ScenesFolder}/Boot.unity");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesFolder}/Boot.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Gameplay.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Home.unity", false),
                new EditorBuildSettingsScene($"{ScenesFolder}/WorldMap.unity", false),
            };
            EditorSceneManager.OpenScene($"{ScenesFolder}/Gameplay.unity");
        }
    }
}
