using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class ExplosionEffect : MonoBehaviour, IPoolable
    {
        public float duration = 0.4f;

        PoolService pool;
        float elapsed;
        float radius;
        bool playing;

        public void Play(PoolService poolService, float explosionRadius)
        {
            pool = poolService;
            radius = explosionRadius;
            elapsed = 0f;
            playing = true;
            transform.localScale = Vector3.zero;
        }

        public void OnSpawn() { }

        public void OnDespawn()
        {
            playing = false;
        }

        void Update()
        {
            if (!playing)
                return;
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.one * (radius * 2f * t);
            if (t >= 1f)
            {
                playing = false;
                pool.Release(gameObject);
            }
        }
    }
}
