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

public static class CampaignBuilder
{
    const string Folder = "Assets/_Project/ScriptableObjects/Campaign";
    const string CampaignPath = Folder + "/Campaign_Main.asset";
    public const string ScenePath = "Assets/_Project/Scenes/CampaignMap.unity";

    [MenuItem("EC/Level/Build Campaign Map")]
    public static void BuildFromMenu()
    {
        Build(AssetDatabase.LoadAssetAtPath<LevelDefinition>("Assets/_Project/ScriptableObjects/Levels/Level_1_1.asset"));
    }

    public static void Build(LevelDefinition firstLevel)
    {
        var campaign = BuildAssets(firstLevel);
        BuildScene(campaign);
    }

    public static CampaignDefinition BuildAssets(LevelDefinition firstLevel)
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Campaign");

        var chapters = new[]
        {
            Chapter("ch1_afueras", "Las Afueras", firstLevel),
            Chapter("ch2_aldea", "La Aldea"),
            Chapter("ch3_bosque", "El Bosque Maldito"),
            Chapter("ch4_catedral", "La Catedral")
        };

        var campaign = LoadOrCreate<CampaignDefinition>(CampaignPath);
        campaign.chapters = chapters;
        EditorUtility.SetDirty(campaign);
        AssetDatabase.SaveAssets();
        return campaign;
    }

    static ChapterDefinition Chapter(string id, string displayName, params LevelDefinition[] levels)
    {
        var chapter = LoadOrCreate<ChapterDefinition>(Folder + "/" + id + ".asset");
        chapter.id = id;
        chapter.displayName = displayName;
        chapter.levels = levels;
        EditorUtility.SetDirty(chapter);
        return chapter;
    }

    static void BuildScene(CampaignDefinition campaign)
    {
        var theme = HudBuilder.LoadOrCreateTheme();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cam.AddComponent<Camera>();
        camera.backgroundColor = theme.background;
        camera.clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();

        var canvasObject = new GameObject("MapCanvas");
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

        var title = HudBuilder.CreateLabel(safe, "Title", "Campaña", 96f, new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1000f, 140f), theme, theme.titleFont);
        title.color = theme.gold;

        var scrollRect = HudBuilder.NewRect("Scroll", safe);
        scrollRect.anchorMin = new Vector2(0f, 0f);
        scrollRect.anchorMax = new Vector2(1f, 1f);
        scrollRect.offsetMin = new Vector2(40f, 180f);
        scrollRect.offsetMax = new Vector2(-40f, -200f);
        var scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
        scrollRect.gameObject.AddComponent<RectMask2D>();
        scroll.horizontal = true;
        scroll.vertical = false;

        var content = HudBuilder.NewRect("Content", scrollRect);
        content.anchorMin = new Vector2(0f, 0f);
        content.anchorMax = new Vector2(0f, 1f);
        content.pivot = new Vector2(0f, 0.5f);
        content.anchoredPosition = Vector2.zero;
        var row = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.spacing = 60f;
        row.padding = new RectOffset(40, 40, 0, 0);
        row.childControlWidth = true;
        row.childControlHeight = true;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = true;
        content.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = content;

        var chapterTemplate = HudBuilder.NewRect("ChapterTemplate", content);
        chapterTemplate.gameObject.AddComponent<LayoutElement>().preferredWidth = 640f;
        var column = chapterTemplate.gameObject.AddComponent<VerticalLayoutGroup>();
        column.spacing = 30f;
        column.childControlWidth = true;
        column.childControlHeight = true;
        column.childForceExpandWidth = true;
        column.childForceExpandHeight = false;
        var chapterTitle = HudBuilder.CreateLabel(chapterTemplate, "Title", "", 56f, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(640f, 100f), theme, theme.titleFont);
        chapterTitle.color = theme.gold;
        chapterTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 100f;
        var nodes = HudBuilder.NewRect("Nodes", chapterTemplate);
        var nodesLayout = nodes.gameObject.AddComponent<VerticalLayoutGroup>();
        nodesLayout.spacing = 24f;
        nodesLayout.childControlWidth = true;
        nodesLayout.childControlHeight = true;
        nodesLayout.childForceExpandWidth = true;
        nodesLayout.childForceExpandHeight = false;

        var nodeButton = HudBuilder.CreateButton(chapterTemplate, "NodeTemplate", "", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 140f), theme);
        nodeButton.gameObject.AddComponent<LayoutElement>().preferredHeight = 140f;
        var node = nodeButton.gameObject.AddComponent<LevelNode>();
        node.button = nodeButton;
        node.label = nodeButton.GetComponentInChildren<TMP_Text>();
        nodeButton.transform.SetParent(safe, false);

        chapterTemplate.gameObject.SetActive(false);
        nodeButton.gameObject.SetActive(false);

        var map = canvasObject.AddComponent<CampaignMap>();
        map.campaign = campaign;
        map.content = content;
        map.chapterTemplate = chapterTemplate.gameObject;
        map.nodeTemplate = nodeButton.gameObject;

        var back = HudBuilder.CreateButton(safe, "BackButton", "Volver", new Vector2(0f, 0f), new Vector2(260f, 100f), new Vector2(420f, 120f), theme);
        UnityEventTools.AddPersistentListener(back.onClick, map.Back);

        var emporium = HudBuilder.CreateButton(safe, "EmporiumButton", "Emporium", new Vector2(1f, 0f), new Vector2(-260f, 100f), new Vector2(420f, 120f), theme);
        UnityEventTools.AddPersistentListener(emporium.onClick, map.OpenEmporium);

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
