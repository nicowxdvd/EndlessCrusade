#pragma warning disable 618
using System.Collections.Generic;
using System.IO;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public static class HeroSpriteImporter
{
    public enum Style { Pixel, Illustrated }

    public struct ClipSpec
    {
        public string id;
        public int frames;
        public float fps;
        public bool loop;

        public ClipSpec(string id, int frames, float fps, bool loop)
        {
            this.id = id;
            this.frames = frames;
            this.fps = fps;
            this.loop = loop;
        }
    }

    public static readonly ClipSpec[] Clips =
    {
        new ClipSpec("idle", 4, 6f, true),
        new ClipSpec("move", 6, 10f, true),
        new ClipSpec("attack_sword", 5, 14f, false),
        new ClipSpec("attack_whip", 6, 14f, false),
        new ClipSpec("hurt", 2, 10f, false),
        new ClipSpec("dead", 6, 10f, false),
        new ClipSpec("summon", 5, 10f, false),
        new ClipSpec("holy_water", 5, 12f, false)
    };

    const string Root = "Assets/_Project/Art/Sprites/Hero";
    const string SetsFolder = "Assets/_Project/ScriptableObjects/Sprites";
    const string AtlasFolder = "Assets/_Project/Art/Atlases";

    public static int FrameSize(Style style) { return style == Style.Pixel ? 128 : 512; }
    public static float PixelsPerUnit(Style style) { return style == Style.Pixel ? 64f : 256f; }
    public static string StyleFolder(Style style) { return Root + "/" + style; }
    public static string SheetPath(Style style, string clip) { return StyleFolder(style) + "/Hero_" + style + "_" + clip + ".png"; }
    public static string FrameName(Style style, string clip, int index) { return "Hero_" + style + "_" + clip + "_" + index.ToString("00"); }
    public static string SetPath(Style style) { return SetsFolder + "/HeroSprites_" + style + ".asset"; }

    [MenuItem("EC/Art/Import Hero Pixel Sheets")]
    public static void ImportPixel() { Import(Style.Pixel); }

    [MenuItem("EC/Art/Import Hero Illustrated Sheets")]
    public static void ImportIllustrated() { Import(Style.Illustrated); }

    public static SpriteAnimationSet Import(Style style)
    {
        EnsureFolder("Assets/_Project/ScriptableObjects", "Sprites");
        EnsureFolder("Assets/_Project/Art", "Atlases");
        EnsureFolder("Assets/_Project/Art/Sprites", "Hero");
        EnsureFolder(Root, style.ToString());

        var clips = new List<SpriteClip>();
        var allSprites = new List<Object>();
        foreach (var spec in Clips)
        {
            var path = SheetPath(style, spec.id);
            if (!File.Exists(path))
            {
                Debug.LogWarning("[Arte] Falta " + path);
                continue;
            }

            ConfigureSheet(path, style, spec);
            var frames = new List<Sprite>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                if (asset is Sprite sprite)
                    frames.Add(sprite);
            frames.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            if (frames.Count != spec.frames)
                Debug.LogWarning("[Arte] " + path + " tiene " + frames.Count + " frames, se esperaban " + spec.frames);

            clips.Add(new SpriteClip { id = spec.id, frames = frames.ToArray(), framesPerSecond = spec.fps, loop = spec.loop });
            allSprites.AddRange(frames);
        }

        var set = AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(SetPath(style));
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<SpriteAnimationSet>();
            AssetDatabase.CreateAsset(set, SetPath(style));
        }
        set.clips = clips.ToArray();
        EditorUtility.SetDirty(set);

        BuildAtlas(style, allSprites);
        AssetDatabase.SaveAssets();
        return set;
    }

    static void ConfigureSheet(string path, Style style, ClipSpec spec)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = PixelsPerUnit(style);
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = style == Style.Pixel ? FilterMode.Point : FilterMode.Bilinear;

        var android = importer.GetPlatformTextureSettings("Android");
        android.overridden = true;
        android.format = style == Style.Pixel ? TextureImporterFormat.RGBA32 : TextureImporterFormat.ASTC_6x6;
        importer.SetPlatformTextureSettings(android);
        importer.textureCompression = style == Style.Pixel ? TextureImporterCompression.Uncompressed : TextureImporterCompression.Compressed;

        var size = FrameSize(style);
        var metas = new SpriteMetaData[spec.frames];
        for (int i = 0; i < spec.frames; i++)
        {
            metas[i] = new SpriteMetaData
            {
                name = FrameName(style, spec.id, i),
                rect = new Rect(i * size, 0f, size, size),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(0.5f, 0f)
            };
        }
        importer.spritesheet = metas;
        importer.SaveAndReimport();
    }

    static void BuildAtlas(Style style, List<Object> sprites)
    {
        var path = AtlasFolder + "/HeroAtlas_" + style + ".spriteatlas";
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
        if (atlas == null)
        {
            atlas = new SpriteAtlas();
            AssetDatabase.CreateAsset(atlas, path);
        }
        var existing = atlas.GetPackables();
        if (existing != null && existing.Length > 0)
            atlas.Remove(existing);
        if (sprites.Count > 0)
            atlas.Add(sprites.ToArray());

        var settings = atlas.GetPackingSettings();
        settings.enableRotation = false;
        settings.enableTightPacking = false;
        atlas.SetPackingSettings(settings);
        EditorUtility.SetDirty(atlas);
    }

    public static SpriteAnimationSet LoadSet(Style style)
    {
        return AssetDatabase.LoadAssetAtPath<SpriteAnimationSet>(SetPath(style));
    }

    [MenuItem("EC/Art/Spawn Hero Style Comparison")]
    public static void SpawnComparison()
    {
        var offset = 0f;
        foreach (Style style in System.Enum.GetValues(typeof(Style)))
        {
            var set = LoadSet(style);
            if (set == null)
            {
                Debug.LogWarning("[Arte] Sin sprites importados para " + style);
                continue;
            }

            var go = new GameObject("HeroStyle_" + style);
            go.transform.position = new Vector3(offset, 0.5f, 0f);
            var renderer = go.AddComponent<SpriteRenderer>();
            var animator = go.AddComponent<SpriteStateAnimator>();
            animator.animations = set;
            animator.target = renderer;
            go.AddComponent<BillboardSprite>();
            offset += 3f;
        }
    }

    public static SpriteAnimationSet ChosenSet()
    {
        var chosen = LoadSet(Style.Illustrated);
        if (chosen != null && chosen.clips != null && chosen.clips.Length > 0 && EditorPrefs.GetString("EC.HeroStyle", "Illustrated") == "Illustrated")
            return chosen;
        var pixel = LoadSet(Style.Pixel);
        return pixel != null && pixel.clips != null && pixel.clips.Length > 0 ? pixel : null;
    }

    [MenuItem("EC/Art/Choose Style Illustrated")]
    static void ChooseIllustrated() { EditorPrefs.SetString("EC.HeroStyle", "Illustrated"); }

    [MenuItem("EC/Art/Choose Style Pixel")]
    static void ChoosePixel() { EditorPrefs.SetString("EC.HeroStyle", "Pixel"); LevelBuilderRefreshHint(); }

    static void LevelBuilderRefreshHint()
    {
        Debug.Log("[Arte] Estilo Pixel elegido. Regenera el prefab del héroe con EC/Level/Build Level 1.");
    }

    static void EnsureFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
            AssetDatabase.CreateFolder(parent, name);
    }
}
