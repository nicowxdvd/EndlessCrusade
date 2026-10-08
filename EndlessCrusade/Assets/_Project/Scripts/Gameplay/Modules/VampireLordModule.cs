using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class VampireLordModule : MonoBehaviour, IEnemyModule
    {
        public const float PhaseTwoThreshold = 0.66f;
        public const float PhaseThreeThreshold = 0.33f;

        public GameObject orbPrefab;
        public int orbDamage = 25;
        public float orbInterval = 4f;
        public float orbRange = 9f;
        public float orbSpeed = 9f;
        public float blinkInterval = 5f;
        public float blinkDistance = 6f;
        public float summonInterval = 20f;
        public EnemyDefinition vampireMinion;
        public int vampireCount = 4;
        public EnemyDefinition batMinion;
        public int batCount = 6;
        public float phaseThreeSpeed = 1.3f;

        EnemyBrain brain;
        int phase = 1;
        float orbElapsed;
        float blinkElapsed;
        float summonElapsed;

        public int Phase => phase;

        public static int PhaseFor(float healthRatio)
        {
            if (healthRatio <= PhaseThreeThreshold)
                return 3;
            return healthRatio <= PhaseTwoThreshold ? 2 : 1;
        }

        public void Initialize(EnemyBrain owner)
        {
            brain = owner;
            phase = 1;
            orbElapsed = 0f;
            blinkElapsed = 0f;
            summonElapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (brain == null || !brain.Health.IsAlive)
                return;

            UpdatePhase();
            orbElapsed += deltaTime;
            if (orbElapsed >= orbInterval)
            {
                orbElapsed = 0f;
                FireOrbs(phase >= 3 ? 2 : 1);
            }

            if (phase < 2)
                return;

            blinkElapsed += deltaTime;
            if (blinkElapsed >= blinkInterval && brain.Controller.State == EntityState.Move)
            {
                blinkElapsed = 0f;
                brain.Movement.Step(brain.Movement.Direction, 1f, blinkDistance);
            }

            summonElapsed += deltaTime;
            if (summonElapsed >= summonInterval)
            {
                summonElapsed = 0f;
                Summon();
            }
        }

        void UpdatePhase()
        {
            var health = brain.Health;
            var next = PhaseFor(health.maxHealth > 0 ? (float)health.Current / health.maxHealth : 0f);
            if (next <= phase)
                return;
            phase = next;
            if (phase >= 3)
                brain.SpeedMultiplier = phaseThreeSpeed;
            EventBus<BossPhaseChanged>.Publish(new BossPhaseChanged(gameObject, phase));
        }

        void FireOrbs(int count)
        {
            if (orbPrefab == null || brain.pool == null)
                return;
            var target = TargetFinder.FindNearest(transform.position, brain.Health.Team, orbRange);
            if (target == null || !(target is Component component))
                return;

            for (int i = 0; i < count; i++)
            {
                var origin = transform.position + new Vector3(0f, 1.2f + i * 0.5f, 0f);
                var instance = brain.pool.Get(orbPrefab, origin, Quaternion.identity);
                instance.GetComponent<Bolt>().Launch(brain.pool, target, component.transform, orbDamage, gameObject, orbSpeed);
            }
        }

        void Summon()
        {
            if (brain.pool == null)
                return;
            if (vampireMinion != null && vampireMinion.prefab != null)
                for (int i = 0; i < vampireCount; i++)
                    SummonModule.SpawnAround(brain, transform.position, vampireMinion, i, vampireCount, 1.5f);
            if (batMinion != null && batMinion.prefab != null)
                for (int i = 0; i < batCount; i++)
                    SummonModule.SpawnAround(brain, transform.position, batMinion, i, batCount, 1f);
        }
    }
}
