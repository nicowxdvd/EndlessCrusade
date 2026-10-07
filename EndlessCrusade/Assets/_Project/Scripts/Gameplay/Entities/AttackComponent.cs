using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    [RequireComponent(typeof(EntityController))]
    public class AttackComponent : MonoBehaviour
    {
        public int damage = 10;
        public float range = 1.2f;
        public float cooldown = 1f;

        public IDamageable Target { get; set; }

        EntityController controller;
        float lastAttackTime = float.NegativeInfinity;

        void Awake()
        {
            controller = GetComponent<EntityController>();
        }

        void Update()
        {
            if (controller.State != EntityState.Attack)
                return;
            TryAttack(Target, Time.time);
        }

        public bool TryAttack(IDamageable target, float now)
        {
            if (controller != null && controller.State != EntityState.Attack)
                return false;
            if (target == null || !target.IsAlive || target.Team == controller.Health.Team)
                return false;
            if (now - lastAttackTime < cooldown)
                return false;

            lastAttackTime = now;
            target.TakeDamage(damage, gameObject);
            return true;
        }
    }
}
