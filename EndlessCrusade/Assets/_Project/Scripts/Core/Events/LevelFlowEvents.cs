namespace EC.Core
{
    public enum HeroAction { Move, Sword, Whip }

    public readonly struct HeroActed
    {
        public readonly HeroAction Action;

        public HeroActed(HeroAction action) { Action = action; }
    }

    public readonly struct WavesStartRequested { }
}
