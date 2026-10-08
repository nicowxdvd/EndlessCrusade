using System.Collections.Generic;
using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class HealAuraComponent : MonoBehaviour
    {
        public int amount = 10;
        public float interval = 2f;
        public float radius = 4f;

        static readonly List<IDamageable> allies = new List<IDamageable>(32);

        HealthComponent health;
        float elapsed;

        HealthComponent Health
        {
            get
            {
                if (health == null)
                    health = GetComponent<HealthComponent>();
                return health;
            }
        }

        void OnEnable()
        {
            elapsed = 0f;
        }

        void Update()
        {
            Tick(Time.deltaTime);
        }

        public int Tick(float deltaTime)
        {
            if (Health == null || !Health.IsAlive)
                return 0;
            elapsed += deltaTime;
            if (elapsed < interval)
                return 0;
            elapsed -= interval;
            return Pulse();
        }

        public int Pulse()
        {
            var enemyTeam = Health.Team == Team.Player ? Team.Enemy : Team.Player;
            TargetFinder.Collect(transform.position, radius, enemyTeam, allies);
            var healed = 0;
            for (int i = 0; i < allies.Count; i++)
            {
                if (!(allies[i] is HealthComponent ally) || ally.Current >= ally.maxHealth)
                    continue;
                ally.Heal(amount);
                healed++;
            }
            allies.Clear();
            return healed;
        }
    }
}
