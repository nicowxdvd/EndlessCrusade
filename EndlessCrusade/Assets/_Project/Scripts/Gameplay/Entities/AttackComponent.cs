using System;
using System.Collections.Generic;
using EC.Core;
using EC.Data;
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

        public event Action<int> Hit;

        EntityController controller;
        float lastAttackTime = float.NegativeInfinity;
    readonly Dictionary<string, float> lastAttackTimes = new Dictionary<string, float>();

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
            Hit?.Invoke(damage);
            return true;
        }

        public bool IsReady(MeleeAttackDefinition attack, float now)
        {
            if (attack == null)
                return false;
            return !lastAttackTimes.TryGetValue(attack.id, out var last) || now - last >= attack.cooldown;
        }

        public bool TryAttack(MeleeAttackDefinition attack, IDamageable target, float now)
        {
            if (attack == null)
                return false;
            if (controller != null && controller.State != EntityState.Attack)
                return false;
            if (target == null || !target.IsAlive || target.Team == controller.Health.Team)
                return false;
            if (!IsReady(attack, now))
                return false;

            lastAttackTimes[attack.id] = now;
            target.TakeDamage(attack.damage, gameObject);
            Hit?.Invoke(attack.damage);
            return true;
        }
    }
}
