using System.Collections.Generic;
using EC.Core;
using EC.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EC.Gameplay
{
    [RequireComponent(typeof(EntityController), typeof(StatusEffectComponent))]
    public class HeroController : MonoBehaviour
    {
        public InputActionAsset controls;
        public SpriteRenderer sprite;

        EntityController controller;
        MovementComponent movement;
        StatusEffectComponent status;
        AttackComponent attack;
        HeroDefinition definition;
        InputAction moveLeft;
        InputAction moveRight;
        InputAction attackSword;
        InputAction attackWhip;

        public int Facing { get; private set; } = 1;

        void Awake()
        {
            controller = GetComponent<EntityController>();
            movement = GetComponent<MovementComponent>();
            status = GetComponent<StatusEffectComponent>();
            attack = GetComponent<AttackComponent>();
            definition = controller.definition as HeroDefinition;
        }

        void OnEnable()
        {
            if (controls == null)
                return;
            moveLeft = controls.FindAction("MoveLeft", true);
            moveRight = controls.FindAction("MoveRight", true);
            attackSword = controls.FindAction("AttackSword", true);
            attackWhip = controls.FindAction("AttackWhip", true);
            controls.Enable();
        }

        void OnDisable()
        {
            if (controls != null)
                controls.Disable();
        }

        void Start()
        {
            EventBus<HeroSpawned>.Publish(new HeroSpawned(gameObject));
        }

        void Update()
        {
            if (moveLeft == null)
                return;
            Apply(moveLeft.IsPressed(), moveRight.IsPressed(), attackSword.IsPressed(), attackWhip.IsPressed(), Time.time);
        }

        public void Apply(bool left, bool right, bool sword, bool whip, float now)
        {
            if (!controller.Health.IsAlive)
                return;

            if (status != null && status.IsDisoriented)
                (left, right) = (right, left);
            var direction = (right ? 1 : 0) - (left ? 1 : 0);
            if (direction != 0)
                Facing = direction;
            if (sprite != null)
                sprite.flipX = Facing < 0;

            movement.Direction = direction;
            controller.Request(direction != 0 ? EntityState.Move : EntityState.Idle);

            if (definition == null)
                return;
            if (sword)
                Perform(definition.sword, direction, now);
            if (whip)
                Perform(definition.whip, direction, now);
        }

        void Perform(MeleeAttackDefinition attackDefinition, int direction, float now)
        {
            if (!attack.IsReady(attackDefinition, now))
                return;
            var candidates = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);
            var target = SelectTarget(transform.position.x, Facing, controller.Health.Team, attackDefinition.range, candidates);
            if (target == null || !controller.Request(EntityState.Attack))
                return;

            attack.TryAttack(attackDefinition, target, now);
            controller.Request(direction != 0 ? EntityState.Move : EntityState.Idle);
        }

        public static IDamageable SelectTarget(float originX, int facing, Team team, float range, IEnumerable<HealthComponent> candidates)
        {
            IDamageable best = null;
            var bestDistance = float.MaxValue;
            foreach (var candidate in candidates)
            {
                if (!candidate.IsAlive || candidate.Team == team)
                    continue;
                var dx = (candidate.transform.position.x - originX) * facing;
                if (dx < 0f || dx > range || dx >= bestDistance)
                    continue;
                bestDistance = dx;
                best = candidate;
            }
            return best;
        }
    }
}
