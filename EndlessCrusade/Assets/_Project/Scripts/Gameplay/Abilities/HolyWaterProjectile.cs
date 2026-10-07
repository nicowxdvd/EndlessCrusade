using System.Collections.Generic;
using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class HolyWaterProjectile : MonoBehaviour, IPoolable
    {
        public float flightTime = 0.6f;
        public float arcHeight = 2.5f;

        static readonly List<IDamageable> hits = new List<IDamageable>(32);

        AbilityDefinition definition;
        PoolService pool;
        Vector3 start;
        Vector3 end;
        Team ownerTeam;
        GameObject owner;
        float elapsed;
        bool flying;

        public void Launch(AbilityDefinition ability, PoolService poolService, Vector3 from, Vector3 to, Team team, GameObject source)
        {
            definition = ability;
            pool = poolService;
            start = from;
            end = to;
            ownerTeam = team;
            owner = source;
            elapsed = 0f;
            flying = true;
            transform.position = from;
        }

        public void OnSpawn() { }

        public void OnDespawn()
        {
            flying = false;
            definition = null;
            owner = null;
        }

        void Update()
        {
            if (!flying)
                return;
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / flightTime);
            transform.position = Evaluate(start, end, arcHeight, t);
            if (t >= 1f)
                Explode();
        }

        public static Vector3 Evaluate(Vector3 from, Vector3 to, float height, float t)
        {
            var position = Vector3.Lerp(from, to, t);
            position.y += height * 4f * t * (1f - t);
            return position;
        }

        public static int ComputeDamage(AbilityDefinition ability, CreatureTag tags)
        {
            var multiplier = (tags & CreatureTag.Undead) != 0 ? ability.undeadMultiplier : 1f;
            return Mathf.RoundToInt(ability.damage * multiplier);
        }

        void Explode()
        {
            flying = false;
            var center = transform.position;
            TargetFinder.Collect(center, definition.radius, ownerTeam, hits);
            for (int i = 0; i < hits.Count; i++)
            {
                var tags = hits[i] is HealthComponent health ? health.tags : CreatureTag.None;
                hits[i].TakeDamage(ComputeDamage(definition, tags), owner);
            }
            hits.Clear();

            if (definition.explosionPrefab != null)
            {
                var effect = pool.Get(definition.explosionPrefab, center, Quaternion.identity);
                effect.GetComponent<ExplosionEffect>().Play(pool, definition.radius);
            }
            pool.Release(gameObject);
        }
    }
}
