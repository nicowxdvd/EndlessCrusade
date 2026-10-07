using System.IO;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

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
        CreateTouchControls();
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

    static Sprite LoadPlaceholderSprite()
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

    static void CreatePrefab(HeroDefinition definition)
    {
        var go = new GameObject("Hero");
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();

        var visual = new GameObject("Sprite");
        visual.transform.SetParent(go.transform, false);
        visual.transform.localScale = new Vector3(0.8f, 1.8f, 1f);
        visual.AddComponent<BillboardSprite>();
        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadPlaceholderSprite();
        renderer.color = new Color(0.85f, 0.85f, 0.9f);

        var hero = go.AddComponent<HeroController>();
        hero.controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
        hero.sprite = renderer;

        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
    }

    static void CreateTouchControls()
    {
        var canvasGo = new GameObject("Touch Controls");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasGo.AddComponent<GraphicRaycaster>();

        CreateButton(canvasGo.transform, "Left", "<Gamepad>/dpad/left", new Vector2(0f, 0f), new Vector2(150f, 150f));
        CreateButton(canvasGo.transform, "Right", "<Gamepad>/dpad/right", new Vector2(0f, 0f), new Vector2(350f, 150f));
        CreateButton(canvasGo.transform, "Sword", "<Gamepad>/buttonWest", new Vector2(1f, 0f), new Vector2(-350f, 150f));
        CreateButton(canvasGo.transform, "Whip", "<Gamepad>/buttonEast", new Vector2(1f, 0f), new Vector2(-150f, 150f));

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    static void CreateButton(Transform parent, string name, string controlPath, Vector2 anchor, Vector2 position)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(180f, 180f);
        go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.35f);
        go.AddComponent<OnScreenButton>().controlPath = controlPath;

        var label = new GameObject("Label");
        label.transform.SetParent(go.transform, false);
        var labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var text = label.AddComponent<Text>();
        text.text = name;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 40;
        text.color = Color.white;
        text.raycastTarget = false;
    }
}
