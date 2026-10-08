using EC.Data;
using EC.Gameplay;
using EC.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class LevelBuilder
{
    public const string LevelsFolder = "Assets/_Project/ScriptableObjects/Levels";
    public const string StoriesFolder = "Assets/_Project/ScriptableObjects/Stories";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";
    const string WargPath = "Assets/_Project/ScriptableObjects/Units/Enemy_Warg.asset";
    const string BatPath = "Assets/_Project/ScriptableObjects/Units/Enemy_Bat.asset";
    const string BasePath = "Assets/_Project/ScriptableObjects/Bases/Base_CabinRuins.asset";
    const string EnvironmentPath = "Assets/_Project/Prefabs/Env_Outskirts.prefab";
    const string LevelPath = LevelsFolder + "/Level_1_1.asset";
    const string LevelScenePath = "Assets/_Project/Scenes/Level.unity";
    const string MainScenePath = "Assets/_Project/Scenes/Main.unity";
    const string BootScenePath = "Assets/_Project/Scenes/Boot.unity";
    const string SandboxScenePath = "Assets/_Project/Scenes/Sandbox/LaneSandbox.unity";

    [MenuItem("EC/Level/Build Level 1")]
    public static void BuildAll()
    {
        var level = BuildAssets();
        CampaignBuilder.BuildAssets(level);
        BuildLevelScene(level);
        BuildMainScene();
        CampaignBuilder.Build(level);
        EmporiumBuilder.Build();
        EquipmentBuilder.Build();
        PachinkoBuilder.Build();
        AudioBuilder.Build();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootScenePath, true),
            new EditorBuildSettingsScene(MainScenePath, true),
            new EditorBuildSettingsScene(CampaignBuilder.ScenePath, true),
            new EditorBuildSettingsScene(EmporiumBuilder.ScenePath, true),
            new EditorBuildSettingsScene(EquipmentBuilder.ScenePath, true),
            new EditorBuildSettingsScene(PachinkoBuilder.ScenePath, true),
            new EditorBuildSettingsScene(LevelScenePath, true),
            new EditorBuildSettingsScene(SandboxScenePath, true)
        };
    }

    public static LevelDefinition BuildAssets()
    {
        EnemyBaseBuilder.BuildAssets();
        var boss = BossBuilder.BuildAssets();
        EnsureFolder("Assets/_Project/ScriptableObjects", "Levels");
        EnsureFolder("Assets/_Project/ScriptableObjects", "Stories");

        var warg = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(WargPath);
        var bat = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(BatPath);
        var waves = new[]
        {
            Wave(1, 4f, null, 0f, Entry(warg, 4, 2f, 0f)),
            Wave(2, 4f, null, 0f, Entry(bat, 6, 1.2f, 0f)),
            Wave(3, 5f, null, 0f, Entry(warg, 5, 1.5f, 0f), Entry(bat, 4, 1.2f, 2f)),
            Wave(4, 6f, null, 0f, Entry(warg, 8, 1.2f, 0f), Entry(bat, 6, 1f, 2f)),
            Wave(5, 0f, boss, 4f, Entry(warg, 4, 2f, 0f))
        };

        var level = LoadOrCreate<LevelDefinition>(LevelPath);
        level.id = "lv_1_1";
        level.displayName = "El Despertar en las Afueras";
        level.baseDefinition = AssetDatabase.LoadAssetAtPath<BaseDefinition>(BasePath);
        level.environmentPrefab = BuildEnvironment();
        level.intro = Story("Intro_1_1", "story_1_1_intro",
            "El Templario se ha retirado a una cabaña en las afueras, lejos de las órdenes y de las guerras.",
            "Esa noche estalla una tormenta. La lluvia golpea el techo sin descanso.",
            "El Templario bebe hasta perder la noción del tiempo.",
            "Algo irrumpe en la oscuridad. La puerta cae y se llevan al Bebé de la Profecía.",
            "Despierta entre los restos de la puerta destruida. Debe defender las ruinas.");
        level.outro = Story("Outro_1_1", "story_1_1_outro",
            "El Licántropo Gigante huye herido hacia el bosque, aullando entre la lluvia.",
            "El Templario recoge su espada y parte hacia la aldea tras el rastro del Bebé de la Profecía.");
        level.waves = waves;
        level.troopsEnabled = false;
        level.tutorial = true;
        level.reward = new LevelReward { goldFirstClear = 150, goldReplay = 60, gemsFirstClear = 5, ticketsFirstClear = 1 };
        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        return level;
    }

    static GameObject BuildEnvironment()
    {
        var config = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        LaneSandboxBuilder.CreateBackground(config);
        var root = GameObject.Find("Background");
        root.name = "Env_Outskirts";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, EnvironmentPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    static void BuildLevelScene(LevelDefinition level)
    {
        var config = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        LaneSandboxBuilder.CreateGround(config);
        var cam = LaneSandboxBuilder.CreateCamera(config);
        LaneSandboxBuilder.CreateFollowCamera(config, cam, true);
        LaneSandboxBuilder.CreateRain(cam);
        LaneSandboxBuilder.CreateLights(config);
        LaneSandboxBuilder.CreateVolume();

        var spawner = EnemyBaseBuilder.CreateSpawner(config);
        var waves = new GameObject("WaveController").AddComponent<WaveController>();
        waves.spawner = spawner;
        waves.deferStart = true;
        var bootstrap = new GameObject("LevelBootstrap").AddComponent<LevelBootstrap>();
        bootstrap.fallbackLevel = level;
        bootstrap.lane = config;
        bootstrap.waves = waves;
        bootstrap.spawner = spawner;
        bootstrap.catalog = EmporiumBuilder.BuildAssets();
        bootstrap.equipmentCatalog = EquipmentBuilder.BuildAssets();
        bootstrap.pool = spawner.pool;
        bootstrap.boltPrefab = TroopBuilder.CreateBoltPrefab();
        bootstrap.summoner = TroopBuilder.CreateSummoner(config, level, spawner.pool);
        waves.revive = waves.gameObject.AddComponent<ReviveService>();

        HeroBuilder.SpawnHero(config);
        AddLevelUi(GameObject.Find("HudCanvas"));

        EditorSceneManager.SaveScene(scene, LevelScenePath);
    }

    static void AddLevelUi(GameObject hud)
    {
        var theme = HudBuilder.LoadOrCreateTheme();

        var story = hud.AddComponent<StoryPlayer>();
        var overlay = HudBuilder.CreateOverlay(hud.transform, "StoryPanel", theme);
        overlay.GetComponent<Image>().color = new Color(theme.background.r, theme.background.g, theme.background.b, 0.95f);
        story.root = overlay;
        story.advanceButton = overlay.AddComponent<Button>();
        story.advanceButton.targetGraphic = overlay.GetComponent<Image>();
        var imageRect = HudBuilder.NewRect("Image", overlay.transform);
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = new Vector2(0f, 120f);
        imageRect.sizeDelta = new Vector2(1000f, 560f);
        story.image = imageRect.gameObject.AddComponent<Image>();
        story.image.raycastTarget = false;
        story.text = HudBuilder.CreateLabel(overlay.transform, "Text", "", 52f, new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(1500f, 260f), theme, theme.bodyFont);
        story.skipButton = HudBuilder.CreateButton(overlay.transform, "SkipButton", "Saltar", new Vector2(1f, 1f), new Vector2(-60f, -60f), new Vector2(280f, 110f), theme);
        overlay.SetActive(false);

        var tutorial = hud.AddComponent<TutorialController>();
        var tutorialRoot = HudBuilder.NewRect("TutorialPanel", hud.transform);
        tutorialRoot.anchorMin = new Vector2(0.5f, 1f);
        tutorialRoot.anchorMax = new Vector2(0.5f, 1f);
        tutorialRoot.pivot = new Vector2(0.5f, 1f);
        tutorialRoot.anchoredPosition = new Vector2(0f, -240f);
        tutorialRoot.sizeDelta = new Vector2(1200f, 120f);
        tutorial.root = tutorialRoot.gameObject;
        tutorial.label = HudBuilder.CreateLabel(tutorialRoot, "Message", "", 52f, new Vector2(0.5f, 0.5f), Vector2.zero, tutorialRoot.sizeDelta, theme, theme.bodyFont);
        tutorial.label.color = theme.gold;
        tutorialRoot.gameObject.SetActive(false);

        var flow = hud.AddComponent<LevelFlow>();
        flow.storyPlayer = story;
        flow.tutorial = tutorial;

        hud.GetComponent<ResultPanel>().storyPlayer = story;
    }

    static void BuildMainScene()
    {
        var theme = HudBuilder.LoadOrCreateTheme();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera") { tag = "MainCamera" };
        cam.AddComponent<Camera>().backgroundColor = theme.background;
        cam.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();

        var canvasObject = new GameObject("MenuCanvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var title = HudBuilder.CreateLabel(canvasObject.transform, "Title", "Endless Crusade", 140f, new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(1400f, 200f), theme, theme.titleFont);
        title.color = theme.gold;
        var play = HudBuilder.CreateButton(canvasObject.transform, "PlayButton", "Jugar", new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(520f, 140f), theme);

        var menu = canvasObject.AddComponent<MainMenu>();
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);

        var settings = canvasObject.AddComponent<SettingsPanel>();
        var settingsButton = HudBuilder.CreateButton(canvasObject.transform, "SettingsButton", "Ajustes", new Vector2(0.5f, 0.5f), new Vector2(0f, -300f), new Vector2(520f, 140f), theme);
        UnityEventTools.AddPersistentListener(settingsButton.onClick, settings.Open);

        settings.panel = HudBuilder.CreateOverlay(canvasObject.transform, "SettingsPanel", theme);
        var settingsTitle = HudBuilder.CreateLabel(settings.panel.transform, "Title", "Ajustes", 96f, new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(800f, 140f), theme, theme.titleFont);
        settingsTitle.color = theme.gold;
        HudBuilder.CreateLabel(settings.panel.transform, "MusicLabel", "Música", 48f, new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(600f, 70f), theme, theme.bodyFont);
        settings.musicSlider = HudBuilder.CreateSlider(settings.panel.transform, "MusicSlider", new Vector2(0.5f, 0.5f), new Vector2(0f, 70f), new Vector2(800f, 60f));
        HudBuilder.CreateLabel(settings.panel.transform, "SfxLabel", "Efectos", 48f, new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(600f, 70f), theme, theme.bodyFont);
        settings.sfxSlider = HudBuilder.CreateSlider(settings.panel.transform, "SfxSlider", new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(800f, 60f));
        UnityEventTools.AddPersistentListener(settings.musicSlider.onValueChanged, settings.OnSliderChanged);
        UnityEventTools.AddPersistentListener(settings.sfxSlider.onValueChanged, settings.OnSliderChanged);
        var settingsClose = HudBuilder.CreateButton(settings.panel.transform, "CloseButton", "Cerrar", new Vector2(0.5f, 0.5f), new Vector2(0f, -280f), new Vector2(480f, 120f), theme);
        UnityEventTools.AddPersistentListener(settingsClose.onClick, settings.Close);
        settings.panel.SetActive(false);

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();

        EditorSceneManager.SaveScene(scene, MainScenePath);
    }

    public static StorySequence Story(string assetName, string id, params string[] texts)
    {
        var sequence = LoadOrCreate<StorySequence>(StoriesFolder + "/" + assetName + ".asset");
        sequence.id = id;
        sequence.panels = new StoryPanel[texts.Length];
        for (int i = 0; i < texts.Length; i++)
            sequence.panels[i] = new StoryPanel { text = texts[i] };
        EditorUtility.SetDirty(sequence);
        return sequence;
    }

    public static SpawnEntry Entry(EnemyDefinition enemy, int count, float interval, float startDelay)
    {
        return new SpawnEntry { enemy = enemy, count = count, interval = interval, startDelay = startDelay };
    }

    static WaveDefinition Wave(int number, float delayBeforeNext, BossDefinition boss, float bossStartDelay, params SpawnEntry[] entries)
    {
        return WaveFor("1_1", number, delayBeforeNext, boss, bossStartDelay, entries);
    }

    public static WaveDefinition WaveFor(string levelKey, int number, float delayBeforeNext, BossDefinition boss, float bossStartDelay, params SpawnEntry[] entries)
    {
        var wave = LoadOrCreate<WaveDefinition>(LevelsFolder + "/Wave_" + levelKey + "_" + number + ".asset");
        wave.entries = entries;
        wave.delayBeforeNext = delayBeforeNext;
        wave.boss = boss;
        wave.bossStartDelay = bossStartDelay;
        EditorUtility.SetDirty(wave);
        return wave;
    }

    public static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }

    public static T LoadOrCreate<T>(string path) where T : ScriptableObject
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
