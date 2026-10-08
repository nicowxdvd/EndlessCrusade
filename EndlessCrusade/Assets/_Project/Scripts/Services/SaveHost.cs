using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public class SaveHost : MonoBehaviour
    {
        public static ISaveService Service { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (Service != null) return;
            Service = new JsonSaveService();
            Service.Load();

            var go = new GameObject("SaveHost");
            DontDestroyOnLoad(go);
            go.AddComponent<SaveHost>();
        }

        void OnApplicationPause(bool paused)
        {
            if (paused) Service.Save();
        }

        void OnApplicationQuit()
        {
            Service.Save();
        }
    }
}
