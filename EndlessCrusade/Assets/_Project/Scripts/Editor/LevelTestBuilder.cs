using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class LevelTestBuilder
{
    const string LevelsFolder = "Assets/_Project/ScriptableObjects/Levels";
    const string WargPath = "Assets/_Project/ScriptableObjects/Units/Enemy_Warg.asset";
    const string BatPath = "Assets/_Project/ScriptableObjects/Units/Enemy_Bat.asset";
    const string BasePath = "Assets/_Project/ScriptableObjects/Bases/Base_CabinRuins.asset";
    const string LevelPath = LevelsFolder + "/Level_Test.asset";

    [MenuItem("EC/Sandbox/Build Level Test")]
    public static LevelDefinition BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(LevelsFolder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Levels");

        var warg = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(WargPath);
        var bat = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BatPath);
        var waves = new[]
        {
            LoadOrCreateWave(1, 4f, Entry(warg, 3, 2f, 0f)),
            LoadOrCreateWave(2, 4f, Entry(warg, 3, 1.5f, 0f), Entry(bat, 3, 1f, 2f)),
            LoadOrCreateWave(3, 0f, Entry(warg, 4, 1f, 0f), Entry(bat, 5, 0.8f, 1f))
        };

        var level = LoadOrCreate<LevelDefinition>(LevelPath);
        level.id = "level_test";
        level.displayName = "Nivel de prueba";
        level.baseDefinition = AssetDatabase.LoadAssetAtPath<BaseDefinition>(BasePath);
        level.waves = waves;
        level.troopsEnabled = true;
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        return level;
    }

    public static void SpawnController()
    {
        var level = BuildAssets();
        var controller = new GameObject("WaveController").AddComponent<WaveController>();
        controller.level = level;
        controller.spawner = Object.FindFirstObjectByType<EnemySpawner>();
        TroopBuilder.SpawnSummoner(AssetDatabase.LoadAssetAtPath<LaneConfig>("Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset"), level);
    }

    static SpawnEntry Entry(EnemyDefinition enemy, int count, float interval, float startDelay)
    {
        return new SpawnEntry { enemy = enemy, count = count, interval = interval, startDelay = startDelay };
    }

    static WaveDefinition LoadOrCreateWave(int number, float delayBeforeNext, params SpawnEntry[] entries)
    {
        var wave = LoadOrCreate<WaveDefinition>(LevelsFolder + "/Wave_Test_" + number + ".asset");
        wave.entries = entries;
        wave.delayBeforeNext = delayBeforeNext;
        EditorUtility.SetDirty(wave);
        return wave;
    }

    static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
        }
        return asset;
    }
}
