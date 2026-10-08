using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class EnemyBrain : MonoBehaviour, IPoolable
    {
        const float DescendSpeed = 6f;
        const float AttackHeight = 1f;

        public Transform baseTarget;
        public PoolService pool;
        public float despawnDelay = 1f;

        EnemyDefinition definition;
        EntityController controller;
        MovementComponent movement;
        AttackComponent attack;
        float baseSpeed;
        IEnemyModule[] modules;
        float chargeTimeLeft;
        bool chargeUsed;
        float bobPhase;
        float deadTime;

        public EntityController Controller => controller;
        public HealthComponent Health { get; private set; }
        public MovementComponent Movement => movement;
        public AttackComponent Attack => attack;
        public float SpeedMultiplier { get; set; } = 1f;

        void Awake()
        {
            controller = GetComponent<EntityController>();
            Health = GetComponent<HealthComponent>();
            movement = GetComponent<MovementComponent>();
            attack = GetComponent<AttackComponent>();
            definition = controller.definition as EnemyDefinition;
            baseSpeed = controller.definition != null ? controller.definition.moveSpeed : movement.moveSpeed;
            modules = GetComponents<IEnemyModule>();
            InitializeModules();
        }

        void InitializeModules()
        {
            SpeedMultiplier = 1f;
            for (int i = 0; i < modules.Length; i++)
                modules[i].Initialize(this);
        }

        public void OnSpawn()
        {
            controller.ResetState();
            InitializeModules();
            movement.moveSpeed = baseSpeed;
            chargeTimeLeft = 0f;
            chargeUsed = false;
            deadTime = 0f;
            bobPhase = Random.value * Mathf.PI * 2f;
        }

        public void OnDespawn()
        {
            attack.Target = null;
            chargeTimeLeft = 0f;
        }

        void Update()
        {
            if (!controller.Health.IsAlive)
            {
                UpdateDead();
                return;
            }

            for (int i = 0; i < modules.Length; i++)
                modules[i].Tick(Time.deltaTime);

            var team = controller.Health.Team;
            var target = TargetFinder.FindNearest(transform.position, team, attack.range);
            attack.Target = target;

            if (target != null)
            {
                chargeTimeLeft = 0f;
                controller.Request(EntityState.Attack);
            }
            else
            {
                UpdateCharge(team);
                if (ReachedBase())
                {
                    controller.Request(EntityState.Idle);
                }
                else
                {
                    movement.Direction = DirectionToBase();
                    controller.Request(EntityState.Move);
                }
            }

            var speed = baseSpeed * SpeedMultiplier;
            if (chargeTimeLeft > 0f)
                speed *= definition.chargeSpeedMultiplier;
            movement.moveSpeed = speed;

            if (definition != null && definition.kind == EnemyKind.Flying)
                UpdateFlight(target != null);
        }

        float DirectionToBase()
        {
            if (baseTarget == null)
                return -1f;
            var dx = baseTarget.position.x - transform.position.x;
            return dx == 0f ? -1f : dx;
        }

        bool ReachedBase()
        {
            return baseTarget != null && Mathf.Abs(baseTarget.position.x - transform.position.x) <= attack.range;
        }

        void UpdateCharge(Team team)
        {
            if (definition == null || definition.kind != EnemyKind.Ground)
                return;

            if (chargeTimeLeft > 0f)
            {
                chargeTimeLeft -= Time.deltaTime;
                return;
            }

            var inChargeRange = TargetFinder.FindNearest(transform.position, team, definition.chargeRange) != null;
            if (!inChargeRange)
            {
                chargeUsed = false;
                return;
            }
            if (chargeUsed)
                return;

            chargeUsed = true;
            chargeTimeLeft = definition.chargeDuration;
        }

        void UpdateFlight(bool attacking)
        {
            var groundY = movement.lane != null ? movement.lane.groundY : 0f;
            var desiredY = groundY + AttackHeight;
            if (!attacking)
                desiredY = groundY + definition.flightHeight + Mathf.Sin(Time.time * definition.bobFrequency + bobPhase) * definition.bobAmplitude;
            var position = transform.position;
            position.y = Mathf.MoveTowards(position.y, desiredY, DescendSpeed * Time.deltaTime);
            transform.position = position;
        }

        void UpdateDead()
        {
            deadTime += Time.deltaTime;
            if (deadTime < despawnDelay)
                return;
            deadTime = float.NegativeInfinity;
            if (pool != null)
                pool.Release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
