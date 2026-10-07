using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class TroopBrain : MonoBehaviour, IPoolable
    {
        public Transform baseTarget;
        public PoolService pool;
        public float despawnDelay = 1f;
        public float aggroRange = 10f;
        public float rallyOffset = 3f;
        public float rallyTolerance = 0.4f;

        EntityController controller;
        MovementComponent movement;
        AttackComponent attack;
        float deadTime;

        void Awake()
        {
            controller = GetComponent<EntityController>();
            movement = GetComponent<MovementComponent>();
            attack = GetComponent<AttackComponent>();
        }

        public void OnSpawn()
        {
            controller.ResetState();
            deadTime = 0f;
        }

        public void OnDespawn()
        {
            attack.Target = null;
        }

        void Update()
        {
            if (!controller.Health.IsAlive)
            {
                UpdateDead();
                return;
            }

            var position = transform.position;
            var target = TargetFinder.FindNearest(position, controller.Health.Team, aggroRange);
            var targetTransform = (target as Component)?.transform;

            if (targetTransform != null)
            {
                var dx = targetTransform.position.x - position.x;
                if (Mathf.Abs(dx) <= attack.range)
                {
                    attack.Target = target;
                    controller.Request(EntityState.Attack);
                }
                else
                {
                    attack.Target = null;
                    movement.Direction = dx;
                    controller.Request(EntityState.Move);
                }
                return;
            }

            attack.Target = null;
            ReturnToRally(position.x);
        }

        void ReturnToRally(float x)
        {
            if (baseTarget == null)
            {
                controller.Request(EntityState.Idle);
                return;
            }

            var dx = baseTarget.position.x + rallyOffset - x;
            if (Mathf.Abs(dx) <= rallyTolerance)
            {
                controller.Request(EntityState.Idle);
                return;
            }

            movement.Direction = dx;
            controller.Request(EntityState.Move);
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
