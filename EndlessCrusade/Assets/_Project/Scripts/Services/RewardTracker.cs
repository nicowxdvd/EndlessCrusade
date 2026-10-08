using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public class RewardTracker : MonoBehaviour
    {
        int enemyGold;
        bool settled;

        public int EnemyGold => enemyGold;

        void OnEnable()
        {
            EventBus<GoldDropped>.Subscribe(OnGoldDropped);
            EventBus<LevelEnded>.Subscribe(OnLevelEnded);
        }

        void OnDisable()
        {
            EventBus<GoldDropped>.Unsubscribe(OnGoldDropped);
            EventBus<LevelEnded>.Unsubscribe(OnLevelEnded);
        }

        void OnGoldDropped(GoldDropped evt)
        {
            enemyGold += evt.Amount;
        }

        void OnLevelEnded(LevelEnded evt)
        {
            if (settled)
                return;
            settled = true;
            var summary = RunRewards.Apply(LevelSession.Current, evt.Outcome, enemyGold, SaveHost.Service, CurrencyService.Instance);
            EventBus<RewardsGranted>.Publish(new RewardsGranted(summary.EnemyGold, summary.LevelGold, summary.Gems, summary.Tickets, summary.FirstClear));
        }
    }
}
