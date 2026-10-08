using EC.Core;
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

public static class EmporiumBuilder
{
    const string Folder = "Assets/_Project/ScriptableObjects/Shop";
    const string CatalogPath = Folder + "/Shop_Main.asset";
    public const string ScenePath = "Assets/_Project/Scenes/Emporium.unity";

    [MenuItem("EC/Level/Build Emporium")]
    public static void Build()
    {
        BuildScene(BuildAssets());
    }

    public static ShopCatalog BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Shop");

        var catalog = LoadOrCreate<ShopCatalog>(CatalogPath);
        catalog.upgrades = new[]
        {
            Upgrade("hero_health", "Vida máxima", UpgradeCategory.Hero, UpgradeStat.HeroHealth, null, 10, 100, 0.10f),
            Upgrade("hero_sword", "Daño de espada", UpgradeCategory.Hero, UpgradeStat.SwordDamage, null, 10, 100, 0.08f),
            Upgrade("hero_whip", "Daño de látigo", UpgradeCategory.Hero, UpgradeStat.WhipDamage, null, 10, 100, 0.08f),
            Upgrade("hero_cooldown", "Reducción de cooldown", UpgradeCategory.Hero, UpgradeStat.CooldownReduction, null, 8, 150, 0.05f),
            Upgrade("troop_squire", "Escudero", UpgradeCategory.Troop, UpgradeStat.TroopPower, "squire", 10, 120, 0.10f),
            Upgrade("troop_peasant", "Campesino", UpgradeCategory.Troop, UpgradeStat.TroopPower, "peasant", 10, 120, 0.10f),
            Upgrade("troop_crossbowman", "Ballestero", UpgradeCategory.Troop, UpgradeStat.TroopPower, "crossbowman", 10, 150, 0.10f),
            Upgrade("troop_priest", "Sacerdote", UpgradeCategory.Troop, UpgradeStat.TroopPower, "priest", 10, 150, 0.10f),
            Upgrade("troop_paladin", "Paladín", UpgradeCategory.Troop, UpgradeStat.TroopPower, "paladin", 10, 200, 0.10f),
            Upgrade("base_resistance", "Resistencia de la base", UpgradeCategory.Base, UpgradeStat.BaseResistance, null, 10, 120, 0.15f),
            Upgrade("base_crossbows", "Ballesteros en las almenas", UpgradeCategory.Base, UpgradeStat.BaseCrossbows, null, 1, 800, 0f)
        };
        catalog.consumables = new[]
        {
            Consumable(ConsumableDefinition.HolyWaterPotionId, "Poma de Agua Bendita", CurrencyType.Gold, 80),
            Consumable(ConsumableDefinition.ReviveElixirId, "Elixir de Resurrección", CurrencyType.Gems, 3)
        };
        catalog.iapProducts = new[]
        {
            Product("gems_pack_small", "Puñado de reliquias", 80),
            Product("gems_pack_medium", "Cofre de reliquias", 450),
            Product("gems_pack_large", "Tesoro de reliquias", 1000)
        };
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        return catalog;
    }

    static UpgradeDefinition Upgrade(string id, string displayName, UpgradeCategory category, UpgradeStat stat, string target, int maxLevel, int baseCost, float valuePerLevel)
    {
        var upgrade = LoadOrCreate<UpgradeDefinition>(Folder + "/Upgrade_" + id + ".asset");
        upgrade.id = id;
        upgrade.displayName = displayName;
        upgrade.category = category;
        upgrade.stat = stat;
        upgrade.targetId = target;
        upgrade.maxLevel = maxLevel;
        upgrade.baseCost = baseCost;
        upgrade.costGrowth = 1.35f;
        upgrade.valuePerLevel = valuePerLevel;
        upgrade.currency = CurrencyType.Gold;
        EditorUtility.SetDirty(upgrade);
        return upgrade;
    }

    static IapProductDefinition Product(string productId, string displayName, int gems)
    {
        var product = LoadOrCreate<IapProductDefinition>(Folder + "/Iap_" + productId + ".asset");
        product.productId = productId;
        product.displayName = displayName;
        product.gemsGranted = gems;
        EditorUtility.SetDirty(product);
        return product;
    }

    static ConsumableDefinition Consumable(string id, string displayName, CurrencyType currency, int cost)
    {
        var consumable = LoadOrCreate<ConsumableDefinition>(Folder + "/Consumable_" + id + ".asset");
        consumable.id = id;
        consumable.displayName = displayName;
        consumable.currency = currency;
        consumable.cost = cost;
        consumable.maxStack = 9;
        EditorUtility.SetDirty(consumable);
        return consumable;
    }

    static void BuildScene(ShopCatalog catalog)
    {
        var theme = HudBuilder.LoadOrCreateTheme();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cam.AddComponent<Camera>();
        camera.backgroundColor = theme.background;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();

        var canvasObject = new GameObject("EmporiumCanvas");
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

        var screen = canvasObject.AddComponent<EmporiumScreen>();
        screen.catalog = catalog;

        var title = HudBuilder.CreateLabel(safe, "Title", "Emporium", 96f, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 140f), theme, theme.titleFont);
        title.color = theme.gold;
        screen.balanceLabel = HudBuilder.CreateLabel(safe, "Balance", "", 40f, new Vector2(0f, 1f), new Vector2(420f, -60f), new Vector2(800f, 70f), theme, theme.bodyFont);
        screen.balanceLabel.color = theme.gold;
        screen.messageLabel = HudBuilder.CreateLabel(safe, "Message", "", 40f, new Vector2(0.5f, 0f), new Vector2(0f, 190f), new Vector2(1200f, 70f), theme, theme.bodyFont);

        var tabs = new[] { ("Héroe", (UnityEngine.Events.UnityAction)screen.ShowHero), ("Tropas", screen.ShowTroops), ("Base", screen.ShowBase), ("Consumibles", screen.ShowConsumables), ("Reliquias", screen.ShowGems) };
        for (int i = 0; i < tabs.Length; i++)
        {
            var x = (i - (tabs.Length - 1) * 0.5f) * 370f;
            var tab = HudBuilder.CreateButton(safe, "Tab_" + tabs[i].Item1, tabs[i].Item1, new Vector2(0.5f, 1f), new Vector2(x, -230f), new Vector2(350f, 110f), theme);
            UnityEventTools.AddPersistentListener(tab.onClick, tabs[i].Item2);
        }

        var listRect = HudBuilder.NewRect("List", safe);
        listRect.anchorMin = new Vector2(0f, 0f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.offsetMin = new Vector2(120f, 280f);
        listRect.offsetMax = new Vector2(-120f, -320f);
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
        rowView.buyButton = HudBuilder.CreateButton(row, "BuyButton", "", new Vector2(1f, 0.5f), new Vector2(-260f, 0f), new Vector2(480f, 100f), theme);
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
