using System.Collections.Generic;
using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public enum StrikeEvent { None, TelegraphStarted, Struck }

    public class AreaStrikeModule : MonoBehaviour, IEnemyModule
    {
        public float triggerRange = 3.5f;
        public float radius = 3f;
        public int damage = 45;
        public float telegraphSeconds = 1.2f;
        public float cooldown = 6f;
        public float vulnerableSeconds = 4f;
        public float shakeIntensity = 0.3f;

        static readonly List<IDamageable> hits = new List<IDamageable>(16);

        EnemyBrain brain;
        float cooldownLeft;
        float telegraphLeft;
        float vulnerableLeft;

        public bool Telegraphing => telegraphLeft > 0f;
        public bool Vulnerable => vulnerableLeft > 0f;

        public void Initialize(EnemyBrain owner)
        {
            brain = owner;
            cooldownLeft = cooldown;
            telegraphLeft = 0f;
            vulnerableLeft = 0f;
        }

        public StrikeEvent Advance(float deltaTime, bool targetInRange)
        {
            if (vulnerableLeft > 0f)
                vulnerableLeft = Mathf.Max(0f, vulnerableLeft - deltaTime);

            if (telegraphLeft > 0f)
            {
                telegraphLeft -= deltaTime;
                if (telegraphLeft > 0f)
                    return StrikeEvent.None;
                telegraphLeft = 0f;
                cooldownLeft = cooldown;
                vulnerableLeft = vulnerableSeconds;
                return StrikeEvent.Struck;
            }

            cooldownLeft = Mathf.Max(0f, cooldownLeft - deltaTime);
            if (cooldownLeft > 0f || !targetInRange)
                return StrikeEvent.None;

            telegraphLeft = telegraphSeconds;
            return StrikeEvent.TelegraphStarted;
        }

        public void Tick(float deltaTime)
        {
            if (brain == null || !brain.Health.IsAlive)
                return;

            var inRange = TargetFinder.FindNearest(transform.position, brain.Health.Team, triggerRange) != null;
            if (Advance(deltaTime, inRange) == StrikeEvent.Struck)
                Strike();
        }

        void Strike()
        {
            TargetFinder.Collect(transform.position, radius, brain.Health.Team, hits);
            for (int i = 0; i < hits.Count; i++)
                hits[i].TakeDamage(damage, gameObject);
            hits.Clear();
            EventBus<CameraShakeRequested>.Publish(new CameraShakeRequested(shakeIntensity, 0.4f));
        }
    }
}
