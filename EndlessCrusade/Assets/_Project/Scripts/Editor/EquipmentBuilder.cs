using EC.Data;
using EC.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class EquipmentBuilder
{
    const string Folder = "Assets/_Project/ScriptableObjects/Equipment";
    const string CatalogPath = Folder + "/Equipment_Catalog.asset";
    public const string ScenePath = "Assets/_Project/Scenes/Equipment.unity";
    public const string ArmorId = "armor_templar";
    public const string BlessingId = "miracle_blessing";
    public const string ShieldId = "shield_templar";
    public const string MaceId = "mace_templar";
    public const string CrossbowId = "crossbow_templar";
    public const string JudgmentId = "miracle_judgment";

    [MenuItem("EC/Level/Build Equipment")]
    public static void Build()
    {
        BuildScene(BuildAssets());
    }

    public static EquipmentCatalog BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Equipment");

        var catalog = LoadOrCreate<EquipmentCatalog>(CatalogPath);
        catalog.equipment = new[]
        {
            Equipment(ArmorId, "Armadura templaria", EquipmentSlot.Armor, 80, 0.2f, 0f, null, 0f, null),
            Equipment(ShieldId, "Escudo templario", EquipmentSlot.Shield, 0, 0f, 0.7f, null, 0f, null),
            Equipment(MaceId, "Maza templaria", EquipmentSlot.HeavyWeapon, 0, 0f, 0f, new MeleeAttackDefinition { id = "mace", damage = 35, range = 1.6f, cooldown = 1.2f }, 1f, null),
            Equipment(CrossbowId, "Ballesta templaria", EquipmentSlot.RangedWeapon, 0, 0f, 0f, new MeleeAttackDefinition { id = "crossbow", damage = 22, range = 9f, cooldown = 1.4f }, 0f, TroopBuilder.CreateBoltPrefab())
        };
        catalog.miracles = new[]
        {
            Miracle(BlessingId, "Bendición", AbilityKind.Heal, 25f, 0, 0f),
            Miracle(JudgmentId, "Juicio Divino", AbilityKind.ScreenDamage, 45f, 60, 20f)
        };
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        return catalog;
    }

    static EquipmentDefinition Equipment(string id, string displayName, EquipmentSlot slot, int health, float reduction, float block, MeleeAttackDefinition attack, float stun, GameObject projectile)
    {
        var item = LoadOrCreate<EquipmentDefinition>(Folder + "/Equipment_" + id + ".asset");
        item.id = id;
        item.displayName = displayName;
        item.slot = slot;
        item.bonusHealth = health;
        item.damageReduction = reduction;
        item.blockFraction = block;
        item.attackOverride = attack;
        item.stunSeconds = stun;
        item.projectilePrefab = projectile;
        EditorUtility.SetDirty(item);
        return item;
    }

    static MiracleDefinition Miracle(string id, string displayName, AbilityKind kind, float cooldown, int damage, float radius)
    {
        var ability = LoadOrCreate<AbilityDefinition>(Folder + "/Ability_" + id + ".asset");
        ability.id = id;
        ability.displayName = displayName;
        ability.kind = kind;
        ability.cooldown = cooldown;
        ability.damage = damage;
        ability.radius = radius;
        ability.undeadMultiplier = 2f;
        ability.healFraction = 0.25f;
        EditorUtility.SetDirty(ability);

        var miracle = LoadOrCreate<MiracleDefinition>(Folder + "/Miracle_" + id + ".asset");
        miracle.id = id;
        miracle.displayName = displayName;
        miracle.ability = ability;
        EditorUtility.SetDirty(miracle);
        return miracle;
    }

    static void BuildScene(EquipmentCatalog catalog)
    {
        var theme = HudBuilder.LoadOrCreateTheme();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cam.AddComponent<Camera>();
        camera.backgroundColor = theme.background;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();

        var canvasObject = new GameObject("EquipmentCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var safe = HudBuilder.NewRect("SafeArea", canvasObject.transform);
        HudBuilder.Stretch(safe);
        safe.gameObject.AddComponent<SafeAreaFitter>();

        var screen = canvasObject.AddComponent<EquipmentScreen>();
        screen.catalog = catalog;

        var title = HudBuilder.CreateLabel(safe, "Title", "Equipo", 96f, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 140f), theme, theme.titleFont);
        title.color = theme.gold;
        screen.emptyLabel = HudBuilder.CreateLabel(safe, "Empty", "", 44f, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 100f), theme, theme.bodyFont);

        var listRect = HudBuilder.NewRect("List", safe);
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.offsetMin = new Vector2(120f, 280f);
        listRect.offsetMax = new Vector2(-120f, -220f);
        var layout = listRect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        screen.listRoot = listRect;

        var row = HudBuilder.NewRect("RowTemplate", listRect);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 130f;
        var background = row.gameObject.AddComponent<Image>();
        background.color = new Color(theme.background.r, theme.background.g, theme.background.b, 0.85f);
        var rowView = row.gameObject.AddComponent<EmporiumRow>();
        rowView.nameLabel = HudBuilder.CreateLabel(row, "Name", "", 44f, new Vector2(0f, 0.5f), new Vector2(420f, 24f), new Vector2(800f, 60f), theme, theme.bodyFont);
        rowView.infoLabel = HudBuilder.CreateLabel(row, "Info", "", 32f, new Vector2(0f, 0.5f), new Vector2(420f, -30f), new Vector2(800f, 50f), theme, theme.bodyFont);
        rowView.buyButton = HudBuilder.CreateButton(row, "ToggleButton", "", new Vector2(1f, 0.5f), new Vector2(-260f, 0f), new Vector2(480f, 100f), theme);
        rowView.buyLabel = rowView.buyButton.GetComponentInChildren<TMP_Text>();
        screen.rowTemplate = row.gameObject;
        row.gameObject.SetActive(false);

        var back = HudBuilder.CreateButton(safe, "BackButton", "Volver", new Vector2(0f, 0f), new Vector2(260f, 100f), new Vector2(420f, 120f), theme);
        UnityEventTools.AddPersistentListener(back.onClick, screen.Back);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, ScenePath);
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
