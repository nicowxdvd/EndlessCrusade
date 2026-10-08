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
    const string CampaignPath = "Assets/_Project/ScriptableObjects/Campaign/Campaign_Main.asset";
    public const string SquirePath = UnitsFolder + "/Troop_Squire.asset";

    enum Role { Melee, Ranged, Healer }

    [MenuItem("EC/Sandbox/Build Troops")]
    public static TroopDefinition[] BuildAssets()
    {
        var bolt = CreateBoltPrefab();
        return new[]
        {
            Build("Squire", "squire", "Escudero", Role.Melee, 120, 10, 1.2f, 1f, 2.2f, 30, 6f, "", new Color(0.3f, 0.5f, 0.75f), null),
            Build("Peasant", "peasant", "Campesino", Role.Melee, 50, 6, 1.0f, 1f, 2.6f, 15, 3f, "ch2_aldea", new Color(0.6f, 0.5f, 0.3f), null),
            Build("Crossbowman", "crossbowman", "Ballestero", Role.Ranged, 60, 14, 8.0f, 1.5f, 2.0f, 45, 8f, "ch2_aldea", new Color(0.35f, 0.65f, 0.4f), bolt),
            Build("Priest", "priest", "Sacerdote", Role.Healer, 70, 0, 4.0f, 1f, 2.0f, 60, 14f, "ch3_bosque", new Color(0.9f, 0.9f, 0.95f), null),
            Build("Paladin", "paladin", "Paladín", Role.Melee, 400, 22, 1.4f, 1.2f, 1.8f, 90, 20f, "ch3_bosque", new Color(0.85f, 0.75f, 0.3f), null)
        };
    }

    public static void SpawnSummoner(LaneConfig config, LevelDefinition level)
    {
        var summoner = CreateSummoner(config, level, Object.FindFirstObjectByType<PoolService>());
        summoner.baseTarget = Object.FindFirstObjectByType<BaseStructure>().transform;
    }

    public static TroopSummoner CreateSummoner(LaneConfig config, LevelDefinition level, PoolService pool)
    {
        var troops = BuildAssets();
        var go = new GameObject("TroopSummoner");
        var leadership = go.AddComponent<LeadershipComponent>();
        var summoner = go.AddComponent<TroopSummoner>();
        summoner.leadership = leadership;
        summoner.level = level;
        summoner.campaign = AssetDatabase.LoadAssetAtPath<CampaignDefinition>(CampaignPath);
        summoner.troops = troops;
        summoner.lane = config;
        summoner.pool = pool;
        return summoner;
    }

    static TroopDefinition Build(string key, string id, string displayName, Role role, int health, int damage, float range, float cooldown, float speed, int cost, float summonCooldown, string unlockChapter, Color color, GameObject bolt)
    {
        var path = UnitsFolder + "/Troop_" + key + ".asset";
        var troop = AssetDatabase.LoadAssetAtPath<TroopDefinition>(path);
        if (troop == null)
        {
            troop = ScriptableObject.CreateInstance<TroopDefinition>();
            AssetDatabase.CreateAsset(troop, path);
        }
        troop.id = id;
        troop.displayName = displayName;
        troop.team = Team.Player;
        troop.maxHealth = health;
        troop.attackDamage = damage;
        troop.moveSpeed = speed;
        troop.attackRange = range;
        troop.attackCooldown = cooldown;
        troop.leadershipCost = cost;
        troop.summonCooldown = summonCooldown;
        troop.unlockChapterId = unlockChapter;
        troop.prefab = CreatePrefab("Troop_" + key, troop, role, color, bolt);
        EditorUtility.SetDirty(troop);
        AssetDatabase.SaveAssets();
        return troop;
    }

    static GameObject CreatePrefab(string name, TroopDefinition definition, Role role, Color color, GameObject bolt)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial(name, color);
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        if (role == Role.Ranged)
            go.AddComponent<RangedAttackComponent>().boltPrefab = bolt;
        else
            go.AddComponent<AttackComponent>();
        var brain = go.AddComponent<TroopBrain>();
        if (role == Role.Healer)
        {
            brain.support = true;
            var aura = go.AddComponent<HealAuraComponent>();
            aura.amount = 10;
            aura.interval = 2f;
            aura.radius = 4f;
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    public static GameObject CreateBoltPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Bolt";
        go.transform.localScale = new Vector3(0.5f, 0.08f, 0.08f);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial("Bolt", new Color(0.9f, 0.85f, 0.6f));
        go.AddComponent<Bolt>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/Bolt.prefab");
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
