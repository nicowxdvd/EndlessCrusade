using System;
using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        public int maxHealth = 100;
        public Team team;
        public CreatureTag tags;

        public int Current { get; private set; }
        public Team Team => team;
        public bool IsAlive => Current > 0;

        public event Action Damaged;
        public event Action Died;

        void Awake()
        {
            if (Current == 0 && maxHealth > 0)
                Current = maxHealth;
        }

        void OnEnable()
        {
            TargetFinder.Register(this, transform);
        }

        void OnDisable()
        {
            TargetFinder.Unregister(this);
        }

        public void Initialize(int max, Team newTeam)
        {
            maxHealth = max;
            team = newTeam;
            Current = max;
        }

        public void TakeDamage(int amount, GameObject source)
        {
            if (!IsAlive || amount <= 0)
                return;

            Current = Mathf.Max(0, Current - amount);
            EventBus<HealthChanged>.Publish(new HealthChanged(gameObject, Current, maxHealth));

            if (Current == 0)
            {
                Died?.Invoke();
                EventBus<EntityDied>.Publish(new EntityDied(gameObject));
            }
            else
            {
                Damaged?.Invoke();
            }
        }
    }
}
