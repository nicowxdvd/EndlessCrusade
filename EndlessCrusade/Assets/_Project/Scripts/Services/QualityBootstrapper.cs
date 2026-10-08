using EC.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EC.Services
{
    public class QualityBootstrapper : MonoBehaviour
    {
        public const string PipelineFolder = "Quality/";

        static QualityBootstrapper instance;

        public static QualityTier Current { get; private set; } = QualityTier.High;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (instance != null)
                return;
            var go = new GameObject("QualityBootstrapper");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<QualityBootstrapper>();
            ApplyTier(Resolve());
        }

        void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            ApplyPostProcessing(Current);
        }

        public static QualityTier Resolve()
        {
            var automatic = QualityPolicy.Choose(SystemInfo.systemMemorySize, SystemInfo.graphicsMemorySize, SystemInfo.processorCount);
            var manual = SaveHost.Service != null ? SaveHost.Service.Current.settings.qualityOverride : 0;
            return QualityPolicy.Resolve(manual, automatic);
        }

        public static void SetOverride(int qualityOverride)
        {
            if (SaveHost.Service != null)
            {
                SaveHost.Service.Current.settings.qualityOverride = qualityOverride;
                SaveHost.Service.Save();
            }
            ApplyTier(Resolve());
        }

        public static void ApplyTier(QualityTier tier)
        {
            Current = tier;
            var pipeline = Resources.Load<UniversalRenderPipelineAsset>(PipelineFolder + "URP_" + tier);
            if (pipeline != null)
                QualitySettings.renderPipeline = pipeline;
            ApplyPostProcessing(tier);
        }

        static void ApplyPostProcessing(QualityTier tier)
        {
            foreach (var volume in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                var profile = volume.profile;
                if (profile == null)
                    continue;
                if (profile.TryGet(out UnityEngine.Rendering.Universal.Bloom bloom))
                    bloom.active = QualityPolicy.BloomEnabled(tier);
                if (profile.TryGet(out UnityEngine.Rendering.Universal.Vignette vignette))
                    vignette.active = QualityPolicy.VignetteEnabled(tier);
            }
        }
    }
}
