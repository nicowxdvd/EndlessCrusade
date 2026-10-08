using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildScript
{
    public static void BuildDebugApk()
    {
        var output = Environment.GetEnvironmentVariable("EC_APK_PATH") ?? "Builds/EndlessCrusade.apk";
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/_Project/Scenes/Boot.unity", "Assets/_Project/Scenes/Main.unity", "Assets/_Project/Scenes/Level.unity", "Assets/_Project/Scenes/Sandbox/LaneSandbox.unity" },
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.Development
        };
        var report = BuildPipeline.BuildPlayer(options);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
