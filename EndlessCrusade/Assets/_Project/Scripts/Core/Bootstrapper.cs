using UnityEngine;
using UnityEngine.SceneManagement;

namespace EC.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        const string MainScene = "Main";
        const int TargetFrameRate = 60;

        void Start()
        {
            Application.targetFrameRate = TargetFrameRate;
            Debug.Log("[Boot] Endless Crusade iniciado");
            SceneManager.LoadScene(MainScene);
        }
    }
}
