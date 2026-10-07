namespace EC.Core
{
    public readonly struct BaseResistanceChanged
    {
        public readonly int Current;
        public readonly int Max;

        public BaseResistanceChanged(int current, int max) { Current = current; Max = max; }
    }

    public readonly struct BaseDestroyed { }
}
