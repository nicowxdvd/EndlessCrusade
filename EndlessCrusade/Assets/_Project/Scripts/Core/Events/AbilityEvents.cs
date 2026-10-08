namespace EC.Core
{
    public readonly struct AbilityRequested
    {
        public readonly int Slot;
        public AbilityRequested(int slot) { Slot = slot; }
    }

    public readonly struct AbilityCooldownChanged
    {
        public readonly int Slot;
        public readonly float Normalized;
        public AbilityCooldownChanged(int slot, float normalized) { Slot = slot; Normalized = normalized; }
    }

    public readonly struct AbilityCooldownResetRequested { }
}
