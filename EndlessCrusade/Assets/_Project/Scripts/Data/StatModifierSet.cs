using System;
using System.Collections.Generic;
using UnityEngine;

namespace EC.Data
{
    public class StatModifierSet
    {
        public const float MinCooldownMultiplier = 0.4f;

        public float heroHealth = 1f;
        public float swordDamage = 1f;
        public float whipDamage = 1f;
        public float cooldown = 1f;
        public float baseResistance = 1f;
        public bool baseCrossbows;

        readonly Dictionary<string, float> troopPower = new Dictionary<string, float>();

        public float TroopMultiplier(string troopId)
        {
            return troopId != null && troopPower.TryGetValue(troopId, out var value) ? value : 1f;
        }

        public static StatModifierSet Compute(IEnumerable<UpgradeDefinition> definitions, Func<string, int> levelOf)
        {
            var set = new StatModifierSet();
            if (definitions == null)
                return set;

            var cooldownReduction = 0f;
            foreach (var definition in definitions)
            {
                if (definition == null)
                    continue;
                var level = levelOf(definition.id);
                if (level <= 0)
                    continue;
                var amount = definition.valuePerLevel * level;
                switch (definition.stat)
                {
                    case UpgradeStat.HeroHealth: set.heroHealth += amount; break;
                    case UpgradeStat.SwordDamage: set.swordDamage += amount; break;
                    case UpgradeStat.WhipDamage: set.whipDamage += amount; break;
                    case UpgradeStat.CooldownReduction: cooldownReduction += amount; break;
                    case UpgradeStat.BaseResistance: set.baseResistance += amount; break;
                    case UpgradeStat.BaseCrossbows: set.baseCrossbows = true; break;
                    case UpgradeStat.TroopPower:
                        set.troopPower.TryGetValue(definition.targetId, out var current);
                        set.troopPower[definition.targetId] = (current == 0f ? 1f : current) + amount;
                        break;
                }
            }
            set.cooldown = Mathf.Max(MinCooldownMultiplier, 1f - cooldownReduction);
            return set;
        }
    }

    public static class LevelModifiers
    {
        public static StatModifierSet Current = new StatModifierSet();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = new StatModifierSet();
        }
    }
}
