using EC.Core;
using UnityEngine;

namespace EC.Data
{
    public enum UpgradeCategory { Hero, Troop, Base }

    public enum UpgradeStat { HeroHealth, SwordDamage, WhipDamage, CooldownReduction, TroopPower, BaseResistance, BaseCrossbows }

    [CreateAssetMenu(menuName = "EC/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public UpgradeCategory category;
        public UpgradeStat stat;
        public string targetId;
        public int maxLevel = 10;
        public int baseCost = 100;
        public float costGrowth = 1.35f;
        public float valuePerLevel;
        public CurrencyType currency = CurrencyType.Gold;

        public int CostAtLevel(int level)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costGrowth, level));
        }
    }
}
