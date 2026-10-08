using System;
using System.Collections.Generic;
using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        public int maxHealth = 100;
        public Team team;
        public CreatureTag tags;

        readonly List<IDamageModifier> modifiers = new List<IDamageModifier>();

        public int Current { get; private set; }
        public Team Team => team;
        public bool IsAlive => Current > 0;

        public event Action Damaged;
        public event Action Died;

        void Awake()
        {
            if (Current == 0 && maxHealth > 0)
                Current = maxHealth;
            if (!TryGetComponent<HitFlash>(out _))
                gameObject.AddComponent<HitFlash>();
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

        public void AddModifier(IDamageModifier modifier)
        {
            if (!modifiers.Contains(modifier))
                modifiers.Add(modifier);
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0 || Current >= maxHealth)
                return;

            Current = Mathf.Min(maxHealth, Current + amount);
            EventBus<HealthChanged>.Publish(new HealthChanged(gameObject, Current, maxHealth));
        }

        public void Revive(int health)
        {
            Current = Mathf.Clamp(health, 1, maxHealth);
            EventBus<HealthChanged>.Publish(new HealthChanged(gameObject, Current, maxHealth));
        }

        public void TakeDamage(int amount, GameObject source)
        {
            if (!IsAlive)
                return;

            for (int i = 0; i < modifiers.Count; i++)
                amount = modifiers[i].Modify(amount, source);

            if (amount <= 0)
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
                EventBus<EntityHurt>.Publish(new EntityHurt(gameObject));
            }
        }
    }
}
