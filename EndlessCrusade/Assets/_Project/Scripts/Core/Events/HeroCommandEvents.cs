namespace EC.Core
{
    public enum HeroCommandKind { Block, Heavy, Ranged }

    public readonly struct HeroCommand
    {
        public readonly HeroCommandKind Kind;
        public readonly bool Pressed;

        public HeroCommand(HeroCommandKind kind, bool pressed) { Kind = kind; Pressed = pressed; }
    }
}
