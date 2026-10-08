using System.Collections.Generic;
using UnityEngine;

namespace EC.Data
{
    public class HeroLoadout
    {
        public int bonusHealth;
        public float damageReduction;
        public float blockFraction;
        public EquipmentDefinition heavyWeapon;
        public EquipmentDefinition rangedWeapon;
        public EquipmentDefinition armor;
        public EquipmentDefinition shield;
        public readonly List<AbilityDefinition> miracles = new List<AbilityDefinition>();

        public bool Has(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Armor: return armor != null;
                case EquipmentSlot.Shield: return shield != null;
                case EquipmentSlot.HeavyWeapon: return heavyWeapon != null;
                default: return rangedWeapon != null;
            }
        }

        public static HeroLoadout Compute(EquipmentCatalog catalog, IEnumerable<string> equippedIds)
        {
            var loadout = new HeroLoadout();
            if (catalog == null || equippedIds == null)
                return loadout;

            foreach (var id in equippedIds)
            {
                var item = catalog.FindEquipment(id);
                if (item != null)
                {
                    loadout.Add(item);
                    continue;
                }
                var miracle = catalog.FindMiracle(id);
                if (miracle != null && miracle.ability != null)
                    loadout.miracles.Add(miracle.ability);
            }
            return loadout;
        }

        void Add(EquipmentDefinition item)
        {
            switch (item.slot)
            {
                case EquipmentSlot.Armor:
                    armor = item;
                    bonusHealth += item.bonusHealth;
                    damageReduction = Mathf.Clamp01(damageReduction + item.damageReduction);
                    break;
                case EquipmentSlot.Shield:
                    shield = item;
                    blockFraction = Mathf.Clamp01(item.blockFraction);
                    break;
                case EquipmentSlot.HeavyWeapon: heavyWeapon = item; break;
                case EquipmentSlot.RangedWeapon: rangedWeapon = item; break;
            }
        }
    }

    public static class HeroLoadoutHolder
    {
        public static HeroLoadout Current = new HeroLoadout();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = new HeroLoadout();
        }
    }
}
