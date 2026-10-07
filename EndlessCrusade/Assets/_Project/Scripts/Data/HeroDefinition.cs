using UnityEngine;

namespace EC.Data
{
    [System.Serializable]
    public class MeleeAttackDefinition
    {
        public string id;
        public int damage;
        public float range;
        public float cooldown;
    }

    [CreateAssetMenu(menuName = "EC/Hero Definition")]
    public class HeroDefinition : UnitDefinition
    {
        public MeleeAttackDefinition sword;
        public MeleeAttackDefinition whip;
    }
}
