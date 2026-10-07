using System.Collections;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using EC.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EC.Tests.PlayMode
{
    public class TroopTests
    {
        GameObject serviceObject;
        GameObject baseMarker;
        GameObject summonerObject;
        PoolService pool;
        LaneConfig lane;
        TroopDefinition squire;
        EnemyDefinition enemyDef;
        LevelDefinition level;
        LeadershipComponent leadership;
        TroopSummoner summoner;

        [SetUp]
        public void SetUp()
        {
            serviceObject = new GameObject("PoolService");
            pool = serviceObject.AddComponent<PoolService>();
            lane = ScriptableObject.CreateInstance<LaneConfig>();
            baseMarker = new GameObject("Base");
            baseMarker.transform.position = new Vector3(lane.baseX, 0f, 0f);

            squire = ScriptableObject.CreateInstance<TroopDefinition>();
            squire.id = "squire";
            squire.team = Team.Player;
            squire.maxHealth = 120;
            squire.moveSpeed = 4f;
            squire.attackDamage = 10;
            squire.attackRange = 1.2f;
            squire.attackCooldown = 0.2f;
            squire.hurtDuration = 0.05f;
            squire.leadershipCost = 30;
            squire.summonCooldown = 1f;
            squire.prefab = CreateTroopPrefab(squire);

            enemyDef = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemyDef.team = Team.Enemy;
            enemyDef.maxHealth = 30;
            enemyDef.moveSpeed = 0f;
            enemyDef.attackDamage = 1;
            enemyDef.attackRange = 1.2f;
            enemyDef.attackCooldown = 0.5f;
            enemyDef.hurtDuration = 0.05f;

            level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.troopsEnabled = true;

            summonerObject = new GameObject("Summoner");
            summonerObject.SetActive(false);
            leadership = summonerObject.AddComponent<LeadershipComponent>();
            leadership.regenPerSecond = 0f;
            summoner = summonerObject.AddComponent<TroopSummoner>();
            summoner.leadership = leadership;
            summoner.level = level;
            summoner.troops = new[] { squire };
            summoner.pool = pool;
            summoner.lane = lane;
            summoner.baseTarget = baseMarker.transform;
            summonerObject.SetActive(true);
            leadership.SetCurrent(100f);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var brain in Object.FindObjectsByType<TroopBrain>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(brain.gameObject);
            foreach (var health in Object.FindObjectsByType<HealthComponent>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(health.gameObject);
            Object.Destroy(summonerObject);
            Object.Destroy(serviceObject);
            Object.Destroy(baseMarker);
            Object.Destroy(squire.prefab);
        }

        GameObject CreateTroopPrefab(TroopDefinition definition)
        {
            var go = new GameObject("Squire");
            go.SetActive(false);
            go.AddComponent<EntityController>().definition = definition;
            go.AddComponent<HealthComponent>();
            go.AddComponent<MovementComponent>().lane = lane;
            go.AddComponent<AttackComponent>();
            go.AddComponent<TroopBrain>();
            return go;
        }

        GameObject SpawnEnemy(float x)
        {
            var go = new GameObject("Enemy");
            go.SetActive(false);
            go.AddComponent<EntityController>().hurtDuration = 0.05f;
            go.AddComponent<HealthComponent>().Initialize(enemyDef.maxHealth, Team.Enemy);
            go.transform.position = new Vector3(x, 1f, 0f);
            go.SetActive(true);
            return go;
        }

        [Test]
        public void Summon_SpendsCostAndSpawnsNextToBase()
        {
            var result = summoner.TrySummon("squire");

            Assert.AreEqual(SummonResult.Success, result);
            Assert.AreEqual(70f, leadership.Current, 0.0001f);
            Assert.AreEqual(1, summoner.ActiveCount);
            var troop = Object.FindFirstObjectByType<TroopBrain>();
            Assert.AreEqual(lane.baseX, troop.transform.position.x, summoner.spawnOffset + 0.01f);
        }

        [Test]
        public void Summon_RejectedWithInsufficientLeadership()
        {
            leadership.SetCurrent(29f);

            Assert.AreEqual(SummonResult.NotEnoughLeadership, summoner.TrySummon("squire"));
            Assert.AreEqual(29f, leadership.Current, 0.0001f);
            Assert.AreEqual(0, summoner.ActiveCount);
        }

        [Test]
        public void Summon_RejectedWhileOnCooldown()
        {
            Assert.AreEqual(SummonResult.Success, summoner.TrySummon("squire"));
            Assert.AreEqual(SummonResult.OnCooldown, summoner.TrySummon("squire"));
            Assert.AreEqual(70f, leadership.Current, 0.0001f);
        }

        [UnityTest]
        public IEnumerator Summon_AllowedAfterCooldown()
        {
            Assert.AreEqual(SummonResult.Success, summoner.TrySummon("squire"));
            yield return new WaitForSeconds(squire.summonCooldown + 0.1f);
            Assert.AreEqual(SummonResult.Success, summoner.TrySummon("squire"));
        }

        [Test]
        public void Summon_RejectedAtActiveLimit()
        {
            squire.summonCooldown = 0f;
            for (int i = 0; i < 8; i++)
            {
                leadership.SetCurrent(100f);
                Assert.AreEqual(SummonResult.Success, summoner.TrySummon("squire"));
            }
            leadership.SetCurrent(100f);

            Assert.AreEqual(SummonResult.LimitReached, summoner.TrySummon("squire"));
            Assert.AreEqual(100f, leadership.Current, 0.0001f);
        }

        [Test]
        public void Summon_RejectedWhenTroopsDisabled()
        {
            level.troopsEnabled = false;
            Assert.AreEqual(SummonResult.Disabled, summoner.TrySummon("squire"));
        }

        [Test]
        public void SummonRequestedEvent_TriggersSummon()
        {
            EventBus<TroopSummonRequested>.Publish(new TroopSummonRequested("squire"));
            Assert.AreEqual(1, summoner.ActiveCount);
        }

        [UnityTest]
        public IEnumerator Squire_AdvancesAndKillsEnemy()
        {
            var enemy = SpawnEnemy(lane.baseX + 6f);
            summoner.TrySummon("squire");

            var timeout = Time.time + 10f;
            var health = enemy.GetComponent<HealthComponent>();
            while (health.IsAlive && Time.time < timeout)
                yield return null;

            Assert.IsFalse(health.IsAlive);
        }

        [UnityTest]
        public IEnumerator Squire_WithoutEnemies_StaysNearBase()
        {
            summoner.TrySummon("squire");
            var troop = Object.FindFirstObjectByType<TroopBrain>();
            troop.transform.position = new Vector3(lane.baseX + 10f, 1f, 0f);

            yield return new WaitForSeconds(4f);

            Assert.Less(troop.transform.position.x, lane.baseX + troop.rallyOffset + 1f);
        }

        [UnityTest]
        public IEnumerator Squire_DiesAtZeroHealth_AndReturnsToPool()
        {
            summoner.TrySummon("squire");
            var troop = Object.FindFirstObjectByType<TroopBrain>();
            var health = troop.GetComponent<HealthComponent>();
            yield return null;

            health.TakeDamage(1000, null);
            Assert.AreEqual(EntityState.Dead, troop.GetComponent<EntityController>().State);
            Assert.AreEqual(0, summoner.ActiveCount);
            yield return new WaitForSeconds(troop.despawnDelay + 0.3f);

            Assert.IsFalse(troop.gameObject.activeSelf);
        }

        [Test]
        public void SummonAndDeathCycles_DoNotAllocate()
        {
            squire.summonCooldown = 0f;
            pool.Prewarm(squire.prefab, 1);
            for (int i = 0; i < 3; i++)
                Cycle();

            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 30; i++)
                Cycle();
            var delta = System.GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.AreEqual(0, delta);
        }

        void Cycle()
        {
            leadership.SetCurrent(100f);
            summoner.TrySummon("squire");
            var brain = Object.FindFirstObjectByType<TroopBrain>();
            brain.GetComponent<HealthComponent>().TakeDamage(1000, null);
            pool.Release(brain.gameObject);
        }

        [Test]
        public void TroopBar_HiddenWhenTroopsDisabled_VisibleWhenEnabled()
        {
            var root = new GameObject("Hud");
            root.SetActive(false);
            var presenter = root.AddComponent<TroopBarPresenter>();
            var bar = new GameObject("Bar");
            bar.transform.SetParent(root.transform);
            presenter.bar = bar;
            var buttonObject = new GameObject("Button");
            buttonObject.transform.SetParent(bar.transform);
            buttonObject.AddComponent<Image>();
            presenter.slots = new[] { new TroopBarPresenter.Slot { troop = squire, button = buttonObject.AddComponent<Button>() } };
            root.SetActive(true);

            EventBus<TroopsAvailable>.Publish(new TroopsAvailable(false));
            Assert.IsFalse(bar.activeSelf);
            EventBus<TroopsAvailable>.Publish(new TroopsAvailable(true));
            Assert.IsTrue(bar.activeSelf);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Summoner_PublishesAvailabilityFromLevel()
        {
            bool? received = null;
            System.Action<TroopsAvailable> handler = e => received = e.Enabled;
            level.troopsEnabled = false;
            EventBus<TroopsAvailable>.Subscribe(handler);
            var go = new GameObject("S2");
            go.SetActive(false);
            go.AddComponent<LeadershipComponent>();
            var s2 = go.AddComponent<TroopSummoner>();
            s2.level = level;
            s2.troops = new TroopDefinition[0];
            go.SetActive(true);
            s2.SendMessage("Start");
            EventBus<TroopsAvailable>.Unsubscribe(handler);
            Object.DestroyImmediate(go);

            Assert.AreEqual(false, received);
        }
    }
}
