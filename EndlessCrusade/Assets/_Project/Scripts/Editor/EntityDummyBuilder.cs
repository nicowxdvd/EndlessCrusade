using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class EntityDummyBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string PrefabPath = "Assets/_Project/Prefabs/TestDummy.prefab";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";
    const string PlayerPath = UnitsFolder + "/Unit_TestDummy.asset";
    const string EnemyPath = UnitsFolder + "/Unit_TestDummyEnemy.asset";

    [MenuItem("EC/Sandbox/Build TestDummy Assets")]
    public static void BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(UnitsFolder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Units");

        var player = CreateUnit(PlayerPath, "test_dummy", "Test Dummy", Team.Player);
        var enemy = CreateUnit(EnemyPath, "test_dummy_enemy", "Test Dummy Enemy", Team.Enemy);
        CreatePrefab(player);
        player.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        enemy.prefab = player.prefab;
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(enemy);
        AssetDatabase.SaveAssets();
    }

    public static void SpawnDummies(LaneConfig config)
    {
        BuildAssets();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var player = AssetDatabase.LoadAssetAtPath<UnitDefinition>(PlayerPath);
        var enemy = AssetDatabase.LoadAssetAtPath<UnitDefinition>(EnemyPath);
        Spawn(prefab, player, new Vector3(-4f, config.groundY + 1f, 0f), new Color(0.3f, 0.5f, 1f));
        Spawn(prefab, enemy, new Vector3(4f, config.groundY + 1f, 0f), new Color(1f, 0.3f, 0.3f));
    }

    static void Spawn(GameObject prefab, UnitDefinition definition, Vector3 position, Color color)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = definition.displayName;
        instance.transform.position = position;
        instance.GetComponent<EntityController>().definition = definition;
        var material = new Material(instance.GetComponent<Renderer>().sharedMaterial);
        material.color = color;
        instance.GetComponent<Renderer>().sharedMaterial = material;
    }

    static UnitDefinition CreateUnit(string path, string id, string displayName, Team team)
    {
        var unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
        if (unit == null)
        {
            unit = ScriptableObject.CreateInstance<UnitDefinition>();
            AssetDatabase.CreateAsset(unit, path);
        }
        unit.id = id;
        unit.displayName = displayName;
        unit.team = team;
        EditorUtility.SetDirty(unit);
        return unit;
    }

    static void CreatePrefab(UnitDefinition definition)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "TestDummy";
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        var controller = go.AddComponent<EntityController>();
        controller.definition = definition;
        go.AddComponent<DummyBrain>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
    }
}
