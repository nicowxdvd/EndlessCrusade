using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class BaseStructure : MonoBehaviour, IDamageable
    {
        public BaseDefinition definition;
        public SpriteRenderer spriteRenderer;
        public float resistanceMultiplier = 1f;

        bool initialized;

        public int Current { get; private set; }
        public int Max { get; private set; }
        public int Stage { get; private set; }
        public Team Team => Team.Player;
        public bool IsAlive => Current > 0;

        void OnEnable()
        {
            TargetFinder.Register(this, transform);
        }

        void OnDisable()
        {
            TargetFinder.Unregister(this);
        }

        void Start()
        {
            if (!initialized && definition != null)
                Initialize(definition);
        }

        public void Initialize(BaseDefinition newDefinition)
        {
            definition = newDefinition;
            initialized = true;
            Max = Mathf.RoundToInt(newDefinition.maxResistance * resistanceMultiplier);
            Current = Max;
            ApplyStage();
            EventBus<BaseResistanceChanged>.Publish(new BaseResistanceChanged(Current, Max));
        }

        public void TakeDamage(int amount, GameObject source)
        {
            if (!IsAlive || amount <= 0 || source == null)
                return;
            if (source.TryGetComponent<IDamageable>(out var attacker) && attacker.Team == Team.Player)
                return;

            Current = Mathf.Max(0, Current - amount);
            ApplyStage();
            EventBus<BaseResistanceChanged>.Publish(new BaseResistanceChanged(Current, Max));
            if (Current == 0)
                EventBus<BaseDestroyed>.Publish(new BaseDestroyed());
        }

        public static int GetStage(int current, int max)
        {
            if (max <= 0)
                return 2;
            if (current * 100 > 66 * max)
                return 0;
            if (current * 100 > 33 * max)
                return 1;
            return 2;
        }

        void ApplyStage()
        {
            Stage = GetStage(Current, Max);
            var stages = definition != null ? definition.damageStages : null;
            if (spriteRenderer != null && stages != null && Stage < stages.Length)
                spriteRenderer.sprite = stages[Stage];
        }
    }
}
