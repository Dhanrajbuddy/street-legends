using System.IO;
using StreetLegends.Bootstrap;
using StreetLegends.Core;
using StreetLegends.Data;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace StreetLegends.Editor.Setup
{
    /// <summary>
    /// Idempotent one-shot setup for Phase 0. Safe to re-run. Invoked headlessly via
    /// -executeMethod StreetLegends.Editor.Setup.Phase0Setup.Run or from the menu.
    /// </summary>
    public static class Phase0Setup
    {
        private const string ProjectRoot = "Assets/_Project";
        private const string ScenesDir = ProjectRoot + "/Scenes";
        private const string EnvDir = ProjectRoot + "/Data/Environment";
        private const string DevEnvPath = EnvDir + "/EnvironmentConfig_Dev.asset";

        private static readonly string[] Folders =
        {
            ProjectRoot + "/Art/Materials",
            ProjectRoot + "/Art/Models",
            ProjectRoot + "/Art/Textures",
            ProjectRoot + "/Art/Animations",
            ProjectRoot + "/Audio/Music",
            ProjectRoot + "/Audio/Sfx",
            ProjectRoot + "/Data/Characters",
            ProjectRoot + "/Data/Abilities",
            ProjectRoot + "/Data/Economy",
            ProjectRoot + "/Data/Match",
            EnvDir,
            ProjectRoot + "/Prefabs/Gameplay",
            ProjectRoot + "/Prefabs/UI",
            ProjectRoot + "/Prefabs/Systems",
            ScenesDir,
            ProjectRoot + "/Settings/Input",
            ProjectRoot + "/Settings/Rendering",
            "Assets/ThirdParty"
        };

        [MenuItem("Street Legends/Setup/Run Phase 0 Setup")]
        public static void Run()
        {
            CreateFolders();
            ApplyProjectSettings();
            EnvironmentConfig devEnv = CreateEnvironmentConfig();
            CreateScenes(devEnv);
            RegisterScenesInBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase0Setup] Complete.");
        }

        private static void CreateFolders()
        {
            foreach (string folder in Folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(Path.Combine(folder, ".gitkeep"), string.Empty);
                }
            }

            AssetDatabase.Refresh();
        }

        private static void ApplyProjectSettings()
        {
            PlayerSettings.companyName = "StreetForge Games";
            PlayerSettings.productName = "Street Legends";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.streetforge.streetlegends");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.streetforge.streetlegends");

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            PlayerSettings.colorSpace = ColorSpace.Linear;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, ApiCompatibilityLevel.NET_Standard);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.iOS.targetOSVersionString = "13.0";

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, "SL_DEV");
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.iOS, "SL_DEV");
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, "SL_DEV");

            EditorSettings.serializationMode = SerializationMode.ForceText;
            VersionControlSettings.mode = "Visible Meta Files";

            Time.fixedDeltaTime = 0.02f;
        }

        private static EnvironmentConfig CreateEnvironmentConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(DevEnvPath);
            if (existing != null)
            {
                return existing;
            }

            var config = ScriptableObject.CreateInstance<EnvironmentConfig>();
            AssetDatabase.CreateAsset(config, DevEnvPath);
            return config;
        }

        private static void CreateScenes(EnvironmentConfig devEnv)
        {
            string bootPath = $"{ScenesDir}/{SceneNames.Boot}.unity";
            if (!File.Exists(bootPath))
            {
                Scene boot = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var go = new GameObject("GameBootstrap");
                var bootstrap = go.AddComponent<GameBootstrap>();
                var so = new SerializedObject(bootstrap);
                so.FindProperty("_environment").objectReferenceValue = devEnv;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.SaveScene(boot, bootPath);
            }

            CreateDefaultSceneIfMissing($"{ScenesDir}/{SceneNames.MainMenu}.unity");
            CreateDefaultSceneIfMissing($"{ScenesDir}/{SceneNames.Match}.unity");
        }

        private static void CreateDefaultSceneIfMissing(string path)
        {
            if (File.Exists(path))
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void RegisterScenesInBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesDir}/{SceneNames.Boot}.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/{SceneNames.MainMenu}.unity", true),
                new EditorBuildSettingsScene($"{ScenesDir}/{SceneNames.Match}.unity", true)
            };
        }
    }
}
