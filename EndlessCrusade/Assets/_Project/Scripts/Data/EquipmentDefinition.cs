using UnityEngine;

namespace EC.Data
{
    public enum EquipmentSlot { Armor, Shield, HeavyWeapon, RangedWeapon }

    [CreateAssetMenu(menuName = "EC/Equipment Definition")]
    public class EquipmentDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public EquipmentSlot slot;
        public int bonusHealth;
        public float damageReduction;
        public float blockFraction;
        public MeleeAttackDefinition attackOverride;
        public float stunSeconds;
        public GameObject projectilePrefab;
        public Sprite[] layerSprites;
    }
}
