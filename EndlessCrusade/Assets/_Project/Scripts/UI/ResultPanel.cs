using EC.Core;
using EC.Data;
using EC.Services;
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
        public UnityEngine.UI.Button doubleGoldButton;

        bool shown;
        bool resultVisible;
        bool victory;
        bool doubled;
        bool hasRewards;
        RewardsGranted rewards;
        bool firstClear;

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
            rewards = evt;
            hasRewards = true;
            if (rewardsLabel != null)
                rewardsLabel.text = FormatRewards(evt);
            RefreshDoubleGold();
        }

        void RefreshDoubleGold()
        {
            if (doubleGoldButton == null)
                return;
            var total = rewards.EnemyGold + rewards.LevelGold;
            var visible = resultVisible && victory && hasRewards && !doubled && total > 0 && SaveHost.Service != null && AdRewardService.Instance.IsReady(AdPlacement.DoubleGold);
            doubleGoldButton.gameObject.SetActive(visible);
        }

        public async void DoubleGold()
        {
            if (doubled || !hasRewards)
                return;
            doubled = true;
            var total = rewards.EnemyGold + rewards.LevelGold;
            RefreshDoubleGold();
            if (await AdRewardService.Instance.DoubleGoldAsync(total))
            {
                if (rewardsLabel != null)
                    rewardsLabel.text += "\nOro duplicado: +" + total;
                return;
            }
            doubled = false;
            RefreshDoubleGold();
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
            var level = LevelSession.Current;
            if (evt.Outcome != LevelOutcome.Victory || level == null || storyPlayer == null)
            {
                ShowResult(evt.Outcome);
                return;
            }

            var epilogue = firstClear ? level.epilogue : null;
            if (level.outro != null)
                storyPlayer.Play(level.outro, () => PlayEpilogue(epilogue, evt.Outcome));
            else
                PlayEpilogue(epilogue, evt.Outcome);
        }

        void PlayEpilogue(StorySequence epilogue, LevelOutcome outcome)
        {
            if (epilogue == null)
            {
                ShowResult(outcome);
                return;
            }
            storyPlayer.Play(epilogue, () => ShowResult(outcome));
        }

        void Start()
        {
            var level = LevelSession.Current;
            firstClear = level != null && SaveHost.Service != null && !SaveHost.Service.Current.progress.completedLevels.Contains(level.id);
        }

        void ShowResult(LevelOutcome outcome)
        {
            title.text = outcome == LevelOutcome.Victory ? victoryText : defeatText;
            victory = outcome == LevelOutcome.Victory;
            resultVisible = true;
            panel.SetActive(true);
            RefreshDoubleGold();
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
