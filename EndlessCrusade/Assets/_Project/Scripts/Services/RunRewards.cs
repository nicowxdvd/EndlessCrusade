using EC.Core;
using EC.Data;

namespace EC.Services
{
    public readonly struct RewardSummary
    {
        public readonly int EnemyGold;
        public readonly int LevelGold;
        public readonly int Gems;
        public readonly int Tickets;
        public readonly bool FirstClear;

        public RewardSummary(int enemyGold, int levelGold, int gems, int tickets, bool firstClear)
        {
            EnemyGold = enemyGold;
            LevelGold = levelGold;
            Gems = gems;
            Tickets = tickets;
            FirstClear = firstClear;
        }
    }

    public static class RunRewards
    {
        public const int DefeatGoldPercent = 50;

        public static RewardSummary Compute(LevelReward reward, bool alreadyCompleted, LevelOutcome outcome, int enemyGold)
        {
            if (outcome == LevelOutcome.Defeat)
                return new RewardSummary(enemyGold * DefeatGoldPercent / 100, 0, 0, 0, false);

            reward = reward ?? new LevelReward();
            var firstClear = !alreadyCompleted;
            return new RewardSummary(
                enemyGold,
                firstClear ? reward.goldFirstClear : reward.goldReplay,
                firstClear ? reward.gemsFirstClear : 0,
                firstClear ? reward.ticketsFirstClear : 0,
                firstClear);
        }

        public static RewardSummary Apply(LevelDefinition level, LevelOutcome outcome, int enemyGold, ISaveService save, CurrencyService currency)
        {
            var completed = level != null && save.Current.progress.completedLevels.Contains(level.id);
            var summary = Compute(level != null ? level.reward : null, completed, outcome, enemyGold);

            currency.Add(CurrencyType.Gold, summary.EnemyGold + summary.LevelGold);
            currency.Add(CurrencyType.Gems, summary.Gems);
            currency.Add(CurrencyType.Tickets, summary.Tickets);

            if (outcome == LevelOutcome.Victory && level != null && !completed)
            {
                save.Current.progress.completedLevels.Add(level.id);
                save.Save();
            }
            return summary;
        }
    }
}
