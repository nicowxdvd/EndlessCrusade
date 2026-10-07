namespace EC.Core
{
    public enum LevelOutcome { Victory, Defeat }

    public readonly struct WaveStarted
    {
        public readonly int Index;
        public readonly int Total;

        public WaveStarted(int index, int total) { Index = index; Total = total; }
    }

    public readonly struct LevelEnded
    {
        public readonly LevelOutcome Outcome;

        public LevelEnded(LevelOutcome outcome) { Outcome = outcome; }
    }
}
