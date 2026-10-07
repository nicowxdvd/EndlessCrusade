using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class FontBuilder
{
    const string FontsFolder = "Assets/_Project/Art/Fonts";

    public static TMP_FontAsset LoadOrCreate(string name)
    {
        var assetPath = FontsFolder + "/" + name + " SDF.asset";
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (asset != null)
            return asset;

        var source = AssetDatabase.LoadAssetAtPath<Font>(FontsFolder + "/" + name + ".ttf");
        asset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        asset.name = name + " SDF";
        AssetDatabase.CreateAsset(asset, assetPath);
        asset.material.name = name + " Material";
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        foreach (var texture in asset.atlasTextures)
        {
            texture.name = name + " Atlas";
            AssetDatabase.AddObjectToAsset(texture, asset);
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return asset;
    }
}
