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
        MeleeAttackDefinition swordAttack;
        MeleeAttackDefinition whipAttack;
        MeleeAttackDefinition heavyAttack;
        HeroDefense defense;
        PoolService pool;
        float nextRangedTime;

        const float RangedBoltSpeed = 16f;
        const int ImpactFrame = 2;

        class PendingHit
        {
            public MeleeAttackDefinition attack;
            public int facing;
            public bool whip;
            public float due;
            public System.Action<IDamageable> onHit;
        }

        readonly List<PendingHit> pendingHits = new List<PendingHit>();
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
            ApplyModifiers(new StatModifierSet());
        }

        void OnEnable()
        {
            EventBus<HeroCommand>.Subscribe(OnCommand);
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
            EventBus<HeroCommand>.Unsubscribe(OnCommand);
            if (controls != null)
                controls.Disable();
        }

        void Start()
        {
            var modifiers = LevelModifiers.Current;
            ApplyModifiers(modifiers);
            controller.healthMultiplier = modifiers.heroHealth;
            var loadout = HeroLoadoutHolder.Current;
            controller.bonusHealth = loadout.bonusHealth;
            controller.ResetState();
            defense = new HeroDefense(loadout.damageReduction, loadout.blockFraction);
            controller.Health.AddModifier(defense);
            if (loadout.heavyWeapon != null)
                heavyAttack = Scale(loadout.heavyWeapon.attackOverride, modifiers.swordDamage, modifiers.cooldown);
            EventBus<HeroSpawned>.Publish(new HeroSpawned(gameObject));
        }

        void Update()
        {
            ResolvePending(Time.time);
            if (moveLeft == null)
                return;
            if (useAbility1.WasPressedThisFrame())
                EventBus<AbilityRequested>.Publish(new AbilityRequested(0));
            Apply(moveLeft.IsPressed(), moveRight.IsPressed(), attackSword.IsPressed(), attackWhip.IsPressed(), Time.time);
        }

        void ApplyModifiers(StatModifierSet modifiers)
        {
            if (definition == null)
                return;
            swordAttack = Scale(definition.sword, modifiers.swordDamage, modifiers.cooldown);
            whipAttack = Scale(definition.whip, modifiers.whipDamage, modifiers.cooldown);
        }

        static MeleeAttackDefinition Scale(MeleeAttackDefinition source, float damage, float cooldown)
        {
            if (source == null)
                return null;
            return new MeleeAttackDefinition { id = source.id, damage = Mathf.RoundToInt(source.damage * damage), range = source.range, cooldown = source.cooldown * cooldown };
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
                Perform(swordAttack, "attack_sword", direction, now);
            if (whip)
                Perform(whipAttack, "attack_whip", direction, now);
        }

        void Perform(MeleeAttackDefinition attackDefinition, string clipId, int direction, float now, System.Action<IDamageable> onHit = null)
        {
            if (attackDefinition == null || !attack.IsReady(attackDefinition, now) || !controller.Request(EntityState.Attack))
                return;

            attack.MarkUsed(attackDefinition, now);
            if (animator != null)
                animator.Play(clipId);
            controller.Request(direction != 0 ? EntityState.Move : EntityState.Idle);

            var delay = animator != null ? animator.SecondsToFrame(clipId, ImpactFrame) : 0f;
            var pendingHit = new PendingHit { attack = attackDefinition, facing = Facing, whip = clipId == "attack_whip", due = now + delay, onHit = onHit };
            if (delay <= 0f)
                Resolve(pendingHit);
            else
                pendingHits.Add(pendingHit);
        }

        void ResolvePending(float now)
        {
            for (var i = pendingHits.Count - 1; i >= 0; i--)
            {
                var pendingHit = pendingHits[i];
                if (!controller.Health.IsAlive)
                {
                    pendingHits.RemoveAt(i);
                    continue;
                }
                if (now < pendingHit.due)
                    continue;
                pendingHits.RemoveAt(i);
                Resolve(pendingHit);
            }
        }

        void Resolve(PendingHit pendingHit)
        {
            var candidates = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);
            var target = SelectTarget(transform.position.x, pendingHit.facing, controller.Health.Team, pendingHit.attack.range, candidates);
            if (target != null && attack.Strike(pendingHit.attack, target))
            {
                MeleeSlashEffect.Spawn(transform.position, ((Component)target).transform.position, pendingHit.whip);
                pendingHit.onHit?.Invoke(target);
                return;
            }

            var reach = transform.position + Vector3.right * pendingHit.facing * pendingHit.attack.range;
            MeleeSlashEffect.Spawn(transform.position, reach, pendingHit.whip);
        }

        void OnCommand(HeroCommand command)
        {
            if (controller == null || !controller.Health.IsAlive)
                return;
            switch (command.Kind)
            {
                case HeroCommandKind.Block:
                    if (defense != null)
                        defense.Blocking = command.Pressed;
                    break;
                case HeroCommandKind.Heavy:
                    if (command.Pressed)
                        PerformHeavy(Time.time);
                    break;
                case HeroCommandKind.Ranged:
                    if (command.Pressed)
                        FireRanged(Time.time);
                    break;
            }
        }

        void PerformHeavy(float now)
        {
            var weapon = HeroLoadoutHolder.Current.heavyWeapon;
            Perform(heavyAttack, "attack_sword", 0, now, target =>
            {
                if (weapon == null || weapon.stunSeconds <= 0f || !(target is Component component))
                    return;
                if (!component.TryGetComponent<StatusEffectComponent>(out var status))
                    status = component.gameObject.AddComponent<StatusEffectComponent>();
                status.Apply(StatusEffectType.Stunned, weapon.stunSeconds);
            });
        }

        void FireRanged(float now)
        {
            var weapon = HeroLoadoutHolder.Current.rangedWeapon;
            if (weapon == null || weapon.attackOverride == null || weapon.projectilePrefab == null || now < nextRangedTime)
                return;
            var candidates = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);
            var target = SelectTarget(transform.position.x, Facing, controller.Health.Team, weapon.attackOverride.range, candidates);
            if (target == null || !(target is Component component))
                return;
            if (pool == null)
                pool = FindFirstObjectByType<PoolService>();
            if (pool == null)
                return;

            nextRangedTime = now + weapon.attackOverride.cooldown * LevelModifiers.Current.cooldown;
            var instance = pool.Get(weapon.projectilePrefab, transform.position + Vector3.up * 0.6f, Quaternion.identity);
            var damage = Mathf.RoundToInt(weapon.attackOverride.damage * LevelModifiers.Current.swordDamage);
            instance.GetComponent<Bolt>().Launch(pool, target, component.transform, damage, gameObject, RangedBoltSpeed);
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
