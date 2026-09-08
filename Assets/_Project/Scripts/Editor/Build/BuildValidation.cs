using System.Collections.Generic;
using StreetLegends.Bootstrap;
using StreetLegends.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StreetLegends.Editor.Build
{
    /// <summary>
    /// Fails the player build early when project configuration would produce a non-functional APK.
    /// Every check here corresponds to a failure that previously reached a device.
    /// </summary>
    public sealed class BuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            List<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new BuildFailedException("[BuildValidation] " + string.Join("\n", errors));
            }
        }

        public static List<string> Validate()
        {
            var errors = new List<string>();
            ValidateRenderPipeline(errors);
            ValidateBootScene(errors);
            ValidateBuildScenes(errors);
            ValidateTags(errors);
            return errors;
        }

        public static void ValidateTags(List<string> errors)
        {
            var defined = new HashSet<string>(UnityEditorInternal.InternalEditorUtility.tags);
            foreach (string tag in Setup.Phase0Setup.RequiredTags)
            {
                if (!defined.Contains(tag))
                {
                    errors.Add($"Required tag '{tag}' is not defined in TagManager (GoalTrigger.CompareTag would never match).");
                }
            }
        }

        public static void ValidateRenderPipeline(List<string> errors)
        {
            var pipeline = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (pipeline == null)
            {
                errors.Add("GraphicsSettings.defaultRenderPipeline is not a UniversalRenderPipelineAsset.");
                return;
            }

            var so = new SerializedObject(pipeline);
            SerializedProperty list = so.FindProperty("m_RendererDataList");
            bool hasRenderer = false;
            for (int i = 0; list != null && i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue != null)
                {
                    hasRenderer = true;
                    break;
                }
            }

            if (!hasRenderer)
            {
                errors.Add($"URP asset '{pipeline.name}' has no Renderer Data assigned. All URP/UI shaders would be stripped (magenta on device).");
            }
        }

        public static void ValidateBootScene(List<string> errors)
        {
            string bootPath = $"Assets/_Project/Scenes/{SceneNames.Boot}.unity";
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootPath);
            if (sceneAsset == null)
            {
                errors.Add($"Boot scene missing at {bootPath}.");
                return;
            }

            SceneSetup[] previous = EditorSceneManager.GetSceneManagerSetup();
            var scene = EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Additive);
            try
            {
                GameBootstrap bootstrap = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    bootstrap = root.GetComponentInChildren<GameBootstrap>(true);
                    if (bootstrap != null) break;
                }

                if (bootstrap == null)
                {
                    errors.Add("Boot scene has no GameBootstrap component.");
                    return;
                }

                var so = new SerializedObject(bootstrap);
                if (so.FindProperty("_environment").objectReferenceValue == null)
                {
                    errors.Add("GameBootstrap._environment (EnvironmentConfig) is not assigned in Boot scene.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
                if (previous.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previous);
                }
            }
        }

        public static void ValidateBuildScenes(List<string> errors)
        {
            string[] expected = { SceneNames.Boot, SceneNames.MainMenu, SceneNames.Match };
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes.Length < expected.Length)
            {
                errors.Add($"Build Settings has {scenes.Length} scenes; expected {expected.Length}.");
                return;
            }

            for (int i = 0; i < expected.Length; i++)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(scenes[i].path);
                if (!scenes[i].enabled || name != expected[i])
                {
                    errors.Add($"Build Settings scene[{i}] is '{name}' (enabled={scenes[i].enabled}); expected '{expected[i]}'.");
                }
            }
        }
    }
}
