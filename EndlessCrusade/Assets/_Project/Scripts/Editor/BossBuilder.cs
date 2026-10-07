using EC.Core;
using EC.Data;
using EC.Gameplay;
using UnityEditor;
using UnityEngine;

public static class BossBuilder
{
    const string UnitsFolder = "Assets/_Project/ScriptableObjects/Units";
    const string PrefabsFolder = "Assets/_Project/Prefabs";
    const string MaterialsFolder = "Assets/_Project/Art/Materials";
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";
    const string DefinitionPath = UnitsFolder + "/Boss_GiantLycanthrope.asset";

    [MenuItem("EC/Sandbox/Build Boss")]
    public static BossDefinition BuildAssets()
    {
        if (!AssetDatabase.IsValidFolder(UnitsFolder))
            AssetDatabase.CreateFolder("Assets/_Project/ScriptableObjects", "Units");
        if (!AssetDatabase.IsValidFolder(MaterialsFolder))
            AssetDatabase.CreateFolder("Assets/_Project/Art", "Materials");

        var definition = AssetDatabase.LoadAssetAtPath<BossDefinition>(DefinitionPath);
        if (definition == null)
        {
            definition = ScriptableObject.CreateInstance<BossDefinition>();
            AssetDatabase.CreateAsset(definition, DefinitionPath);
        }
        definition.id = "boss_giant_lycanthrope";
        definition.displayName = "Licantropo Gigante";
        definition.team = Team.Enemy;
        definition.kind = EnemyKind.Ground;
        definition.maxHealth = 600;
        definition.attackDamage = 20;
        definition.moveSpeed = 1.6f;
        definition.attackRange = 2.2f;
        definition.attackCooldown = 1.2f;
        definition.hurtDuration = 0.1f;
        definition.chargeRange = 8f;
        definition.chargeSpeedMultiplier = 4f;
        definition.goldDrop = 100;
        definition.chargeWindup = 1f;
        definition.chargeDamage = 35;
        definition.stunAfterCharge = 1.5f;
        definition.roarInterval = 12f;
        definition.roarRadius = 8f;
        definition.disorientDuration = 3f;
        definition.enrageThreshold = 0.5f;
        definition.enrageSpeedMultiplier = 1.4f;
        definition.prefab = CreatePrefab(definition);
        EditorUtility.SetDirty(definition);
        AssetDatabase.SaveAssets();
        return definition;
    }

    static GameObject CreatePrefab(BossDefinition definition)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Boss_GiantLycanthrope";
        go.transform.localScale = new Vector3(2.4f, 1.6f, 2.4f);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = ColorMaterial("Boss_GiantLycanthrope", new Color(0.25f, 0.2f, 0.2f));
        go.AddComponent<EntityController>().definition = definition;
        go.AddComponent<HealthComponent>();
        var movement = go.AddComponent<MovementComponent>();
        movement.lane = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        go.AddComponent<AttackComponent>();
        var brain = go.AddComponent<BossBrain>();
        brain.telegraphMaterial = ColorMaterial("Boss_ChargeTelegraph", new Color(0.85f, 0.1f, 0.1f));
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabsFolder + "/Boss_GiantLycanthrope.prefab");
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
