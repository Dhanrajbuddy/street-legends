using System.IO;
using StreetLegends.Bootstrap;
using StreetLegends.Core;
using StreetLegends.Data;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
        private const string RenderingDir = ProjectRoot + "/Settings/Rendering";
        public const string UrpAssetPath = RenderingDir + "/URP-Asset.asset";
        public const string UrpRendererPath = RenderingDir + "/URP-Renderer.asset";

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
            EnsureTags();
            EnsureRenderPipeline();
            CreateEnvironmentConfig();
            CreateScenes();
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

        public static readonly string[] RequiredTags = { "Ball" };

        public static void EnsureTags()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;

            var so = new SerializedObject(assets[0]);
            SerializedProperty tags = so.FindProperty("tags");
            foreach (string tag in RequiredTags)
            {
                bool exists = false;
                for (int i = 0; i < tags.arraySize; i++)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag) { exists = true; break; }
                }
                if (!exists)
                {
                    tags.InsertArrayElementAtIndex(tags.arraySize);
                    tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// URP requires a ScriptableRendererData on the pipeline asset. Without it the build-time shader
        /// stripper removes every URP/UI shader variant and the device renders magenta. This is idempotent
        /// and repairs an existing pipeline asset that has an empty renderer list.
        /// </summary>
        public static void EnsureRenderPipeline()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpRendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, UrpRendererPath);
                ResourceReloader.ReloadAllNullIn(renderer, UniversalRenderPipelineAsset.packagePath);
                EditorUtility.SetDirty(renderer);
            }

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, UrpAssetPath);
            }
            else
            {
                var so = new SerializedObject(pipeline);
                SerializedProperty list = so.FindProperty("m_RendererDataList");
                list.arraySize = 1;
                list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                so.FindProperty("m_DefaultRendererIndex").intValue = 0;
                so.ApplyModifiedPropertiesWithoutUndo();
                ResourceReloader.ReloadAllNullIn(pipeline, UniversalRenderPipelineAsset.packagePath);
                EditorUtility.SetDirty(pipeline);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;

            int previous = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipeline;
            }
            QualitySettings.SetQualityLevel(previous, false);

            AssetDatabase.SaveAssets();
        }

        private static void CreateEnvironmentConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(DevEnvPath) != null)
            {
                return;
            }

            var config = ScriptableObject.CreateInstance<EnvironmentConfig>();
            AssetDatabase.CreateAsset(config, DevEnvPath);
            AssetDatabase.SaveAssets();
        }

        private static void CreateScenes()
        {
            string bootPath = $"{ScenesDir}/{SceneNames.Boot}.unity";
            Scene boot;
            GameBootstrap bootstrap;
            if (File.Exists(bootPath))
            {
                boot = EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
                bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
                if (bootstrap == null)
                {
                    bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
                }
            }
            else
            {
                boot = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            }

            // Load after the scene switch: scene loading can unload asset references held before it.
            var devEnv = AssetDatabase.LoadAssetAtPath<EnvironmentConfig>(DevEnvPath);
            if (devEnv == null)
            {
                throw new System.InvalidOperationException($"[Phase0Setup] EnvironmentConfig missing at {DevEnvPath}");
            }

            var so = new SerializedObject(bootstrap);
            so.FindProperty("_environment").objectReferenceValue = devEnv;
            so.ApplyModifiedPropertiesWithoutUndo();
            so.Update();
            if (so.FindProperty("_environment").objectReferenceValue == null)
            {
                throw new System.InvalidOperationException("[Phase0Setup] Failed to assign EnvironmentConfig to GameBootstrap.");
            }

            EditorSceneManager.MarkSceneDirty(boot);
            EditorSceneManager.SaveScene(boot, bootPath);

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
