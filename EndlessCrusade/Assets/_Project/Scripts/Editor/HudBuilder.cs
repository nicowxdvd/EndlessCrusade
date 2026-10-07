using EC.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEditor.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class HudBuilder
{
    const string ThemeFolder = "Assets/_Project/ScriptableObjects/UI";
    const string ThemePath = ThemeFolder + "/UiTheme_Gothic.asset";
    const string PrefabPath = "Assets/_Project/Prefabs/HudCanvas.prefab";
    const float ButtonSize = 180f;
    const float Margin = 40f;

    [MenuItem("EC/Sandbox/Build HUD Assets")]
    public static void BuildAssets()
    {
        var theme = LoadOrCreateTheme();
        var root = CreateHud(theme);
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
    }

    public static void SpawnHud()
    {
        BuildAssets();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "HudCanvas";

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    static UiTheme LoadOrCreateTheme()
    {
        if (!AssetDatabase.IsValidFolder(ThemeFolder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "UI");
        var theme = AssetDatabase.LoadAssetAtPath<UiTheme>(ThemePath);
        if (theme == null)
        {
            theme = ScriptableObject.CreateInstance<UiTheme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
        }
        theme.titleFont = FontBuilder.LoadOrCreate("Cinzel");
        theme.bodyFont = FontBuilder.LoadOrCreate("EBGaramond");
        EditorUtility.SetDirty(theme);
        return theme;
    }

    static GameObject CreateHud(UiTheme theme)
    {
        var root = new GameObject("HudCanvas");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var safe = NewRect("SafeArea", root.transform);
        Stretch(safe);
        safe.gameObject.AddComponent<SafeAreaFitter>();

        var presenter = root.AddComponent<HudPresenter>();
        presenter.heroBar = CreateBar(safe, "HeroBar", "Vida", new Vector2(0f, 1f), new Vector2(Margin, -Margin), theme.blood, theme);
        presenter.baseBar = CreateBar(safe, "BaseBar", "Base", new Vector2(1f, 1f), new Vector2(-Margin, -Margin), theme.gold, theme);
        presenter.waveLabel = CreateLabel(safe, "WaveLabel", "", 48f, new Vector2(0.5f, 1f), new Vector2(0f, -Margin), new Vector2(600f, 80f), theme, theme.titleFont);
        presenter.bossBar = CreateBar(safe, "BossBar", "Jefe", new Vector2(0.5f, 1f), new Vector2(0f, -Margin * 2f - 80f), theme.blood, theme);
        presenter.bossBar.fillOrigin = 0;
        presenter.bossRoot = presenter.bossBar.transform.parent.gameObject;
        presenter.bossLabel = presenter.bossRoot.transform.Find("Caption").GetComponent<TMP_Text>();
        presenter.bossRoot.SetActive(false);

        CreateTouchButton(safe, "Left", "<", "<Gamepad>/dpad/left", new Vector2(0f, 0f), new Vector2(Margin + ButtonSize * 0.5f, Margin + ButtonSize * 0.5f), theme);
        CreateTouchButton(safe, "Right", ">", "<Gamepad>/dpad/right", new Vector2(0f, 0f), new Vector2(Margin * 2f + ButtonSize * 1.5f, Margin + ButtonSize * 0.5f), theme);
        CreateTouchButton(safe, "Sword", "Espada", "<Gamepad>/buttonWest", new Vector2(1f, 0f), new Vector2(-(Margin * 2f + ButtonSize * 1.5f), Margin + ButtonSize * 0.5f), theme);
        CreateTouchButton(safe, "Whip", "Látigo", "<Gamepad>/buttonEast", new Vector2(1f, 0f), new Vector2(-(Margin + ButtonSize * 0.5f), Margin + ButtonSize * 0.5f), theme);

        CreateAbilityButton(safe, theme);

        var pause = root.AddComponent<PausePanel>();
        var pauseButton = CreateButton(safe, "PauseButton", "II", new Vector2(1f, 1f), new Vector2(-Margin, -Margin * 2f - 60f), new Vector2(120f, 120f), theme);
        var pauseOutline = pauseButton.gameObject.AddComponent<Outline>();
        pauseOutline.effectColor = theme.gold;
        pauseOutline.effectDistance = new Vector2(3f, -3f);
        pause.panel = CreateOverlay(safe, "PausePanel", theme);
        var pauseTitle = CreateLabel(pause.panel.transform, "Title", "Pausa", 96f, new Vector2(0.5f, 0.5f), new Vector2(0f, 160f), new Vector2(800f, 140f), theme, theme.titleFont);
        pauseTitle.color = theme.gold;
        var resume = CreateButton(pause.panel.transform, "ResumeButton", "Reanudar", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(480f, 120f), theme);
        UnityEventTools.AddPersistentListener(pauseButton.onClick, pause.Pause);
        UnityEventTools.AddPersistentListener(resume.onClick, pause.Resume);
        pause.panel.SetActive(false);

        var result = root.AddComponent<ResultPanel>();
        result.panel = CreateOverlay(safe, "ResultPanel", theme);
        result.title = CreateLabel(result.panel.transform, "Title", "", 120f, new Vector2(0.5f, 0.5f), new Vector2(0f, 180f), new Vector2(1000f, 180f), theme, theme.titleFont);
        result.title.color = theme.gold;
        var retry = CreateButton(result.panel.transform, "RetryButton", "Reintentar", new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(480f, 120f), theme);
        var exit = CreateButton(result.panel.transform, "ExitButton", "Salir", new Vector2(0.5f, 0.5f), new Vector2(0f, -200f), new Vector2(480f, 120f), theme);
        UnityEventTools.AddPersistentListener(retry.onClick, result.Retry);
        UnityEventTools.AddPersistentListener(exit.onClick, result.Exit);
        result.panel.SetActive(false);

        return root;
    }

    static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static Image CreateBar(Transform parent, string name, string caption, Vector2 anchor, Vector2 position, Color fillColor, UiTheme theme)
    {
        var frame = NewRect(name, parent);
        frame.anchorMin = anchor;
        frame.anchorMax = anchor;
        frame.pivot = anchor;
        frame.anchoredPosition = position;
        frame.sizeDelta = new Vector2(520f, 44f);
        var back = frame.gameObject.AddComponent<Image>();
        back.color = theme.background;
        back.raycastTarget = false;

        var fill = NewRect("Fill", frame);
        Stretch(fill);
        fill.offsetMin = new Vector2(4f, 4f);
        fill.offsetMax = new Vector2(-4f, -4f);
        var image = fill.gameObject.AddComponent<Image>();
        image.color = fillColor;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = anchor.x < 0.5f ? 0 : 1;
        image.fillAmount = 1f;
        image.raycastTarget = false;

        var label = CreateLabel(frame, "Caption", caption, 36f, new Vector2(0.5f, 0.5f), Vector2.zero, frame.sizeDelta, theme, theme.bodyFont);
        label.alignment = TextAlignmentOptions.Center;
        return image;
    }

    static TMP_Text CreateLabel(Transform parent, string name, string content, float size, Vector2 anchor, Vector2 position, Vector2 dimensions, UiTheme theme, TMP_FontAsset font)
    {
        var rect = NewRect(name, parent);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor.y > 0.9f ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.color = theme.text;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        if (font != null)
            text.font = font;
        return text;
    }

    static Button CreateButton(Transform parent, string name, string caption, Vector2 anchor, Vector2 position, Vector2 dimensions, UiTheme theme)
    {
        var rect = NewRect(name, parent);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor.y > 0.9f ? new Vector2(1f, 1f) : new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(theme.background.r, theme.background.g, theme.background.b, 0.85f);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var label = CreateLabel(rect, "Label", caption, 44f, new Vector2(0.5f, 0.5f), Vector2.zero, dimensions, theme, theme.bodyFont);
        label.color = theme.gold;
        return button;
    }

    static void CreateTouchButton(Transform parent, string name, string caption, string controlPath, Vector2 anchor, Vector2 position, UiTheme theme)
    {
        var rect = NewRect(name, parent);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(ButtonSize, ButtonSize);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(theme.gold.r, theme.gold.g, theme.gold.b, 0.35f);
        rect.gameObject.AddComponent<OnScreenButton>().controlPath = controlPath;
        var label = CreateLabel(rect, "Label", caption, 40f, new Vector2(0.5f, 0.5f), Vector2.zero, rect.sizeDelta, theme, theme.bodyFont);
        label.color = Color.white;
    }

    static void CreateAbilityButton(Transform parent, UiTheme theme)
    {
        var ability = AbilityBuilder.BuildAssets();
        var position = new Vector2(-(Margin * 3f + ButtonSize * 2.5f), Margin + ButtonSize * 0.5f);
        var button = CreateButton(parent, "Ability1", ability.displayName, new Vector2(1f, 0f), position, new Vector2(ButtonSize, ButtonSize), theme);
        button.GetComponentInChildren<TMP_Text>().fontSize = 32f;

        var fillRect = NewRect("CooldownFill", button.transform);
        Stretch(fillRect);
        fillRect.SetSiblingIndex(1);
        var fill = fillRect.gameObject.AddComponent<Image>();
        fill.sprite = HeroBuilder.LoadPlaceholderSprite();
        fill.color = new Color(0f, 0f, 0f, 0.65f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Radial360;
        fill.fillOrigin = (int)Image.Origin360.Top;
        fill.fillClockwise = false;
        fill.fillAmount = 0f;
        fill.raycastTarget = false;

        var ability1 = button.gameObject.AddComponent<AbilityButton>();
        ability1.slot = 0;
        ability1.button = button;
        ability1.cooldownFill = fill;
    }

    static GameObject CreateOverlay(Transform parent, string name, UiTheme theme)
    {
        var rect = NewRect(name, parent);
        Stretch(rect);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(theme.background.r, theme.background.g, theme.background.b, 0.8f);
        return rect.gameObject;
    }
}
