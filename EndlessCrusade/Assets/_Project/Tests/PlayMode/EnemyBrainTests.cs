using System.Collections;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class EnemyBrainTests
    {
        GameObject serviceObject;
        PoolService service;
        GameObject hero;
        GameObject baseMarker;
        LaneConfig lane;
        EnemyDefinition warg;
        EnemyDefinition bat;
        GameObject wargPrefab;
        GameObject batPrefab;

        [SetUp]
        public void SetUp()
        {
            serviceObject = new GameObject("PoolService");
            service = serviceObject.AddComponent<PoolService>();
            lane = ScriptableObject.CreateInstance<LaneConfig>();
            baseMarker = new GameObject("Base");
            baseMarker.transform.position = new Vector3(lane.baseX, 0f, 0f);

            warg = CreateDefinition(EnemyKind.Ground, 40, 2.5f, 12);
            bat = CreateDefinition(EnemyKind.Flying, 15, 3.5f, 6);
            wargPrefab = CreatePrefab(warg);
            batPrefab = CreatePrefab(bat);
            warg.prefab = wargPrefab;
            bat.prefab = batPrefab;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var brain in Object.FindObjectsByType<EnemyBrain>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(brain.gameObject);
            Object.Destroy(serviceObject);
            Object.Destroy(baseMarker);
            Object.Destroy(wargPrefab);
            Object.Destroy(batPrefab);
            if (hero != null)
                Object.DestroyImmediate(hero);
        }

        EnemyDefinition CreateDefinition(EnemyKind kind, int health, float speed, int damage)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.kind = kind;
            definition.team = Team.Enemy;
            definition.maxHealth = health;
            definition.moveSpeed = speed;
            definition.attackDamage = damage;
            definition.attackRange = 1.2f;
            definition.attackCooldown = 0.2f;
            definition.hurtDuration = 0.05f;
            return definition;
        }

        GameObject CreatePrefab(EnemyDefinition definition)
        {
            var go = new GameObject(definition.kind.ToString());
            go.SetActive(false);
            go.AddComponent<EntityController>().definition = definition;
            go.AddComponent<HealthComponent>();
            go.AddComponent<MovementComponent>().lane = lane;
            go.AddComponent<AttackComponent>();
            go.AddComponent<EnemyBrain>();
            return go;
        }

        GameObject CreateHero(float x, int health)
        {
            var go = new GameObject("Hero");
            go.SetActive(false);
            go.AddComponent<HealthComponent>().Initialize(health, Team.Player);
            go.transform.position = new Vector3(x, 1f, 0f);
            go.SetActive(true);
            return go;
        }

        GameObject SpawnEnemy(EnemyDefinition definition, float x, float y = 1f)
        {
            var instance = service.Get(definition.prefab, new Vector3(x, y, 0f), Quaternion.identity);
            var brain = instance.GetComponent<EnemyBrain>();
            brain.pool = service;
            brain.baseTarget = baseMarker.transform;
            return instance;
        }

        [UnityTest]
        public IEnumerator Warg_WithoutTargets_AdvancesTowardBase()
        {
            var enemy = SpawnEnemy(warg, 10f);
            yield return null;
            var start = enemy.transform.position.x;

            yield return new WaitForSeconds(0.5f);

            Assert.Less(enemy.transform.position.x, start - 0.5f);
        }

        [UnityTest]
        public IEnumerator Warg_AtChargeRange_AcceleratesThenRecovers()
        {
            hero = CreateHero(0f, 1000);
            var enemy = SpawnEnemy(warg, 4.5f);
            var movement = enemy.GetComponent<MovementComponent>();
            yield return null;
            yield return null;

            Assert.AreEqual(2.5f * warg.chargeSpeedMultiplier, movement.moveSpeed, 0.001f);

            yield return new WaitForSeconds(warg.chargeDuration + 0.2f);

            Assert.AreEqual(2.5f, movement.moveSpeed, 0.001f);
        }

        [UnityTest]
        public IEnumerator Warg_OutOfChargeRange_DoesNotCharge()
        {
            hero = CreateHero(0f, 1000);
            var enemy = SpawnEnemy(warg, 10f);
            var movement = enemy.GetComponent<MovementComponent>();
            yield return null;
            yield return null;

            Assert.AreEqual(2.5f, movement.moveSpeed, 0.001f);
        }

        [UnityTest]
        public IEnumerator Bat_Flies_AtHeightWithBob()
        {
            var enemy = SpawnEnemy(bat, 10f, bat.flightHeight);
            var min = float.MaxValue;
            var max = float.MinValue;
            var end = Time.time + 1.5f;
            while (Time.time < end)
            {
                yield return null;
                var y = enemy.transform.position.y;
                min = Mathf.Min(min, y);
                max = Mathf.Max(max, y);
            }

            Assert.GreaterOrEqual(min, lane.groundY + bat.flightHeight - bat.bobAmplitude - 0.01f);
            Assert.LessOrEqual(max, lane.groundY + bat.flightHeight + bat.bobAmplitude + 0.01f);
            Assert.Greater(max - min, bat.bobAmplitude * 0.5f);
        }

        [UnityTest]
        public IEnumerator Bat_AttackingHero_DescendsToAttackHeight()
        {
            hero = CreateHero(0f, 1000);
            var enemy = SpawnEnemy(bat, 0.5f, bat.flightHeight);

            yield return new WaitForSeconds(1f);

            Assert.Less(enemy.transform.position.y, bat.flightHeight - bat.bobAmplitude);
        }

        [UnityTest]
        public IEnumerator Enemy_InRange_AttacksHero()
        {
            hero = CreateHero(0f, 1000);
            SpawnEnemy(warg, 0.5f);

            yield return new WaitForSeconds(0.6f);

            Assert.Less(hero.GetComponent<HealthComponent>().Current, 1000);
        }

        [UnityTest]
        public IEnumerator Enemy_InRangeOfBase_AttacksBaseStructure()
        {
            var definition = ScriptableObject.CreateInstance<BaseDefinition>();
            var structure = baseMarker.AddComponent<BaseStructure>();
            structure.Initialize(definition);
            SpawnEnemy(warg, lane.baseX + 0.5f);

            yield return new WaitForSeconds(0.6f);

            Assert.Less(structure.Current, structure.Max);
        }

        [UnityTest]
        public IEnumerator DeadEnemy_ReturnsToPoolAndRespawnsWithFullHealth()
        {
            var enemy = SpawnEnemy(warg, 10f);
            var health = enemy.GetComponent<HealthComponent>();
            yield return null;

            health.TakeDamage(1000, null);
            Assert.AreEqual(EntityState.Dead, enemy.GetComponent<EntityController>().State);
            yield return new WaitForSeconds(enemy.GetComponent<EnemyBrain>().despawnDelay + 0.3f);

            Assert.IsFalse(enemy.activeSelf);
            Assert.AreEqual(0, service.GetActiveCount(warg.prefab));

            var again = SpawnEnemy(warg, 10f);
            Assert.AreSame(enemy, again);
            Assert.AreEqual(warg.maxHealth, health.Current);
            Assert.AreEqual(EntityState.Idle, again.GetComponent<EntityController>().State);
        }

        [Test]
        public void ThirtySpawnDeathCycles_DoNotAllocate()
        {
            service.Prewarm(warg.prefab, 1);
            for (int i = 0; i < 3; i++)
            {
                var warmup = SpawnEnemy(warg, 10f);
                warmup.GetComponent<HealthComponent>().TakeDamage(1000, null);
                service.Release(warmup);
            }

            var before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 30; i++)
            {
                var go = service.Get(warg.prefab, new Vector3(10f, 1f, 0f), Quaternion.identity);
                go.GetComponent<HealthComponent>().TakeDamage(1000, null);
                service.Release(go);
            }
            var delta = System.GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.AreEqual(0, delta);
        }
    }
}
