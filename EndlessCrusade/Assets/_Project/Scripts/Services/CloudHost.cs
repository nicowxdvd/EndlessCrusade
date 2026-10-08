using System.Collections.Generic;
using System.Threading.Tasks;
using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public class CloudHost : MonoBehaviour
    {
        public const int SyncTimeoutMs = 10000;

        static CloudHost instance;

        int highestWave;
        bool syncing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (instance != null)
                return;
            var go = new GameObject("CloudHost");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<CloudHost>();
        }

        void OnEnable()
        {
            EventBus<WavesStartRequested>.Subscribe(OnLevelStart);
            EventBus<WaveStarted>.Subscribe(OnWaveStarted);
            EventBus<LevelEnded>.Subscribe(OnLevelEnded);
            EventBus<TutorialCompleted>.Subscribe(OnTutorialCompleted);
            EventBus<UpgradePurchased>.Subscribe(OnUpgradePurchased);
        }

        void OnDisable()
        {
            EventBus<WavesStartRequested>.Unsubscribe(OnLevelStart);
            EventBus<WaveStarted>.Unsubscribe(OnWaveStarted);
            EventBus<LevelEnded>.Unsubscribe(OnLevelEnded);
            EventBus<TutorialCompleted>.Unsubscribe(OnTutorialCompleted);
            EventBus<UpgradePurchased>.Unsubscribe(OnUpgradePurchased);
        }

        void Start()
        {
            Sync();
        }

        void OnLevelStart(WavesStartRequested evt)
        {
            highestWave = 0;
            CloudServices.Analytics.Log("level_start", new Dictionary<string, object> { { "level_id", LevelId() } });
        }

        void OnWaveStarted(WaveStarted evt)
        {
            highestWave = evt.Index + 1;
        }

        void OnLevelEnded(LevelEnded evt)
        {
            CloudServices.Analytics.Log("level_end", new Dictionary<string, object>
            {
                { "level_id", LevelId() },
                { "result", evt.Outcome == LevelOutcome.Victory ? "victory" : "defeat" },
                { "wave_reached", highestWave }
            });
            Sync();
        }

        void OnTutorialCompleted(TutorialCompleted evt)
        {
            CloudServices.Analytics.Log("tutorial_complete");
        }

        void OnUpgradePurchased(UpgradePurchased evt)
        {
            CloudServices.Analytics.Log("upgrade_purchased", new Dictionary<string, object> { { "upgrade_id", evt.Id }, { "level", evt.Level } });
        }

        static string LevelId()
        {
            return LevelSession.Current != null ? LevelSession.Current.id : "unknown";
        }

        public async void Sync()
        {
            if (syncing || SaveHost.Service == null)
                return;
            syncing = true;
            try
            {
                var cloud = new CloudSaveService(CloudServices.Auth, CloudServices.Store, SaveHost.Service, Application.persistentDataPath);
                var work = cloud.SyncAsync();
                if (await Task.WhenAny(work, Task.Delay(SyncTimeoutMs)) != work)
                    Debug.LogWarning("[Cloud] Sincronización agotó el tiempo");
            }
            finally
            {
                syncing = false;
            }
        }
    }
}
