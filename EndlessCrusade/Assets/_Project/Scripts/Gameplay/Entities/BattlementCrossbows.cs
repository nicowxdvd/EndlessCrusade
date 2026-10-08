using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class BattlementCrossbows : MonoBehaviour
    {
        public PoolService pool;
        public GameObject boltPrefab;
        public int count = 2;
        public int damage = 12;
        public float range = 9f;
        public float cooldown = 2.5f;
        public float boltSpeed = 14f;
        public float spacing = 0.8f;
        public float originHeight = 1.5f;

        float elapsed;

        void Update()
        {
            if (!HasCrossbows() || pool == null || boltPrefab == null)
                return;
            elapsed += Time.deltaTime;
            if (elapsed < cooldown)
                return;
            elapsed = 0f;
            Fire();
        }

        static bool HasCrossbows()
        {
            return LevelModifiers.Current.baseCrossbows;
        }

        public int Fire()
        {
            var fired = 0;
            for (int i = 0; i < count; i++)
            {
                var origin = transform.position + new Vector3((i - (count - 1) * 0.5f) * spacing, originHeight, 0f);
                var target = TargetFinder.FindNearest(origin, Team.Player, range);
                if (target == null || !(target is Component component))
                    continue;
                var instance = pool.Get(boltPrefab, origin, Quaternion.identity);
                instance.GetComponent<Bolt>().Launch(pool, target, component.transform, damage, gameObject, boltSpeed);
                fired++;
            }
            return fired;
        }
    }
}
