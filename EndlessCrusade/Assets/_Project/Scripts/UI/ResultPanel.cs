using EC.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EC.UI
{
    public class ResultPanel : MonoBehaviour
    {
        public GameObject panel;
        public TMP_Text title;
        public string victoryText = "Victoria";
        public string defeatText = "Derrota";
        public string exitScene = "Main";

        bool shown;

        void OnEnable()
        {
            EventBus<LevelEnded>.Subscribe(OnLevelEnded);
        }

        void OnDisable()
        {
            EventBus<LevelEnded>.Unsubscribe(OnLevelEnded);
        }

        void OnLevelEnded(LevelEnded evt)
        {
            if (shown)
                return;
            shown = true;
            title.text = evt.Outcome == LevelOutcome.Victory ? victoryText : defeatText;
            panel.SetActive(true);
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(exitScene);
        }
    }
}
