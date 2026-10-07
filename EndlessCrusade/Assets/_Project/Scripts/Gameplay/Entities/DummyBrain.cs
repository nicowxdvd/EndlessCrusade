using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class DummyBrain : MonoBehaviour
    {
        EntityController controller;
        MovementComponent movement;
        AttackComponent attack;

        void Awake()
        {
            controller = GetComponent<EntityController>();
            movement = GetComponent<MovementComponent>();
            attack = GetComponent<AttackComponent>();
        }

        void Update()
        {
            if (!controller.Health.IsAlive)
                return;

            var target = FindNearestEnemy();
            attack.Target = target;
            if (target == null)
            {
                controller.Request(EntityState.Idle);
                return;
            }

            var dx = ((Component)target).transform.position.x - transform.position.x;
            if (Mathf.Abs(dx) > attack.range)
            {
                movement.Direction = dx;
                controller.Request(EntityState.Move);
            }
            else
            {
                controller.Request(EntityState.Attack);
            }
        }

        IDamageable FindNearestEnemy()
        {
            IDamageable best = null;
            var bestDistance = float.MaxValue;
            foreach (var candidate in FindObjectsByType<HealthComponent>(FindObjectsSortMode.None))
            {
                if (!candidate.IsAlive || candidate.Team == controller.Health.Team)
                    continue;
                var distance = Mathf.Abs(candidate.transform.position.x - transform.position.x);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
            return best;
        }
    }
}
