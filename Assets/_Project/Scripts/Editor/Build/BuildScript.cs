using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace StreetLegends.Editor.Build
{
    /// <summary>
    /// Command-line entry points for CI/local builds.
    /// Example: Unity.exe -batchmode -quit -projectPath . -executeMethod StreetLegends.Editor.Build.BuildScript.BuildAndroidDev
    /// </summary>
    public static class BuildScript
    {
        private const string OutputRoot = "Build";

        [MenuItem("Street Legends/Build/Android Dev APK")]
        public static void BuildAndroidDev()
        {
            string output = Path.Combine(OutputRoot, "Android", "StreetLegends-dev.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output));

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Report(BuildPipeline.BuildPlayer(options));
        }

        private static string[] GetEnabledScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            var list = new System.Collections.Generic.List<string>(scenes.Length);
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.enabled)
                {
                    list.Add(scene.path);
                }
            }

            return list.ToArray();
        }

        private static void Report(BuildReport report)
        {
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] Succeeded: {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB) in {summary.totalTime.TotalSeconds:F0}s");
                return;
            }

            Debug.LogError($"[Build] {summary.result}: {summary.totalErrors} errors");
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }
    }
}
