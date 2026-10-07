using UnityEngine;

namespace EC.Core
{
    public readonly struct HealthChanged
    {
        public readonly GameObject Source;
        public readonly int Current;
        public readonly int Max;

        public HealthChanged(GameObject source, int current, int max)
        {
            Source = source;
            Current = current;
            Max = max;
        }
    }

    public readonly struct EntityDied
    {
        public readonly GameObject Source;

        public EntityDied(GameObject source)
        {
            Source = source;
        }
    }

    public readonly struct HeroSpawned
    {
        public readonly GameObject Hero;

        public HeroSpawned(GameObject hero)
        {
            Hero = hero;
        }
    }
}
