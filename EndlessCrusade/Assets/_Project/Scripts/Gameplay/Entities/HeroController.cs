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
        public SpriteStateAnimator animator;

        EntityController controller;
        MovementComponent movement;
        StatusEffectComponent status;
        AttackComponent attack;
        HeroDefinition definition;
        InputAction moveLeft;
        InputAction moveRight;
        InputAction attackSword;
        InputAction attackWhip;
        InputAction useAbility1;

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
            useAbility1 = controls.FindAction("UseAbility1", true);
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
            if (useAbility1.WasPressedThisFrame())
                EventBus<AbilityRequested>.Publish(new AbilityRequested(0));
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
            if (direction != 0)
                EventBus<HeroActed>.Publish(new HeroActed(HeroAction.Move));
            if (sword)
                EventBus<HeroActed>.Publish(new HeroActed(HeroAction.Sword));
            if (whip)
                EventBus<HeroActed>.Publish(new HeroActed(HeroAction.Whip));

            if (definition == null)
                return;
            if (sword)
                Perform(definition.sword, "attack_sword", direction, now);
            if (whip)
                Perform(definition.whip, "attack_whip", direction, now);
        }

        void Perform(MeleeAttackDefinition attackDefinition, string clipId, int direction, float now)
        {
            if (!attack.IsReady(attackDefinition, now))
                return;
            var candidates = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);
            var target = SelectTarget(transform.position.x, Facing, controller.Health.Team, attackDefinition.range, candidates);
            if (target == null || !controller.Request(EntityState.Attack))
                return;

            attack.TryAttack(attackDefinition, target, now);
            if (animator != null)
                animator.Play(clipId);
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
