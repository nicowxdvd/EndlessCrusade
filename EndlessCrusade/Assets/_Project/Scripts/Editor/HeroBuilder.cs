using System.IO;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public static class HeroBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string DefinitionPath = UnitsFolder + "/Hero_Templar_Unarmored.asset";
    const string PrefabPath = "Assets/_Project/Prefabs/Hero.prefab";
    const string ControlsPath = "Assets/_Project/Input/Controls.inputactions";
    const string SpritePath = "Assets/_Project/Art/Sprites/Hero_Placeholder.png";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";

    [MenuItem("EC/Sandbox/Build Hero Assets")]
    public static void BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(UnitsFolder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Units");

        var definition = CreateDefinition();
        CreatePrefab(definition);
        definition.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        EditorUtility.SetDirty(definition);
        AssetDatabase.SaveAssets();
    }

    public static void SpawnHero(LaneConfig config)
    {
        BuildAssets();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "Hero";
        instance.transform.position = new Vector3(config.heroStartX, config.groundY + 1f, 0f);
        HudBuilder.SpawnHud();
    }

    static HeroDefinition CreateDefinition()
    {
        var definition = AssetDatabase.LoadAssetAtPath<HeroDefinition>(DefinitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<HeroDefinition>();
            AssetDatabase.CreateAsset(definition, DefinitionPath);
        }
        definition.id = "hero_templar_unarmored";
        definition.displayName = "Templario";
        definition.team = Team.Player;
        definition.maxHealth = 120;
        definition.moveSpeed = 3f;
        definition.sword = new MeleeAttackDefinition { id = "sword", damage = 18, range = 1.4f, cooldown = 0.5f };
        definition.whip = new MeleeAttackDefinition { id = "whip", damage = 10, range = 3f, cooldown = 0.9f };
        EditorUtility.SetDirty(definition);
        return definition;
    }

    public static Sprite LoadPlaceholderSprite()
    {
        if (!File.Exists(SpritePath))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Sprites"))
                AssetDatabase.CreateFolder("Assets/_Project/Art", "Sprites");
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SpritePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 1f;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
    }

    static SpriteRenderer CreateLayer(Transform parent, string name, int order)
    {
        var layer = new GameObject(name);
        layer.transform.SetParent(parent, false);
        var renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = order;
        renderer.enabled = false;
        return renderer;
    }

    static void CreatePrefab(HeroDefinition definition)
    {
        var go = new GameObject("Hero");
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        go.AddComponent<AbilityComponent>().abilities = new[] { AbilityBuilder.BuildAssets(), null, null };

        var visual = new GameObject("Sprite");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localScale = new Vector3(0.8f, 1.8f, 1f);
        visual.AddComponent<BillboardSprite>();
        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadPlaceholderSprite();
        renderer.color = new Color(0.85f, 0.85f, 0.9f);

        var visualController = go.AddComponent<HeroVisualController>();
        visualController.armorLayer = CreateLayer(visual.transform, "Layer_Armor", 1);
        visualController.shieldLayer = CreateLayer(visual.transform, "Layer_Shield", 2);
        visualController.weaponLayer = CreateLayer(visual.transform, "Layer_Weapon", 3);

        var hero = go.AddComponent<HeroController>();
        hero.controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
        hero.sprite = renderer;

        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
    }
}
