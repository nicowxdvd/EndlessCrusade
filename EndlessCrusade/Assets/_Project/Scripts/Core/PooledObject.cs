using UnityEngine;

namespace EC.Core
{
    public class PooledObject : MonoBehaviour
    {
        IPoolable[] poolables;

        public GameObject Prefab { get; internal set; }

        internal void CachePoolables()
        {
            poolables = GetComponentsInChildren<IPoolable>(true);
        }

        internal void NotifySpawn()
        {
            for (int i = 0; i < poolables.Length; i++) poolables[i].OnSpawn();
        }

        internal void NotifyDespawn()
        {
            for (int i = 0; i < poolables.Length; i++) poolables[i].OnDespawn();
        }
    }
}
