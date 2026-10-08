using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class QualityBuilder
{
    const string SourcePipeline = "Assets/_Project/Settings/Mobile_RPAsset.asset";
    const string ResourcesFolder = "Assets/_Project/Resources";
    const string QualityFolder = ResourcesFolder + "/Quality";
    const string LevelsFolder = "Assets/_Project/ScriptableObjects/Levels";
    const string StressPath = LevelsFolder + "/Level_Stress.asset";
    const string LevelScenePath = "Assets/_Project/Scenes/Level.unity";

    [MenuItem("EC/Quality/Build Tier Pipelines")]
    public static void BuildPipelines()
    {
        EnsureFolder("Assets/_Project", "Resources");
        EnsureFolder(ResourcesFolder, "Quality");

        foreach (QualityTier tier in System.Enum.GetValues(typeof(QualityTier)))
        {
            var path = QualityFolder + "/URP_" + tier + ".asset";
            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path) == null)
                AssetDatabase.CopyAsset(SourcePipeline, path);

            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("m_RenderScale").floatValue = QualityPolicy.RenderScale(tier);
            serialized.FindProperty("m_MSAA").intValue = QualityPolicy.Msaa(tier);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("EC/Quality/Build Stress Level")]
    public static LevelDefinition BuildStressLevel()
    {
        EnemyBaseBuilder.BuildAssets();
        ExtraEnemiesBuilder.BuildAssets();
        EnsureFolder("Assets/_Project/ScriptableObjects", "Levels");

        var warg = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/ScriptableObjects/Units/Enemy_Warg.asset");
        var bat = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/ScriptableObjects/Units/Enemy_Bat.asset");
        var goblin = AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/ScriptableObjects/Units/Enemy_goblin.asset");

        var wave = LevelBuilder.WaveFor("stress", 1, 0f, null, 0f,
            LevelBuilder.Entry(warg, 25, 0.25f, 0f),
            LevelBuilder.Entry(bat, 20, 0.3f, 0f),
            LevelBuilder.Entry(goblin, 15, 0.4f, 0f));

        var level = LevelBuilder.LoadOrCreate<LevelDefinition>(StressPath);
        level.id = "level_stress";
        level.displayName = "Nivel de estrés";
        level.baseDefinition = AssetDatabase.LoadAssetAtPath<BaseDefinition>("Assets/_Project/ScriptableObjects/Bases/Base_CabinRuins.asset");
        level.waves = new[] { wave };
        level.troopsEnabled = true;
        level.tutorial = false;
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        return level;
    }

    [MenuItem("EC/Quality/Run Stress Level")]
    public static void RunStress()
    {
        var level = BuildStressLevel();
        LevelSession.Current = level;
        EditorSceneManager.OpenScene(LevelScenePath);
        var harness = new GameObject("StressHarness").AddComponent<StressHarness>();
        harness.summoner = Object.FindFirstObjectByType<TroopSummoner>();
        EditorApplication.isPlaying = true;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }
}
