using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class Bolt : MonoBehaviour, IPoolable
    {
        public float hitDistance = 0.35f;
        public float maxLifetime = 3f;

        PoolService pool;
        IDamageable target;
        Transform targetTransform;
        GameObject owner;
        int damage;
        float speed;
        float age;
        bool flying;

        public void Launch(PoolService poolService, IDamageable victim, Transform victimTransform, int amount, GameObject source, float boltSpeed)
        {
            pool = poolService;
            target = victim;
            targetTransform = victimTransform;
            damage = amount;
            owner = source;
            speed = boltSpeed;
            age = 0f;
            flying = true;
        }

        public void OnSpawn() { }

        public void OnDespawn()
        {
            flying = false;
            target = null;
            targetTransform = null;
            owner = null;
        }

        public static Vector3 Step(Vector3 position, Vector3 destination, float speed, float deltaTime)
        {
            return Vector3.MoveTowards(position, destination, speed * deltaTime);
        }

        void Update()
        {
            if (!flying)
                return;

            age += Time.deltaTime;
            if (age >= maxLifetime || targetTransform == null || !target.IsAlive)
            {
                Finish();
                return;
            }

            var destination = targetTransform.position;
            var next = Step(transform.position, destination, speed, Time.deltaTime);
            transform.position = next;
            if ((destination - next).sqrMagnitude <= hitDistance * hitDistance)
            {
                target.TakeDamage(damage, owner);
                Finish();
            }
        }

        void Finish()
        {
            flying = false;
            pool.Release(gameObject);
        }
    }
}
