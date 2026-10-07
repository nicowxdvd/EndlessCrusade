namespace EC.Core
{
    public readonly struct LeadershipChanged
    {
        public readonly float Current;
        public readonly float Max;

        public LeadershipChanged(float current, float max)
        {
            Current = current;
            Max = max;
        }
    }

    public readonly struct TroopSummonRequested
    {
        public readonly string TroopId;

        public TroopSummonRequested(string troopId)
        {
            TroopId = troopId;
        }
    }

    public readonly struct TroopSummoned
    {
        public readonly string TroopId;
        public readonly float Cooldown;

        public TroopSummoned(string troopId, float cooldown)
        {
            TroopId = troopId;
            Cooldown = cooldown;
        }
    }

    public readonly struct TroopsAvailable
    {
        public readonly bool Enabled;

        public TroopsAvailable(bool enabled)
        {
            Enabled = enabled;
        }
    }
}
