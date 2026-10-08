using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class ExtraEnemiesBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string PrefabsFolder = "Assets/_Project/Prefabs";
    const string MaterialsFolder = "Assets/_Project/Art/Materials";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";

    [MenuItem("EC/Sandbox/Build Extra Enemies")]
    public static void BuildAssets()
    {
        Build("goblin", "Goblin", 25, 5, 3.0f, CreatureTag.None, new Vector3(0.7f, 0.7f, 0.7f), new Color(0.3f, 0.6f, 0.25f), null);
        Build("imp", "Duende", 20, 7, 4.0f, CreatureTag.None, new Vector3(0.6f, 0.6f, 0.6f), new Color(0.85f, 0.45f, 0.15f), go => go.AddComponent<DodgeModule>().dodgeChance = 0.25f);
        Build("vampire", "Vampiro", 90, 15, 2.2f, CreatureTag.Undead, new Vector3(0.9f, 1.1f, 0.9f), new Color(0.6f, 0.05f, 0.1f), go =>
        {
            var blink = go.AddComponent<BlinkModule>();
            blink.distance = 4f;
            blink.interval = 6f;
            go.AddComponent<LifestealModule>().fraction = 0.3f;
        });
        Build("werewolf", "Licantropo", 150, 20, 2.8f, CreatureTag.Beast, new Vector3(1.1f, 1.2f, 1.1f), new Color(0.4f, 0.4f, 0.45f), go =>
        {
            var rage = go.AddComponent<RageModule>();
            rage.healthThreshold = 0.5f;
            rage.speedBonus = 0.4f;
        });
        Build("swamp_troll", "Troll de pantano", 400, 30, 1.2f, CreatureTag.None, new Vector3(1.6f, 1.6f, 1.6f), new Color(0.2f, 0.35f, 0.2f), go => go.AddComponent<RegenerationModule>().healthPerSecond = 3f);
        AssetDatabase.SaveAssets();
    }

    static void Build(string id, string displayName, int health, int damage, float speed, CreatureTag tags, Vector3 scale, Color color, System.Action<GameObject> addModules)
    {
        var path = UnitsFolder + "/Enemy_" + id + ".asset";
        var definition = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            AssetDatabase.CreateAsset(definition, path);
        }
        definition.id = id;
        definition.displayName = displayName;
        definition.team = Team.Enemy;
        definition.kind = EnemyKind.Ground;
        definition.maxHealth = health;
        definition.attackDamage = damage;
        definition.moveSpeed = speed;
        definition.tags = tags;

        var name = "Enemy_" + id;
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial(name, color);
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        go.AddComponent<MovementComponent>().lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        go.AddComponent<EnemyBrain>();
        addModules?.Invoke(go);
        definition.prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        EditorUtility.SetDirty(definition);
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
