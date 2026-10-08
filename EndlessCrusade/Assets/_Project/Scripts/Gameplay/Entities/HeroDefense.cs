using UnityEngine;

namespace EC.Gameplay
{
    public class HeroDefense : IDamageModifier
    {
        readonly float damageReduction;
        readonly float blockFraction;

        public bool Blocking { get; set; }

        public HeroDefense(float damageReduction, float blockFraction)
        {
            this.damageReduction = damageReduction;
            this.blockFraction = blockFraction;
        }

        public int Modify(int amount, GameObject source)
        {
            if (amount <= 0)
                return amount;
            var scaled = amount * (1f - damageReduction);
            if (Blocking)
                scaled *= 1f - blockFraction;
            return Mathf.Max(1, Mathf.RoundToInt(scaled));
        }
    }
}
