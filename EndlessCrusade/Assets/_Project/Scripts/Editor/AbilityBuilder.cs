using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class AbilityBuilder
{
    const string AbilitiesFolder = "Assets/_Project/ScriptableObjects/Abilities";
    const string AbilityPath = AbilitiesFolder + "/Ability_HolyWater.asset";
    const string PrefabsFolder = "Assets/_Project/Prefabs";
    const string MaterialsFolder = "Assets/_Project/Art/Materials";
    const string ProjectilePath = PrefabsFolder + "/HolyWaterProjectile.prefab";
    const string ExplosionPath = PrefabsFolder + "/HolyWaterExplosion.prefab";

    [MenuItem("EC/Sandbox/Build Ability Assets")]
    public static AbilityDefinition BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(AbilitiesFolder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Abilities");
        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            AssetDatabase.CreateFolder("Assets/_Project/Art", "Materials");

        var ability = AssetDatabase.LoadAssetAtPath<AbilityDefinition>(AbilityPath);
        if (ability == null)
        {
            ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            AssetDatabase.CreateAsset(ability, AbilityPath);
        }
        ability.id = "holy_water";
        ability.displayName = "Agua bendita";
        ability.kind = AbilityKind.ThrownArea;
        ability.cooldown = 12f;
        ability.damage = 40;
        ability.radius = 2.5f;
        ability.undeadMultiplier = 2f;
        ability.throwDistance = 6f;
        ability.projectilePrefab = CreateProjectilePrefab();
        ability.explosionPrefab = CreateExplosionPrefab();
        EditorUtility.SetDirty(ability);
        AssetDatabase.SaveAssets();
        return ability;
    }

    static GameObject CreateProjectilePrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HolyWaterProjectile";
        go.transform.localScale = Vector3.one * 0.35f;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial("HolyWater", new Color(0.5f, 0.8f, 1f));
        go.AddComponent<HolyWaterProjectile>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static GameObject CreateExplosionPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "HolyWaterExplosion";
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial("HolyWaterExplosion", new Color(0.6f, 0.9f, 1f));
        go.AddComponent<ExplosionEffect>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, ExplosionPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static Material ColorMaterial(string name, Color color)
    {
        var path = MaterialsFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }
}
