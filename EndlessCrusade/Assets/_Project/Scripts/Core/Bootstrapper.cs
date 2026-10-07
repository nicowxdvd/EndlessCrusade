using UnityEngine;
using UnityEngine.SceneManagement;

namespace EC.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        const string MainScene = "LaneSandbox";

        void Start()
        {
            Debug.Log("[Boot] Endless Crusade iniciado");
            SceneManager.LoadScene(MainScene);
        }
    }
}
