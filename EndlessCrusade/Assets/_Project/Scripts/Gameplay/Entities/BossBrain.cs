using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public enum BossState { Advance, Windup, Charge, Stunned, Roar }

    public class BossBrain : MonoBehaviour, IPoolable
    {
        const float RoarDuration = 1f;
        const float ChargeCooldown = 3f;
        const float RoarShakeIntensity = 0.4f;
        const float ImpactShakeIntensity = 0.6f;
        const float ShakeDuration = 0.5f;
        const float TelegraphDepth = 4f;

        public Transform baseTarget;
        public PoolService pool;
        public Material telegraphMaterial;
        public float despawnDelay = 1f;

        BossDefinition definition;
        EntityController controller;
        MovementComponent movement;
        AttackComponent attack;
        HealthComponent health;
        Transform telegraph;
        float baseSpeed;
        float stateTime;
        float roarTimer;
        float chargeCooldown;
        float chargeDirection;
        float deadTime;

        public BossState State { get; private set; }
        public bool IsEnraged { get; private set; }
        public bool IsTelegraphVisible => telegraph != null && telegraph.gameObject.activeSelf;

        void Awake()
        {
            controller = GetComponent<EntityController>();
            movement = GetComponent<MovementComponent>();
            attack = GetComponent<AttackComponent>();
            health = GetComponent<HealthComponent>();
            definition = controller.definition as BossDefinition;
            baseSpeed = controller.definition != null ? controller.definition.moveSpeed : movement.moveSpeed;
        }

        void OnDestroy()
        {
            if (telegraph != null)
                Destroy(telegraph.gameObject);
        }

        public void OnSpawn()
        {
            controller.ResetState();
            movement.moveSpeed = baseSpeed;
            IsEnraged = false;
            deadTime = 0f;
            chargeCooldown = 0f;
            roarTimer = definition != null ? definition.roarInterval : float.MaxValue;
            Enter(BossState.Advance);
        }

        public void OnDespawn()
        {
            attack.Target = null;
            HideTelegraph();
        }

        void Update()
        {
            if (definition == null)
                return;
            if (!health.IsAlive)
            {
                HideTelegraph();
                UpdateDead();
                return;
            }

            UpdateEnrage();
            stateTime += Time.deltaTime;
            chargeCooldown -= Time.deltaTime;
            roarTimer -= Time.deltaTime;

            switch (State)
            {
                case BossState.Advance: UpdateAdvance(); break;
                case BossState.Windup: UpdateWindup(); break;
                case BossState.Charge: UpdateCharge(); break;
                case BossState.Stunned: UpdateStunned(); break;
                case BossState.Roar: UpdateRoar(); break;
            }
        }

        void UpdateEnrage()
        {
            if (IsEnraged || health.maxHealth <= 0)
                return;
            if ((float)health.Current / health.maxHealth > definition.enrageThreshold)
                return;
            IsEnraged = true;
            movement.moveSpeed = CurrentSpeed;
        }

        float CurrentSpeed => baseSpeed * (IsEnraged ? definition.enrageSpeedMultiplier : 1f);

        void Enter(BossState next)
        {
            State = next;
            stateTime = 0f;
        }

        void UpdateAdvance()
        {
            movement.moveSpeed = CurrentSpeed;
            var team = health.Team;

            if (roarTimer <= 0f)
            {
                StartRoar();
                return;
            }

            var melee = TargetFinder.FindNearest(transform.position, team, attack.range);
            attack.Target = melee;
            if (melee != null)
            {
                controller.Request(EntityState.Attack);
                return;
            }

            if (chargeCooldown <= 0f)
            {
                var far = TargetFinder.FindNearest(transform.position, team, definition.chargeRange);
                if (far != null)
                {
                    StartWindup(far);
                    return;
                }
            }

            if (ReachedBase())
            {
                controller.Request(EntityState.Idle);
                return;
            }
            movement.Direction = DirectionToBase();
            controller.Request(EntityState.Move);
        }

        void StartWindup(IDamageable target)
        {
            var targetX = target is Component component ? component.transform.position.x : transform.position.x + DirectionToBase();
            var delta = targetX - transform.position.x;
            chargeDirection = delta == 0f ? DirectionToBase() : Mathf.Sign(delta);
            attack.Target = null;
            controller.Request(EntityState.Idle);
            ShowTelegraph();
            Enter(BossState.Windup);
        }

        void UpdateWindup()
        {
            if (stateTime < definition.chargeWindup)
                return;
            HideTelegraph();
            Enter(BossState.Charge);
        }

        void UpdateCharge()
        {
            var speed = CurrentSpeed * definition.chargeSpeedMultiplier;
            var duration = definition.chargeRange / Mathf.Max(0.01f, speed);
            movement.Step(chargeDirection, Time.deltaTime, speed);

            var victim = TargetFinder.FindNearest(transform.position, health.Team, attack.range);
            if (victim != null)
            {
                victim.TakeDamage(definition.chargeDamage, gameObject);
                EventBus<CameraShakeRequested>.Publish(new CameraShakeRequested(ImpactShakeIntensity, ShakeDuration));
                EndCharge();
            }
            else if (stateTime >= duration)
            {
                EndCharge();
            }
        }

        void EndCharge()
        {
            chargeCooldown = ChargeCooldown;
            controller.Request(EntityState.Idle);
            Enter(BossState.Stunned);
        }

        void UpdateStunned()
        {
            attack.Target = null;
            controller.Request(EntityState.Idle);
            if (stateTime >= definition.stunAfterCharge)
                Enter(BossState.Advance);
        }

        void StartRoar()
        {
            roarTimer = definition.roarInterval;
            attack.Target = null;
            controller.Request(EntityState.Idle);
            Enter(BossState.Roar);

            foreach (var status in FindObjectsByType<StatusEffectComponent>(FindObjectsSortMode.None))
            {
                if (Mathf.Abs(status.transform.position.x - transform.position.x) <= definition.roarRadius)
                    status.Apply(StatusEffectType.Disoriented, definition.disorientDuration);
            }
            EventBus<CameraShakeRequested>.Publish(new CameraShakeRequested(RoarShakeIntensity, ShakeDuration));
        }

        void UpdateRoar()
        {
            if (stateTime >= RoarDuration)
                Enter(BossState.Advance);
        }

        void ShowTelegraph()
        {
            if (telegraph == null)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "ChargeTelegraph";
                Destroy(quad.GetComponent<Collider>());
                if (telegraphMaterial != null)
                    quad.GetComponent<MeshRenderer>().sharedMaterial = telegraphMaterial;
                telegraph = quad.transform;
            }
            var groundY = movement.lane != null ? movement.lane.groundY : 0f;
            var length = definition.chargeRange;
            telegraph.rotation = Quaternion.Euler(90f, 0f, 0f);
            telegraph.localScale = new Vector3(length, TelegraphDepth, 1f);
            telegraph.position = new Vector3(transform.position.x + chargeDirection * length * 0.5f, groundY + 0.03f, transform.position.z);
            telegraph.gameObject.SetActive(true);
        }

        void HideTelegraph()
        {
            if (telegraph != null)
                telegraph.gameObject.SetActive(false);
        }

        float DirectionToBase()
        {
            if (baseTarget == null)
                return -1f;
            var dx = baseTarget.position.x - transform.position.x;
            return dx == 0f ? -1f : Mathf.Sign(dx);
        }

        bool ReachedBase()
        {
            return baseTarget != null && Mathf.Abs(baseTarget.position.x - transform.position.x) <= attack.range;
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
