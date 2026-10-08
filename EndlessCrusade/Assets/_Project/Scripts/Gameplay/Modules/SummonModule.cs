using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class SummonModule : MonoBehaviour, IEnemyModule
    {
        public EnemyDefinition minion;
        public int count = 3;
        public float interval = 15f;
        public float spread = 1.2f;

        EnemyBrain brain;
        float elapsed;

        public void Initialize(EnemyBrain owner)
        {
            brain = owner;
            elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (brain == null || brain.pool == null || minion == null || minion.prefab == null)
                return;

            elapsed += deltaTime;
            if (elapsed < interval)
                return;

            elapsed = 0f;
            for (int i = 0; i < count; i++)
                Spawn(i);
        }

        public static float OffsetFor(int index, int total, float spacing)
        {
            return (index - (total - 1) * 0.5f) * spacing;
        }

        void Spawn(int index)
        {
            var position = transform.position + new Vector3(OffsetFor(index, count, spread), 1f, 0f);
            var instance = brain.pool.Get(minion.prefab, position, Quaternion.identity);
            if (!instance.TryGetComponent<EnemyBrain>(out var minionBrain))
                return;
            minionBrain.pool = brain.pool;
            minionBrain.baseTarget = brain.baseTarget;
        }
    }
}
