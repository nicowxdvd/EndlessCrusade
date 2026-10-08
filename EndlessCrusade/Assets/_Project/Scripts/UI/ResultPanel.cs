using EC.Core;
using EC.Data;
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
        public string exitScene = "CampaignMap";
        public StoryPlayer storyPlayer;
        public TMP_Text rewardsLabel;

        bool shown;

        void OnEnable()
        {
            EventBus<LevelEnded>.Subscribe(OnLevelEnded);
            EventBus<RewardsGranted>.Subscribe(OnRewards);
        }

        void OnDisable()
        {
            EventBus<LevelEnded>.Unsubscribe(OnLevelEnded);
            EventBus<RewardsGranted>.Unsubscribe(OnRewards);
        }

        void OnRewards(RewardsGranted evt)
        {
            if (rewardsLabel != null)
                rewardsLabel.text = FormatRewards(evt);
        }

        public static string FormatRewards(RewardsGranted evt)
        {
            var text = "Oro por enemigos: " + evt.EnemyGold;
            if (evt.LevelGold > 0)
                text += "\nBono de nivel: " + evt.LevelGold;
            if (evt.Gems > 0)
                text += "\nReliquias: " + evt.Gems;
            if (evt.Tickets > 0)
                text += "\nBoletos: " + evt.Tickets;
            return text;
        }

        void OnLevelEnded(LevelEnded evt)
        {
            if (shown)
                return;
            shown = true;
            var outro = LevelSession.Current != null ? LevelSession.Current.outro : null;
            if (evt.Outcome == LevelOutcome.Victory && outro != null && storyPlayer != null)
            {
                storyPlayer.Play(outro, () => ShowResult(evt.Outcome));
                return;
            }
            ShowResult(evt.Outcome);
        }

        void ShowResult(LevelOutcome outcome)
        {
            title.text = outcome == LevelOutcome.Victory ? victoryText : defeatText;
            panel.SetActive(true);
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            SceneFlow.Load(SceneManager.GetActiveScene().name);
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            SceneFlow.Load(exitScene);
        }
    }
}
