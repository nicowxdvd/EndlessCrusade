using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using EC.Core;
using UnityEngine;

public static class BuildScript
{
    static readonly string[] Scenes =
    {
        "Assets/_Project/Scenes/Boot.unity",
        "Assets/_Project/Scenes/Main.unity",
        "Assets/_Project/Scenes/CampaignMap.unity",
        "Assets/_Project/Scenes/Emporium.unity",
        "Assets/_Project/Scenes/Equipment.unity",
        "Assets/_Project/Scenes/Pachinko.unity",
        "Assets/_Project/Scenes/Level.unity",
        "Assets/_Project/Scenes/Sandbox/LaneSandbox.unity"
    };

    public static void BuildDebugApk()
    {
        var output = Environment.GetEnvironmentVariable("EC_APK_PATH") ?? "Builds/EndlessCrusade.apk";
        var options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.Development
        };
        var report = BuildPipeline.BuildPlayer(options);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    public static void BuildRelease()
    {
        var keystorePath = Require("EC_KEYSTORE_PATH");
        var keystorePass = Require("EC_KEYSTORE_PASS");
        var keyAlias = Require("EC_KEY_ALIAS");
        var keyPass = Require("EC_KEY_PASS");
        var output = Environment.GetEnvironmentVariable("EC_AAB_PATH") ?? "Builds/EndlessCrusade.aab";
        var version = Environment.GetEnvironmentVariable("EC_VERSION") ?? PlayerSettings.bundleVersion;

        PlayerSettings.bundleVersion = version;
        PlayerSettings.Android.bundleVersionCode = VersionCodes.ToCode(version);
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = keystorePath;
        PlayerSettings.Android.keystorePass = keystorePass;
        PlayerSettings.Android.keyaliasName = keyAlias;
        PlayerSettings.Android.keyaliasPass = keyPass;
        EditorUserBuildSettings.buildAppBundle = true;

        var options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = output,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("[Release] " + report.summary.result + " versión " + version + " código " + PlayerSettings.Android.bundleVersionCode);
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }

    static string Require(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("[Release] Falta la variable de entorno " + name);
            EditorApplication.Exit(2);
            throw new InvalidOperationException("Falta " + name);
        }
        return value;
    }
}
