using EC.Data;
using EC.Services;
using UnityEngine;

namespace EC.Gameplay
{
    public class LevelBootstrap : MonoBehaviour
    {
        public LevelDefinition fallbackLevel;
        public LaneConfig lane;
        public WaveController waves;
        public EnemySpawner spawner;
        public ShopCatalog catalog;
        public EquipmentCatalog equipmentCatalog;
        public PoolService pool;
        public GameObject boltPrefab;
        public TroopSummoner summoner;

        void Awake()
        {
            var level = LevelSession.Current != null ? LevelSession.Current : fallbackLevel;
            if (level == null)
            {
                Debug.LogError("[Level] sin LevelDefinition: LevelSession.Current y fallbackLevel son nulos", this);
                return;
            }
            LevelSession.Current = level;

            LevelModifiers.Current = catalog != null && SaveHost.Service != null
                ? StatModifierSet.Compute(catalog.upgrades, UpgradeService.Instance.GetLevel)
                : new StatModifierSet();

            if (level.environmentPrefab != null)
                Instantiate(level.environmentPrefab).name = "Environment";

            var baseTransform = SpawnBase(level);
            if (baseTransform != null)
                spawner.baseTarget = baseTransform;

            HeroLoadoutHolder.Current = equipmentCatalog != null && SaveHost.Service != null
                ? HeroLoadout.Compute(equipmentCatalog, SaveHost.Service.Current.equipment.equipped)
                : new HeroLoadout();

            if (summoner != null)
            {
                summoner.level = level;
                summoner.baseTarget = baseTransform;
            }

            waves.level = level;
            waves.deferStart = true;
        }

        Transform SpawnBase(LevelDefinition level)
        {
            var definition = level.baseDefinition;
            if (definition == null || definition.prefab == null)
                return null;
            var instance = Instantiate(definition.prefab, new Vector3(lane.baseX, lane.groundY + 1.5f, 0f), Quaternion.identity);
            instance.name = "Base";
            var structure = instance.GetComponent<BaseStructure>();
            structure.definition = definition;
            structure.resistanceMultiplier = LevelModifiers.Current.baseResistance;
            if (LevelModifiers.Current.baseCrossbows && boltPrefab != null)
            {
                var crossbows = instance.AddComponent<BattlementCrossbows>();
                crossbows.pool = pool != null ? pool : spawner.pool;
                crossbows.boltPrefab = boltPrefab;
            }
            return instance.transform;
        }
    }
}
