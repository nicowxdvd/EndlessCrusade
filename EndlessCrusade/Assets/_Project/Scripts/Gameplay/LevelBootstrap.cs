using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class LevelBootstrap : MonoBehaviour
    {
        public LevelDefinition fallbackLevel;
        public LaneConfig lane;
        public WaveController waves;
        public EnemySpawner spawner;
        public GameObject[] hiddenWithoutTroops;

        void Awake()
        {
            var level = LevelSession.Current != null ? LevelSession.Current : fallbackLevel;
            if (level == null)
            {
                Debug.LogError("[Level] sin LevelDefinition: LevelSession.Current y fallbackLevel son nulos", this);
                return;
            }
            LevelSession.Current = level;

            if (level.environmentPrefab != null)
                Instantiate(level.environmentPrefab).name = "Environment";

            var baseTransform = SpawnBase(level);
            if (baseTransform != null)
                spawner.baseTarget = baseTransform;

            waves.level = level;
            waves.deferStart = true;

            foreach (var item in hiddenWithoutTroops)
                item.SetActive(level.troopsEnabled);
        }

        Transform SpawnBase(LevelDefinition level)
        {
            var definition = level.baseDefinition;
            if (definition == null || definition.prefab == null)
                return null;
            var instance = Instantiate(definition.prefab, new Vector3(lane.baseX, lane.groundY + 1.5f, 0f), Quaternion.identity);
            instance.name = "Base";
            instance.GetComponent<BaseStructure>().definition = definition;
            return instance.transform;
        }
    }
}
