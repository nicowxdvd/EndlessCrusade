using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HudPreview
{
    const string ScenePath = "Assets/_Project/Scenes/Sandbox/LaneSandbox.unity";
    const string SizeName = "EC 20:9 (2400x1080)";

    [MenuItem("EC/Sandbox/Open HUD 20-9 Preview")]
    public static void Open()
    {
        EditorSceneManager.OpenScene(ScenePath);
        EditorApplication.delayCall += ApplyGameViewSize;
    }

    static void ApplyGameViewSize()
    {
        var editorAssembly = typeof(Editor).Assembly;
        var sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
        var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instance = singletonType.GetProperty("instance").GetValue(null);
        var group = sizesType.GetMethod("GetGroup").Invoke(instance, new object[] { (int)GameViewSizeGroupType.Standalone });
        var groupType = group.GetType();

        var index = IndexOf(group, groupType);
        if (index < 0)
        {
            var sizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
            var kindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");
            var size = Activator.CreateInstance(sizeType, Enum.Parse(kindType, "FixedResolution"), 2400, 1080, SizeName);
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
            index = IndexOf(group, groupType);
        }

        var gameViewType = editorAssembly.GetType("UnityEditor.GameView");
        var gameView = EditorWindow.GetWindow(gameViewType);
        gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(gameView, index);
        gameView.Repaint();
    }

    static int IndexOf(object group, Type groupType)
    {
        var names = (string[])groupType.GetMethod("GetDisplayTexts").Invoke(group, null);
        for (var i = 0; i < names.Length; i++)
            if (names[i].StartsWith(SizeName))
                return i;
        return -1;
    }
}
