using EC.Core;
using EC.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EC.Services
{
    public class MusicDirector : MonoBehaviour
    {
        static MusicDirector instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (instance != null)
                return;
            var go = new GameObject("MusicDirector");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicDirector>();
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus<BossSpawned>.Subscribe(OnBossSpawned);
            EventBus<LevelEnded>.Subscribe(OnLevelEnded);
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus<BossSpawned>.Unsubscribe(OnBossSpawned);
            EventBus<LevelEnded>.Unsubscribe(OnLevelEnded);
        }

        public static string MusicFor(string sceneName, string levelId)
        {
            switch (sceneName)
            {
                case "Level": return "music_ch" + ChapterOf(levelId);
                case "Boot":
                case "Main": return "music_main";
                default: return "music_map";
            }
        }

        public static int ChapterOf(string levelId)
        {
            if (!string.IsNullOrEmpty(levelId) && levelId.StartsWith("lv_") && levelId.Length > 3 && char.IsDigit(levelId[3]))
                return levelId[3] - '0';
            return 1;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var levelId = LevelSession.Current != null ? LevelSession.Current.id : null;
            EventBus<PlayMusic>.Publish(new PlayMusic(MusicFor(scene.name, levelId)));
            EventBus<PlayAmbient>.Publish(new PlayAmbient(scene.name == "Level" ? "ambient_rain" : ""));
        }

        void OnBossSpawned(BossSpawned evt)
        {
            EventBus<PlayMusic>.Publish(new PlayMusic("music_boss"));
        }

        void OnLevelEnded(LevelEnded evt)
        {
            if (evt.Outcome == LevelOutcome.Victory)
                EventBus<PlayMusic>.Publish(new PlayMusic("music_victory"));
        }
    }
}
