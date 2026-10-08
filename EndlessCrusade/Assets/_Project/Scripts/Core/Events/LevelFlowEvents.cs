namespace EC.Core
{
    public enum HeroAction { Move, Sword, Whip }

    public readonly struct HeroActed
    {
        public readonly HeroAction Action;

        public HeroActed(HeroAction action) { Action = action; }
    }

    public readonly struct WavesStartRequested { }

    public readonly struct TutorialCompleted { }

    public readonly struct UpgradePurchased
    {
        public readonly string Id;
        public readonly int Level;

        public UpgradePurchased(string id, int level) { Id = id; Level = level; }
    }
}
