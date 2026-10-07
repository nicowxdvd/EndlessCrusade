using EC.Core;
using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Unit Definition")]
    public class UnitDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public Team team;
        public int maxHealth = 100;
        public float moveSpeed = 2f;
        public int attackDamage = 10;
        public float attackRange = 1.2f;
        public float attackCooldown = 1f;
        public float hurtDuration = 0.25f;
        public CreatureTag tags;
        public GameObject prefab;
    }
}
