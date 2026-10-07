using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class TroopBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string PrefabsFolder = "Assets/_Project/Prefabs";
    const string MaterialsFolder = "Assets/_Project/Art/Materials";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";
    public const string SquirePath = UnitsFolder + "/Troop_Squire.asset";

    [MenuItem("EC/Sandbox/Build Troops")]
    public static TroopDefinition BuildAssets()
    {
        var troop = AssetDatabase.LoadAssetAtPath<TroopDefinition>(SquirePath);
        if (troop == null)
        {
            troop = ScriptableObject.CreateInstance<TroopDefinition>();
            AssetDatabase.CreateAsset(troop, SquirePath);
        }
        troop.id = "squire";
        troop.displayName = "Escudero";
        troop.team = Team.Player;
        troop.maxHealth = 120;
        troop.attackDamage = 10;
        troop.moveSpeed = 2.2f;
        troop.attackRange = 1.2f;
        troop.attackCooldown = 1f;
        troop.leadershipCost = 30;
        troop.summonCooldown = 6f;
        troop.prefab = CreatePrefab("Troop_Squire", troop);
        EditorUtility.SetDirty(troop);
        AssetDatabase.SaveAssets();
        return troop;
    }

    public static void SpawnSummoner(LaneConfig config, LevelDefinition level)
    {
        var troop = BuildAssets();
        var go = new GameObject("TroopSummoner");
        var leadership = go.AddComponent<LeadershipComponent>();
        var summoner = go.AddComponent<TroopSummoner>();
        summoner.leadership = leadership;
        summoner.level = level;
        summoner.troops = new[] { troop };
        summoner.lane = config;
        summoner.pool = Object.FindFirstObjectByType<PoolService>();
        summoner.baseTarget = Object.FindFirstObjectByType<BaseStructure>().transform;
    }

    static GameObject CreatePrefab(string name, TroopDefinition definition)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial(name, new Color(0.3f, 0.5f, 0.75f));
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        go.AddComponent<TroopBrain>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static Material ColorMaterial(string name, Color color)
    {
        var path = MaterialsFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }
}
