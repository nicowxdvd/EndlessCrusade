using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class AbilityComponent : MonoBehaviour
    {
        public const int MaxSlots = 3;

        public AbilityDefinition[] abilities = new AbilityDefinition[MaxSlots];
        public PoolService pool;

        static readonly System.Collections.Generic.List<IDamageable> judged = new System.Collections.Generic.List<IDamageable>(32);

        CooldownTimer[] timers;
        HealthComponent health;
        HeroController hero;

        public int SlotCount => timers == null ? 0 : timers.Length;

        void Awake()
        {
            health = GetComponent<HealthComponent>();
            hero = GetComponent<HeroController>();
            BuildTimers();
        }

        void OnEnable()
        {
            EventBus<AbilityRequested>.Subscribe(OnAbilityRequested);
            EventBus<AbilityCooldownResetRequested>.Subscribe(OnCooldownResetRequested);
        }

        void OnDisable()
        {
            EventBus<AbilityRequested>.Unsubscribe(OnAbilityRequested);
            EventBus<AbilityCooldownResetRequested>.Unsubscribe(OnCooldownResetRequested);
        }

        void Start()
        {
            if (pool == null)
                pool = FindFirstObjectByType<PoolService>();
            ApplyLoadout();
            for (int i = 0; i < timers.Length; i++)
                if (timers[i] != null)
                    EventBus<AbilityCooldownChanged>.Publish(new AbilityCooldownChanged(i, timers[i].Normalized));
        }

        void Update()
        {
            var dt = Time.deltaTime;
            for (int i = 0; i < timers.Length; i++)
            {
                var timer = timers[i];
                if (timer == null || timer.IsReady)
                    continue;
                timer.Tick(dt);
                EventBus<AbilityCooldownChanged>.Publish(new AbilityCooldownChanged(i, timer.Normalized));
            }
        }

        void OnCooldownResetRequested(AbilityCooldownResetRequested evt)
        {
            BuildTimers();
            for (int i = 0; i < timers.Length; i++)
                if (timers[i] != null)
                    EventBus<AbilityCooldownChanged>.Publish(new AbilityCooldownChanged(i, timers[i].Normalized));
        }

        public void BuildTimers()
        {
            var count = Mathf.Min(abilities == null ? 0 : abilities.Length, MaxSlots);
            timers = new CooldownTimer[count];
            for (int i = 0; i < count; i++)
                timers[i] = abilities[i] == null ? null : new CooldownTimer(abilities[i].cooldown);
        }

        public float GetNormalized(int slot)
        {
            return IsValidSlot(slot) ? timers[slot].Normalized : 0f;
        }

        public bool TryUse(int slot)
        {
            if (!IsValidSlot(slot))
                return false;
            if (health != null && !health.IsAlive)
                return false;
            var definition = abilities[slot];
            if (!timers[slot].TryStart())
                return false;

            EventBus<AbilityCooldownChanged>.Publish(new AbilityCooldownChanged(slot, timers[slot].Normalized));
            if (definition.kind == AbilityKind.ThrownArea)
                Throw(definition);
            else if (definition.kind == AbilityKind.Heal)
                Bless(definition);
            else if (definition.kind == AbilityKind.ScreenDamage)
                Judge(definition);
            return true;
        }

        void ApplyLoadout()
        {
            var miracles = HeroLoadoutHolder.Current.miracles;
            if (miracles.Count == 0)
                return;
            if (abilities == null || abilities.Length < MaxSlots)
                System.Array.Resize(ref abilities, MaxSlots);
            for (int i = 0; i < miracles.Count && i + 1 < MaxSlots; i++)
                abilities[i + 1] = miracles[i];
            BuildTimers();
        }

        void Bless(AbilityDefinition definition)
        {
            if (health != null)
                health.Heal(Mathf.RoundToInt(health.maxHealth * definition.healFraction));
            if (TryGetComponent<StatusEffectComponent>(out var status))
                status.Clear();
        }

        void Judge(AbilityDefinition definition)
        {
            var team = health != null ? health.Team : Team.Player;
            TargetFinder.Collect(transform.position, definition.radius, team, judged);
            for (int i = 0; i < judged.Count; i++)
            {
                var tags = judged[i] is HealthComponent target ? target.tags : CreatureTag.None;
                judged[i].TakeDamage(HolyWaterProjectile.ComputeDamage(definition, tags), gameObject);
            }
            judged.Clear();
        }

        void OnAbilityRequested(AbilityRequested evt)
        {
            TryUse(evt.Slot);
        }

        bool IsValidSlot(int slot)
        {
            return timers != null && slot >= 0 && slot < timers.Length && timers[slot] != null;
        }

        void Throw(AbilityDefinition definition)
        {
            if (pool == null || definition.projectilePrefab == null)
                return;
            var facing = hero != null ? hero.Facing : 1;
            var origin = transform.position;
            var target = new Vector3(origin.x + facing * definition.throwDistance, origin.y, origin.z);
            var instance = pool.Get(definition.projectilePrefab, origin, Quaternion.identity);
            var projectile = instance.GetComponent<HolyWaterProjectile>();
            projectile.Launch(definition, pool, origin, target, health != null ? health.Team : Team.Player, gameObject);
        }
    }
}
