using System.IO;
using EC.Core;
using UnityEditor;
using UnityEngine;

public static class AudioBuilder
{
    const string ResourcesFolder = "Assets/_Project/Resources";
    const string AudioFolder = "Assets/_Project/Audio";
    const string CuesFolder = AudioFolder + "/Cues";
    const string LibraryPath = ResourcesFolder + "/AudioLibrary.asset";
    const string LicensesPath = AudioFolder + "/LICENSES.md";

    static readonly string[] SfxIds = { "hit", "death", "attack_sword", "attack_whip", "boss_roar", "summon", "ability", "coin", "victory", "defeat", "ui_click" };
    static readonly string[] MusicIds = { "music_main", "music_map", "music_ch1", "music_ch2", "music_ch3", "music_ch4", "music_boss", "music_victory" };

    [MenuItem("EC/Level/Build Audio Library")]
    public static void Build()
    {
        EnsureFolder("Assets/_Project", "Resources");
        EnsureFolder("Assets/_Project", "Audio");
        EnsureFolder(AudioFolder, "Cues");

        var cues = new System.Collections.Generic.List<AudioCue>();
        foreach (var id in SfxIds)
            cues.Add(Cue(id, false, 0.05f));
        foreach (var id in MusicIds)
            cues.Add(Cue(id, true, 0f));
        cues.Add(Cue("ambient_rain", true, 0f));

        var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<AudioLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }
        library.cues = cues.ToArray();
        EditorUtility.SetDirty(library);

        if (!File.Exists(LicensesPath))
        {
            File.WriteAllText(LicensesPath, "# Licencias de audio\n\nRegistrar aquí cada archivo de audio agregado al proyecto.\n\n| Archivo | Autor | Licencia | Fuente |\n| --- | --- | --- | --- |\n");
            AssetDatabase.ImportAsset(LicensesPath);
        }
        AssetDatabase.SaveAssets();
    }

    static AudioCue Cue(string id, bool loop, float minInterval)
    {
        var path = CuesFolder + "/Cue_" + id + ".asset";
        var cue = AssetDatabase.LoadAssetAtPath<AudioCue>(path);
        if (cue == null)
        {
            cue = ScriptableObject.CreateInstance<AudioCue>();
            AssetDatabase.CreateAsset(cue, path);
        }
        cue.id = id;
        cue.loop = loop;
        cue.minInterval = minInterval;
        EditorUtility.SetDirty(cue);
        return cue;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }
}
