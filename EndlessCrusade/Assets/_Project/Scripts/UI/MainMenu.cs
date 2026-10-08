using EC.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EC.UI
{
    public class MainMenu : MonoBehaviour
    {
        public LevelDefinition firstLevel;
        public string levelScene = "Level";

        public void Play()
        {
            LevelSession.Current = firstLevel;
            SceneManager.LoadScene(levelScene);
        }
    }
}
