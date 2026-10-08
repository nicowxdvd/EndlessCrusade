using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class RangedAttackComponent : AttackComponent
    {
        public GameObject boltPrefab;
        public PoolService pool;
        public float boltSpeed = 14f;
        public float originHeight = 0.6f;

        protected override void Deliver(IDamageable target, int amount)
        {
            if (boltPrefab == null || pool == null || !(target is Component component))
            {
                base.Deliver(target, amount);
                return;
            }

            var origin = transform.position + Vector3.up * originHeight;
            var instance = pool.Get(boltPrefab, origin, Quaternion.identity);
            instance.GetComponent<Bolt>().Launch(pool, target, component.transform, amount, gameObject, boltSpeed);
        }
    }
}
