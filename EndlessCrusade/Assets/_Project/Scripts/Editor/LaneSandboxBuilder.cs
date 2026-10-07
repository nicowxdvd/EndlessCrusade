using EC.Data;
using EC.Gameplay;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class LaneSandboxBuilder
{
    const string ConfigPath = "Assets/_Project/ScriptableObjects/Lane/LaneConfig_Default.asset";
    const string ScenePath = "Assets/_Project/Scenes/Sandbox/LaneSandbox.unity";
    const string VolumeProfilePath = "Assets/_Project/Settings/GlobalVolumeProfile.asset";
    const float CameraDistance = 10f;

    [MenuItem("EC/Sandbox/Build LaneSandbox Scene")]
    public static void Build()
    {
        var config = AssetDatabase.LoadAssetAtPath<LaneConfig>(ConfigPath);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateGround(config);
        var cam = CreateCamera(config);
        CreateFollowCamera(config, cam);
        CreateBackground(config);
        CreateRain(cam);
        CreateLights(config);
        CreateVolume();
        EntityDummyBuilder.SpawnDummies(config);
        HeroBuilder.SpawnHero(config);

        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    static void CreateRain(Camera cam)
    {
        var go = new GameObject("Rain");
        go.transform.SetParent(cam.transform, false);
        go.transform.localPosition = new Vector3(0f, 6f, 2f);
        go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.maxParticles = 800;
        main.loop = true;
        main.startLifetime = 1f;
        main.startSpeed = 12f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.7f, 0.8f, 1f, 0.6f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 600f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 6f, 0.1f);

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 4f;
        renderer.sharedMaterial = RainMaterial();
    }

    static Material RainMaterial()
    {
        const string path = "Assets/_Project/Art/Materials/Lane_Rain.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            LayerMaterial("Rain", Color.white);
            material = AssetDatabase.LoadAssetAtPath<Material>(path);
        }
        material.shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        material.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void CreateVolume()
    {
        var go = new GameObject("Global Volume");
        var volume = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
    }

    static void CreateLights(LaneConfig config)
    {
        var moon = new GameObject("Moon Light");
        moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var moonLight = moon.AddComponent<Light>();
        moonLight.type = LightType.Directional;
        moonLight.color = new Color(0.4f, 0.5f, 0.9f);
        moonLight.intensity = 0.8f;

        var lantern = new GameObject("Lantern Light");
        lantern.transform.position = new Vector3(config.baseX + 6f, config.groundY + 2.5f, -1f);
        var lanternLight = lantern.AddComponent<Light>();
        lanternLight.type = LightType.Point;
        lanternLight.color = new Color(1f, 0.7f, 0.35f);
        lanternLight.intensity = 3f;
        lanternLight.range = 9f;
    }

    static void CreateBackground(LaneConfig config)
    {
        var cameraRange = 2f * LaneCameraClamp.MaxCenterX(config.laneHalfLength, config.cameraOrthoSize, 16f / 9f);
        var width = 2f * config.laneHalfLength + cameraRange;
        var root = new GameObject("Background").transform;

        var far = CreateLayer(root, "Layer Far", 0.2f, 20f);
        CreateQuad(far, "Sky", new Vector3(0f, 4f, 0f), new Vector2(width, 20f), LayerMaterial("Sky", new Color(0.05f, 0.08f, 0.18f)));
        CreateQuad(far, "Moon", new Vector3(-6f, 8f, -0.1f), new Vector2(2f, 2f), LayerMaterial("Moon", new Color(0.85f, 0.9f, 1f)));

        var mid = CreateLayer(root, "Layer Mid", 0.5f, 14f);
        var castleMaterial = LayerMaterial("Castle", new Color(0.08f, 0.09f, 0.14f));
        for (var x = -width / 2f + 4f; x < width / 2f; x += 8f)
            CreateQuad(mid, "Castle", new Vector3(x, 3f, 0f), new Vector2(8.2f, 6f + 3f * Mathf.Abs(Mathf.Sin(x))), castleMaterial);

        var near = CreateLayer(root, "Layer Near", 0.8f, 8f);
        var treeMaterial = LayerMaterial("Tree", new Color(0.03f, 0.05f, 0.05f));
        for (var x = -width / 2f + 1.5f; x < width / 2f; x += 3f)
            CreateQuad(near, "Tree", new Vector3(x, 2f, 0f), new Vector2(3.1f, 4f + 2f * Mathf.Abs(Mathf.Cos(x * 1.7f))), treeMaterial);
    }

    static Transform CreateLayer(Transform parent, string name, float factor, float z)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(0f, 0f, z);
        go.AddComponent<ParallaxLayer>().factor = factor;
        return go.transform;
    }

    static void CreateQuad(Transform parent, string name, Vector3 localPosition, Vector2 size, Material material)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        Object.DestroyImmediate(quad.GetComponent<Collider>());
        quad.transform.SetParent(parent);
        quad.transform.localPosition = localPosition;
        quad.transform.localScale = new Vector3(size.x, size.y, 1f);
        quad.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    static Material LayerMaterial(string name, Color color)
    {
        const string folder = "Assets/_Project/Art/Materials";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Art", "Materials");

        var path = $"{folder}/Lane_{name}.mat";
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

    static void CreateGround(LaneConfig config)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, config.groundY, 0f);
        ground.transform.localScale = new Vector3(2f * config.laneHalfLength / 10f, 1f, config.laneDepth / 10f);
    }

    static Camera CreateCamera(LaneConfig config)
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        go.AddComponent<AudioListener>();
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = config.cameraOrthoSize;
        var pitch = Quaternion.Euler(config.cameraPitch, 0f, 0f);
        go.transform.rotation = pitch;
        go.transform.position = new Vector3(0f, config.groundY, 0f) + pitch * Vector3.back * CameraDistance;
        go.AddComponent<CinemachineBrain>();
        return cam;
    }

    static void CreateFollowCamera(LaneConfig config, Camera cam)
    {
        var target = new GameObject("CameraTarget");
        target.transform.position = new Vector3(0f, config.groundY, 0f);
        target.AddComponent<SandboxCameraDriver>();

        var pitch = Quaternion.Euler(config.cameraPitch, 0f, 0f);
        var go = new GameObject("CM Lane Camera");
        go.transform.rotation = pitch;
        go.transform.position = target.transform.position + pitch * Vector3.back * CameraDistance;

        var vcam = go.AddComponent<CinemachineCamera>();
        var lens = LensSettings.FromCamera(cam);
        lens.ModeOverride = LensSettings.OverrideModes.Orthographic;
        lens.OrthographicSize = config.cameraOrthoSize;
        vcam.Lens = lens;
        vcam.Target.TrackingTarget = target.transform;

        var follow = go.AddComponent<CinemachineFollow>();
        follow.FollowOffset = pitch * Vector3.back * CameraDistance;

        var clamp = go.AddComponent<LaneCameraClamp>();
        clamp.config = config;
    }
}
