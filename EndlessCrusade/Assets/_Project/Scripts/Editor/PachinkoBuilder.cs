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

public static class PachinkoBuilder
{
    const string Folder = "Assets/_Project/ScriptableObjects/Pachinko";
    const string TablePath = Folder + "/PrizeTable_Main.asset";
    const string PrefabPath = "Assets/_Project/Prefabs/PachinkoBall.prefab";
    public const string ScenePath = "Assets/_Project/Scenes/Pachinko.unity";

    const float HalfWidth = 4.5f;
    const float TopY = 5f;
    const float SlotY = -4.5f;

    [MenuItem("EC/Level/Build Pachinko")]
    public static void Build()
    {
        BuildScene(BuildAssets());
    }

    public static PrizeTable BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Pachinko");

        var table = AssetDatabase.LoadAssetAtPath<PrizeTable>(TablePath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<PrizeTable>();
            AssetDatabase.CreateAsset(table, TablePath);
        }
        table.entries = new[]
        {
            Entry("gold_100", "100 de oro", PrizeKind.Gold, 100, null, 40),
            Entry("gold_500", "500 de oro", PrizeKind.Gold, 500, null, 20),
            Entry("gems_5", "5 reliquias", PrizeKind.Gems, 5, null, 20),
            Entry("gems_25", "25 reliquias", PrizeKind.Gems, 25, null, 5),
            Entry("potion", "Poma de Agua Bendita", PrizeKind.Consumable, 1, ConsumableDefinition.HolyWaterPotionId, 10),
            Entry("elixir", "Elixir de Resurrección", PrizeKind.Consumable, 1, ConsumableDefinition.ReviveElixirId, 5)
        };
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();
        return table;
    }

    static PrizeEntry Entry(string id, string displayName, PrizeKind kind, int amount, string itemId, int weight)
    {
        return new PrizeEntry { id = id, displayName = displayName, kind = kind, amount = amount, itemId = itemId, weight = weight };
    }

    static void BuildScene(PrizeTable table)
    {
        var theme = HudBuilder.LoadOrCreateTheme();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        var cam = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cam.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.5f;
        camera.backgroundColor = theme.background;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.AddComponent<AudioListener>();

        var board = new GameObject("Board").transform;
        var bounce = new PhysicsMaterial2D("PachinkoBounce") { bounciness = 0.5f, friction = 0.05f };

        BuildWalls(board);
        BuildPegs(board, circle, bounce);
        BuildSlots(board, table, theme);

        var ball = BuildBallPrefab(circle, bounce);
        var spawn = new GameObject("Spawn").transform;
        spawn.position = new Vector3(0f, TopY + 0.5f, 0f);

        var canvasObject = new GameObject("PachinkoCanvas");
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

        var screen = canvasObject.AddComponent<PachinkoScreen>();
        screen.table = table;
        screen.ball = ball;
        screen.spawnPoint = spawn;
        screen.boardHalfWidth = HalfWidth;
        screen.slotY = SlotY;

        var title = HudBuilder.CreateLabel(safe, "Title", "Pachinko", 80f, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(800f, 120f), theme, theme.titleFont);
        title.color = theme.gold;
        screen.ticketsLabel = HudBuilder.CreateLabel(safe, "Tickets", "", 44f, new Vector2(0f, 1f), new Vector2(260f, -60f), new Vector2(480f, 70f), theme, theme.bodyFont);
        screen.resultLabel = HudBuilder.CreateLabel(safe, "Result", "", 48f, new Vector2(0.5f, 0f), new Vector2(0f, 240f), new Vector2(1200f, 80f), theme, theme.bodyFont);
        screen.resultLabel.color = theme.gold;

        var oddsPanel = HudBuilder.CreateLabel(safe, "Odds", "", 34f, new Vector2(1f, 0.5f), new Vector2(-300f, 0f), new Vector2(520f, 520f), theme, theme.bodyFont);
        oddsPanel.alignment = TextAlignmentOptions.TopLeft;
        screen.oddsLabel = oddsPanel;
        HudBuilder.CreateLabel(safe, "OddsTitle", "Probabilidades", 40f, new Vector2(1f, 0.5f), new Vector2(-300f, 300f), new Vector2(520f, 70f), theme, theme.titleFont).color = theme.gold;

        screen.launchButton = HudBuilder.CreateButton(safe, "LaunchButton", "Lanzar (1 boleto)", new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(560f, 120f), theme);
        screen.dailyButton = HudBuilder.CreateButton(safe, "DailyButton", "Boleto diario", new Vector2(0f, 0.5f), new Vector2(300f, 0f), new Vector2(480f, 110f), theme);
        var back = HudBuilder.CreateButton(safe, "BackButton", "Volver", new Vector2(0f, 0f), new Vector2(260f, 100f), new Vector2(420f, 120f), theme);
        UnityEventTools.AddPersistentListener(screen.launchButton.onClick, screen.Launch);
        UnityEventTools.AddPersistentListener(screen.dailyButton.onClick, screen.ClaimDaily);
        UnityEventTools.AddPersistentListener(back.onClick, screen.Back);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    static void BuildWalls(Transform parent)
    {
        AddWall(parent, "WallLeft", new Vector2(-HalfWidth - 0.25f, 0f), new Vector2(0.5f, 14f));
        AddWall(parent, "WallRight", new Vector2(HalfWidth + 0.25f, 0f), new Vector2(0.5f, 14f));
        AddWall(parent, "Floor", new Vector2(0f, SlotY - 0.75f), new Vector2(2f * HalfWidth + 1f, 0.5f));
    }

    static void AddWall(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.AddComponent<BoxCollider2D>().size = size;
    }

    static void BuildPegs(Transform parent, Sprite circle, PhysicsMaterial2D material)
    {
        var rows = 9;
        for (int row = 0; row < rows; row++)
        {
            var y = TopY - 1f - row * 0.9f;
            var count = row % 2 == 0 ? 8 : 7;
            var spacing = 2f * HalfWidth / 8f;
            var offset = row % 2 == 0 ? spacing * 0.5f : spacing;
            for (int i = 0; i < count; i++)
            {
                var peg = new GameObject("Peg");
                peg.transform.SetParent(parent, false);
                peg.transform.position = new Vector3(-HalfWidth + offset + i * spacing, y, 0f);
                peg.transform.localScale = Vector3.one * 0.25f;
                var renderer = peg.AddComponent<SpriteRenderer>();
                renderer.sprite = circle;
                var collider = peg.AddComponent<CircleCollider2D>();
                collider.sharedMaterial = material;
            }
        }
    }

    static void BuildSlots(Transform parent, PrizeTable table, UiTheme theme)
    {
        var count = table.entries.Length;
        var slotWidth = 2f * HalfWidth / count;
        for (int i = 0; i <= count; i++)
        {
            var divider = new GameObject("Divider");
            divider.transform.SetParent(parent, false);
            divider.transform.position = new Vector3(-HalfWidth + i * slotWidth, SlotY - 0.1f, 0f);
            divider.AddComponent<BoxCollider2D>().size = new Vector2(0.1f, 1.2f);
        }

        for (int i = 0; i < count; i++)
        {
            var label = new GameObject("SlotLabel_" + i);
            label.transform.SetParent(parent, false);
            label.transform.position = new Vector3(PachinkoScreen.SlotX(i, count, HalfWidth), SlotY - 1.4f, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.text = table.entries[i].displayName;
            text.fontSize = 2f;
            text.alignment = TextAlignmentOptions.Center;
            text.rectTransform.sizeDelta = new Vector2(slotWidth, 1.2f);
            text.color = theme.gold;
        }
    }

    static PachinkoBall BuildBallPrefab(Sprite circle, PhysicsMaterial2D material)
    {
        var go = new GameObject("PachinkoBall");
        go.transform.localScale = Vector3.one * 0.4f;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = circle;
        renderer.color = new Color(0.9f, 0.85f, 0.6f);
        renderer.sortingOrder = 2;
        var body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 1.2f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        go.AddComponent<CircleCollider2D>().sharedMaterial = material;
        go.AddComponent<PachinkoBall>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "PachinkoBall";
        return instance.GetComponent<PachinkoBall>();
    }
}
