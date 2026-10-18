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
        /// <summary>Idempotent project setup followed by the Windows build (one headless session).</summary>
        public static void SetupAndBuild()
        {
            ProjectSetup.Run();
            BuildWindows();
        }

        public static void BuildWindows()
        {
            string path = GetArg("-buildPath");
            if (string.IsNullOrEmpty(path))
            {
                // Next to the repo (C:\Dev\12th-Stamp -> C:\Dev\12th-Stamp-builds): outside source control, and not under
                // AppData, which a packaged app such as the Claude desktop app redirects into its own private storage,
                // where Explorer, Unity Hub and the desktop shortcuts can't see it.
                var repo = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
                var root = Path.Combine(Path.GetDirectoryName(repo), Path.GetFileName(repo) + "-builds", "Builds", "Windows");
                path = Path.Combine(root, "TheTwelfthStamp.exe");
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (!ApplyIcon())
            {
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

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
            if (s.result == BuildResult.Succeeded) CopyPlayerFiles(Path.GetDirectoryName(path));

            if (Application.isBatchMode)
                EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Ships the shortcut installer, its icon and the player README next to the .exe.</summary>
        static void CopyPlayerFiles(string buildDir)
        {
            var src = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "Tools", "install"));
            foreach (var name in new[] { "Install-Shortcuts.ps1", "Install Shortcuts.cmd", "TheTwelfthStamp.ico", "README.txt" })
            {
                var from = Path.Combine(src, name);
                if (!File.Exists(from)) { Debug.Log("[TS-Build] Missing player file: " + from); continue; }
                File.Copy(from, Path.Combine(buildDir, name), true);
                Debug.Log("[TS-Build] Copied " + name);
            }
        }

        /// <summary>
        /// The Windows executable's icon (also the window, taskbar and Alt-Tab icon) from the approved key art: one
        /// texture per size Unity asks for, made by Tools/branding/make_branding.py (small sizes use the thumbnail crop,
        /// because the title is unreadable there). A missing icon fails the build instead of shipping a stand-in.
        /// </summary>
        static bool ApplyIcon()
        {
            var sizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone, IconKind.Application);
            var icons = new Texture2D[sizes.Length];
            for (int i = 0; i < sizes.Length; i++)
            {
                var path = "Assets/Branding/Icons/icon_" + sizes[i] + ".png";
                icons[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (icons[i] == null)
                {
                    Debug.LogError("[TS-Build] ERROR: missing application icon " + path + " (run Tools/branding/make_branding.py)");
                    return false;
                }
            }
            PlayerSettings.SetIcons(NamedBuildTarget.Standalone, icons, IconKind.Application);
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icons[0] }, IconKind.Any); // Player Settings > Default Icon
            Debug.Log("[TS-Build] Application icon set from Assets/Branding/Icons (sizes " + string.Join(", ", sizes) + ")");
            return true;
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
