using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace EC.Core
{
    public class PoolService : MonoBehaviour
    {
        readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        readonly List<GameObject> prewarmBuffer = new List<GameObject>();

        void Awake()
        {
            if (transform.parent == null) DontDestroyOnLoad(gameObject);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            var pool = GetOrCreatePool(prefab);
            prewarmBuffer.Clear();
            for (int i = 0; i < count; i++) prewarmBuffer.Add(pool.Get());
            for (int i = 0; i < prewarmBuffer.Count; i++) pool.Release(prewarmBuffer[i]);
            prewarmBuffer.Clear();
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            var instance = GetOrCreatePool(prefab).Get();
            var t = instance.transform;
            t.SetParent(null, false);
            t.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            instance.GetComponent<PooledObject>().NotifySpawn();
            return instance;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;

            if (!instance.TryGetComponent<PooledObject>(out var pooled) || pooled.Prefab == null || !pools.TryGetValue(pooled.Prefab, out var pool))
            {
                Debug.LogWarning($"[Pool] Release de '{instance.name}', que no fue creado por el pool; se destruye", instance);
                Destroy(instance);
                return;
            }

            pooled.NotifyDespawn();
            pool.Release(instance);
        }

#if UNITY_EDITOR
        public int GetActiveCount(GameObject prefab)
        {
            return pools.TryGetValue(prefab, out var pool) ? pool.CountActive : 0;
        }
#endif

        ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
        {
            if (pools.TryGetValue(prefab, out var pool)) return pool;

            pool = new ObjectPool<GameObject>(
                () => CreateInstance(prefab),
                null,
                OnReleaseToPool,
                Destroy,
                true,
                16,
                10000);
            pools.Add(prefab, pool);
            return pool;
        }

        GameObject CreateInstance(GameObject prefab)
        {
            var instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            var pooled = instance.GetComponent<PooledObject>();
            if (pooled == null) pooled = instance.AddComponent<PooledObject>();
            pooled.Prefab = prefab;
            pooled.CachePoolables();
            return instance;
        }

        void OnReleaseToPool(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
        }
    }
}
