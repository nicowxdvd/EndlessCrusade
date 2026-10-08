using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Services;
using UnityEngine;

namespace EC.Gameplay
{
    public enum SummonResult { Success, UnknownTroop, Disabled, NotEnoughLeadership, OnCooldown, LimitReached, Locked }

    public class TroopSummoner : MonoBehaviour
    {
        public LevelDefinition level;
        public CampaignDefinition campaign;
        public TroopDefinition[] troops;
        public PoolService pool;
        public LaneConfig lane;
        public Transform baseTarget;
        public LeadershipComponent leadership;
        public int maxActiveTroops = 8;
        public float spawnOffset = 2.5f;

        readonly List<GameObject> active = new List<GameObject>();
        float[] readyAt;

        public bool Enabled => level == null || level.troopsEnabled;

        public int ActiveCount
        {
            get
            {
                for (int i = active.Count - 1; i >= 0; i--)
                    if (active[i] == null || !active[i].activeInHierarchy)
                        active.RemoveAt(i);
                return active.Count;
            }
        }

        void OnEnable()
        {
            EventBus<TroopSummonRequested>.Subscribe(OnSummonRequested);
            EventBus<EntityDied>.Subscribe(OnEntityDied);
        }

        void OnDisable()
        {
            EventBus<TroopSummonRequested>.Unsubscribe(OnSummonRequested);
            EventBus<EntityDied>.Unsubscribe(OnEntityDied);
        }

        void Start()
        {
            EventBus<TroopsAvailable>.Publish(new TroopsAvailable(Enabled));
        }

        public SummonResult TrySummon(string troopId)
        {
            var index = IndexOf(troopId);
            if (index < 0)
                return SummonResult.UnknownTroop;
            if (!Enabled)
                return SummonResult.Disabled;

            var troop = troops[index];
            if (!IsUnlocked(troop))
                return SummonResult.Locked;
            EnsureCooldowns();
            if (Time.time < readyAt[index])
                return SummonResult.OnCooldown;
            if (ActiveCount >= maxActiveTroops)
                return SummonResult.LimitReached;
            if (!leadership.TrySpend(troop.leadershipCost))
                return SummonResult.NotEnoughLeadership;

            readyAt[index] = Time.time + troop.summonCooldown;
            Spawn(troop);
            EventBus<TroopSummoned>.Publish(new TroopSummoned(troop.id, troop.summonCooldown));
            return SummonResult.Success;
        }

        public bool IsUnlocked(TroopDefinition troop)
        {
            if (campaign == null || SaveHost.Service == null)
                return true;
            return TroopUnlockRules.IsUnlocked(troop, campaign, SaveHost.Service.Current.progress.completedLevels);
        }

        void Spawn(TroopDefinition troop)
        {
            var baseX = baseTarget != null ? baseTarget.position.x : 0f;
            var y = lane != null ? lane.groundY + 1f : 1f;
            var instance = pool.Get(troop.prefab, new Vector3(baseX + spawnOffset, y, 0f), Quaternion.identity);
            var brain = instance.GetComponent<TroopBrain>();
            brain.pool = pool;
            brain.baseTarget = baseTarget;
            var ranged = instance.GetComponent<RangedAttackComponent>();
            if (ranged != null)
                ranged.pool = pool;
            active.Add(instance);
        }

        int IndexOf(string troopId)
        {
            if (troops == null)
                return -1;
            for (int i = 0; i < troops.Length; i++)
                if (troops[i] != null && troops[i].id == troopId)
                    return i;
            return -1;
        }

        void EnsureCooldowns()
        {
            if (readyAt == null || readyAt.Length != troops.Length)
                readyAt = new float[troops.Length];
        }

        void OnSummonRequested(TroopSummonRequested evt)
        {
            TrySummon(evt.TroopId);
        }

        void OnEntityDied(EntityDied evt)
        {
            active.Remove(evt.Source);
        }
    }
}
