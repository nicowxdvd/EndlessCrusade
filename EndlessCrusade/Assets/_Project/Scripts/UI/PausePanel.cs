using UnityEngine;

namespace EC.UI
{
    public class PausePanel : MonoBehaviour
    {
        public GameObject panel;

        void OnDisable()
        {
            Time.timeScale = 1f;
        }

        public void Pause()
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
