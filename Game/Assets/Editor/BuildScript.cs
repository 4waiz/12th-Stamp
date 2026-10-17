using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TwelfthStamp.EditorTools
{
    /// <summary>
    /// Headless Windows build:
    /// Unity.exe -batchmode -quit -projectPath Game -executeMethod TwelfthStamp.EditorTools.BuildScript.BuildWindows -buildPath "C:\...\TheTwelfthStamp.exe" [-devBuild]
    /// </summary>
    public static class BuildScript
    {
        public static void BuildWindows()
        {
            string path = GetArg("-buildPath");
            if (string.IsNullOrEmpty(path))
            {
                var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TheTwelfthStamp", "Builds", "Windows");
                path = Path.Combine(root, "TheTwelfthStamp.exe");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            ApplyIcon();

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = HasArg("-devBuild") ? BuildOptions.Development : BuildOptions.None
            };

            Debug.Log("[TS-Build] Building to " + path);
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            Debug.Log($"[TS-Build] Result={s.result} errors={s.totalErrors} warnings={s.totalWarnings} size={s.totalSize / (1024 * 1024)}MB time={s.totalTime}");
            foreach (var step in report.steps)
                foreach (var m in step.messages)
                    if (m.type == LogType.Error || m.type == LogType.Exception)
                        Debug.Log("[TS-Build] ERROR: " + m.content);

            if (Application.isBatchMode)
                EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }

        static void ApplyIcon()
        {
            const string iconPath = "Assets/Resources/Art/icon_app.png";
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
            if (tex == null)
            {
                Debug.Log("[TS-Build] No icon yet at " + iconPath);
                return;
            }
            var sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Application);
            var icons = Enumerable.Repeat(tex, sizes.Length).ToArray();
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Application);
            Debug.Log("[TS-Build] Application icon set from " + iconPath);
        }

        static string GetArg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == name) return args[i + 1];
            return null;
        }

        static bool HasArg(string name) => Environment.GetCommandLineArgs().Contains(name);
    }
}
