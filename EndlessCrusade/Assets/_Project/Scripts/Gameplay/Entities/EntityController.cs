using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class EntityController : MonoBehaviour
    {
        public UnitDefinition definition;
        public float hurtDuration = 0.25f;

        StateMachine machine;
        HealthComponent health;

        public EntityState State => machine.Current;
        public HealthComponent Health => health;

        void Awake()
        {
            health = GetComponent<HealthComponent>();
            ApplyDefinition();

            machine = new StateMachine();
            machine.Register(EntityState.Idle, new SimpleState());
            machine.Register(EntityState.Move, new SimpleState());
            machine.Register(EntityState.Attack, new SimpleState());
            machine.Register(EntityState.Hurt, new HurtState(this));
            machine.Register(EntityState.Dead, new SimpleState());
            machine.Transition(EntityState.Idle);
        }

        void OnEnable()
        {
            if (health == null)
                return;
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        void OnDisable()
        {
            if (health == null)
                return;
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        void Update()
        {
            machine.Tick(Time.deltaTime);
        }

        public bool Request(EntityState requested)
        {
            if (machine.Current == EntityState.Hurt || machine.Current == EntityState.Dead)
                return false;
            if (requested == EntityState.Hurt || requested == EntityState.Dead)
                return false;
            return machine.Transition(requested);
        }

        void ApplyDefinition()
        {
            if (definition == null)
                return;

            hurtDuration = definition.hurtDuration;
            if (health != null)
                health.Initialize(definition.maxHealth, definition.team);

            var movement = GetComponent<MovementComponent>();
            if (movement != null)
                movement.moveSpeed = definition.moveSpeed;

            var attack = GetComponent<AttackComponent>();
            if (attack != null)
            {
                attack.damage = definition.attackDamage;
                attack.range = definition.attackRange;
                attack.cooldown = definition.attackCooldown;
            }
        }

        void OnDamaged()
        {
            machine.Transition(EntityState.Hurt);
        }

        void OnDied()
        {
            machine.Transition(EntityState.Dead);
        }

        void EndHurt()
        {
            machine.Transition(EntityState.Idle);
        }

        class SimpleState : IState
        {
            public void Enter() { }
            public void Tick(float deltaTime) { }
            public void Exit() { }
        }

        class HurtState : IState
        {
            readonly EntityController owner;
            float elapsed;

            public HurtState(EntityController owner)
            {
                this.owner = owner;
            }

            public void Enter()
            {
                elapsed = 0f;
            }

            public void Tick(float deltaTime)
            {
                elapsed += deltaTime;
                if (elapsed >= owner.hurtDuration)
                    owner.EndHurt();
            }

            public void Exit() { }
        }
    }
}
