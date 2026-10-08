namespace EC.Core
{
    public enum CurrencyType { Gold, Gems, Tickets }

    public readonly struct CurrencyChanged
    {
        public readonly CurrencyType Type;
        public readonly int Balance;

        public CurrencyChanged(CurrencyType type, int balance) { Type = type; Balance = balance; }
    }

    public readonly struct GoldDropped
    {
        public readonly int Amount;

        public GoldDropped(int amount) { Amount = amount; }
    }

    public readonly struct RewardsGranted
    {
        public readonly int EnemyGold;
        public readonly int LevelGold;
        public readonly int Gems;
        public readonly int Tickets;
        public readonly bool FirstClear;

        public RewardsGranted(int enemyGold, int levelGold, int gems, int tickets, bool firstClear)
        {
            EnemyGold = enemyGold;
            LevelGold = levelGold;
            Gems = gems;
            Tickets = tickets;
            FirstClear = firstClear;
        }
    }
}
