using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class EnemySpawner : MonoBehaviour
    {
        public PoolService pool;
        public LaneConfig lane;
        public Transform baseTarget;

        public GameObject Spawn(EnemyDefinition definition)
        {
            var height = definition.kind == EnemyKind.Flying ? definition.flightHeight : 1f;
            var position = new Vector3(lane.spawnX, lane.groundY + height, 0f);
            var instance = pool.Get(definition.prefab, position, Quaternion.identity);
            if (instance.TryGetComponent<EnemyBrain>(out var brain))
            {
                brain.pool = pool;
                brain.baseTarget = baseTarget;
            }
            if (instance.TryGetComponent<BossBrain>(out var bossBrain))
            {
                bossBrain.pool = pool;
                bossBrain.baseTarget = baseTarget;
            }
            if (definition is BossDefinition)
                EventBus<BossSpawned>.Publish(new BossSpawned(instance, definition.displayName));
            return instance;
        }
    }
}
