using System.IO;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class EnemyBaseBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string BasesFolder = "Assets/_Project/ScriptableObjects/Bases";
    const string PrefabsFolder = "Assets/_Project/Prefabs";
    const string SpritesFolder = "Assets/_Project/Art/Sprites";
    const string MaterialsFolder = "Assets/_Project/Art/Materials";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";
    const string WargPath = UnitsFolder + "/Enemy_Warg.asset";
    const string BatPath = UnitsFolder + "/Enemy_Bat.asset";
    const string BasePath = BasesFolder + "/Base_CabinRuins.asset";

    [MenuItem("EC/Sandbox/Build Enemies And Base")]
    public static void BuildAssets()
    {
        EnsureFolder("Assets/_Project/ScriptableObjects", "Units");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Bases");
        EnsureFolder("Assets/_Project/Art", "Sprites");
        EnsureFolder("Assets/_Project/Art", "Materials");

        var warg = CreateEnemy(WargPath, "warg", "Wargo", EnemyKind.Ground, 40, 2.5f, 12);
        var bat = CreateEnemy(BatPath, "bat", "Murcielago", EnemyKind.Flying, 15, 3.5f, 6);
        warg.prefab = CreateEnemyPrefab("Enemy_Warg", warg, PrimitiveType.Capsule, new Vector3(1.2f, 0.8f, 1.2f), new Color(0.35f, 0.25f, 0.2f));
        bat.prefab = CreateEnemyPrefab("Enemy_Bat", bat, PrimitiveType.Sphere, new Vector3(0.7f, 0.5f, 0.7f), new Color(0.3f, 0.1f, 0.4f));
        EditorUtility.SetDirty(warg);
        EditorUtility.SetDirty(bat);

        var stages = new[]
        {
            CreateStageSprite("BaseStage_0", new Color(0.45f, 0.38f, 0.3f)),
            CreateStageSprite("BaseStage_1", new Color(0.35f, 0.28f, 0.22f)),
            CreateStageSprite("BaseStage_2", new Color(0.22f, 0.17f, 0.14f))
        };
        var baseDefinition = LoadOrCreate<BaseDefinition>(BasePath);
        baseDefinition.id = "cabin_ruins";
        baseDefinition.displayName = "Ruinas de la cabana";
        baseDefinition.maxResistance = 500;
        baseDefinition.damageStages = stages;
        baseDefinition.prefab = CreateBasePrefab(baseDefinition);
        EditorUtility.SetDirty(baseDefinition);
        AssetDatabase.SaveAssets();
    }

    public static void SpawnSandbox(LaneConfig config)
    {
        BuildAssets();
        var baseDefinition = AssetDatabase.LoadAssetAtPath<BaseDefinition>(BasePath);
        var baseInstance = (GameObject)PrefabUtility.InstantiatePrefab(baseDefinition.prefab);
        baseInstance.name = "Base";
        baseInstance.transform.position = new Vector3(config.baseX, config.groundY + 1.5f, 0f);
        baseInstance.GetComponent<BaseStructure>().definition = baseDefinition;

        var pool = new GameObject("PoolService").AddComponent<PoolService>();
        var spawner = new GameObject("EnemySpawner").AddComponent<EnemySpawner>();
        spawner.pool = pool;
        spawner.lane = config;
        spawner.baseTarget = baseInstance.transform;
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
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

    static EnemyDefinition CreateEnemy(string path, string id, string displayName, EnemyKind kind, int health, float speed, int damage)
    {
        var enemy = LoadOrCreate<EnemyDefinition>(path);
        enemy.id = id;
        enemy.displayName = displayName;
        enemy.team = Team.Enemy;
        enemy.kind = kind;
        enemy.maxHealth = health;
        enemy.moveSpeed = speed;
        enemy.attackDamage = damage;
        return enemy;
    }

    static GameObject CreateEnemyPrefab(string name, EnemyDefinition definition, PrimitiveType shape, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(shape);
        go.name = name;
        go.transform.localScale = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial(name, color);
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        go.AddComponent<EnemyBrain>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateBasePrefab(BaseDefinition definition)
    {
        var go = new GameObject("Base_CabinRuins");
        go.transform.localScale = Vector3.one * 4f;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = definition.damageStages[0];
        var structure = go.AddComponent<BaseStructure>();
        structure.definition = definition;
        structure.spriteRenderer = renderer;
        go.AddComponent<BillboardSprite>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/Base_CabinRuins.prefab");
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

    static Sprite CreateStageSprite(string name, Color color)
    {
        var path = SpritesFolder + "/" + name + ".png";
        if (!File.Exists(path))
        {
            var texture = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels(pixels);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
