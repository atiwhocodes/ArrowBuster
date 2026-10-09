using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ArrowBuster.Editor
{
    /// <summary>
    /// One-click mobile project configuration for Arrow Buster.
    /// Menu: Arrow Buster ▸ Setup. Safe to run more than once.
    /// </summary>
    public static class ProjectSetup
    {
        public const string ProductName = "Arrow Buster";
        public const string CompanyName = "Attila";
        public const string BundleId = "com.attila.arrowbuster"; // change before first store upload

        private const string ScenesFolder = "Assets/_Project/Scenes";
        private static readonly string[] SceneNames = { "Boot", "Home", "WorldMap", "Gameplay" };

        [MenuItem("Arrow Buster/Setup/1. Apply Mobile Player Settings", priority = 1)]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = ProductName;
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.bundleVersion = "0.1.0";

            // Portrait only (GDD §12: portrait, 9:16 reference).
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // Android: IL2CPP + ARM64 is required for Google Play.
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, BundleId);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26; // Unity 6.6 minimum (Android 8.0)
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 1;

            // iOS (built on a Mac with Xcode).
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, BundleId);
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.buildNumber = "1";

            AssetDatabase.SaveAssets();
            Debug.Log($"[Arrow Buster] Player settings applied: {ProductName} ({BundleId}), portrait, IL2CPP/ARM64.");
        }

        [MenuItem("Arrow Buster/Setup/2. Create Scenes + Build List", priority = 2)]
        public static void CreateScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(ScenesFolder);

            var buildScenes = new List<EditorBuildSettingsScene>();
            foreach (var sceneName in SceneNames)
            {
                string path = $"{ScenesFolder}/{sceneName}.unity";
                if (!File.Exists(path))
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, path);
                    Debug.Log($"[Arrow Buster] Created scene {path}");
                }
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            EditorSceneManager.OpenScene($"{ScenesFolder}/Gameplay.unity");
            AssetDatabase.Refresh();
            Debug.Log("[Arrow Buster] Build scene list: Boot, Home, WorldMap, Gameplay.");
        }

        [MenuItem("Arrow Buster/Setup/3. Switch Platform to Android", priority = 3)]
        public static void SwitchToAndroid()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        [MenuItem("Arrow Buster/Setup/Run All (1 + 2)", priority = 20)]
        public static void RunAll()
        {
            ApplyPlayerSettings();
            CreateScenes();
        }
    }
}
