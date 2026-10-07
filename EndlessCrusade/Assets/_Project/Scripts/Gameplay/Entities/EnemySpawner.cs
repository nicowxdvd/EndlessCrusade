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
            var brain = instance.GetComponent<EnemyBrain>();
            brain.pool = pool;
            brain.baseTarget = baseTarget;
            return instance;
        }
    }
}
