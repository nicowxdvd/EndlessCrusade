using System.Collections;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class BossBrainTests
    {
        GameObject serviceObject;
        PoolService service;
        GameObject baseMarker;
        GameObject hero;
        GameObject prefab;
        LaneConfig lane;
        BossDefinition definition;

        [SetUp]
        public void SetUp()
        {
            serviceObject = new GameObject("PoolService");
            service = serviceObject.AddComponent<PoolService>();
            lane = ScriptableObject.CreateInstance<LaneConfig>();
            baseMarker = new GameObject("Base");
            baseMarker.transform.position = new Vector3(lane.baseX, 0f, 0f);

            definition = ScriptableObject.CreateInstance<BossDefinition>();
            definition.team = Team.Enemy;
            definition.maxHealth = 600;
            definition.moveSpeed = 1.6f;
            definition.attackDamage = 0;
            definition.attackRange = 1.2f;
            definition.attackCooldown = 0.2f;
            definition.hurtDuration = 0.05f;
            definition.chargeRange = 8f;
            definition.chargeSpeedMultiplier = 4f;
            definition.roarInterval = 1000f;

            prefab = new GameObject("Boss");
            prefab.SetActive(false);
            prefab.AddComponent<EntityController>().definition = definition;
            prefab.AddComponent<HealthComponent>();
            prefab.AddComponent<MovementComponent>().lane = lane;
            prefab.AddComponent<AttackComponent>();
            prefab.AddComponent<BossBrain>();
            definition.prefab = prefab;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var brain in Object.FindObjectsByType<BossBrain>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(brain.gameObject);
            Object.Destroy(serviceObject);
            Object.Destroy(baseMarker);
            Object.Destroy(prefab);
            if (hero != null)
                Object.DestroyImmediate(hero);
        }

        GameObject CreateHero(float x, int health)
        {
            var go = new GameObject("Hero");
            go.SetActive(false);
            go.AddComponent<HealthComponent>().Initialize(health, Team.Player);
            go.AddComponent<StatusEffectComponent>();
            go.transform.position = new Vector3(x, 1f, 0f);
            go.SetActive(true);
            return go;
        }

        BossBrain SpawnBoss(float x)
        {
            var instance = service.Get(prefab, new Vector3(x, 1f, 0f), Quaternion.identity);
            var brain = instance.GetComponent<BossBrain>();
            brain.pool = service;
            brain.baseTarget = baseMarker.transform;
            return brain;
        }

        IEnumerator WaitForState(BossBrain brain, BossState state, float timeout)
        {
            var end = Time.time + timeout;
            while (brain.State != state && Time.time < end)
                yield return null;
            Assert.AreEqual(state, brain.State, "No llego al estado " + state);
        }

        [UnityTest]
        public IEnumerator Charge_ShowsOneSecondWarningBeforeRunning()
        {
            hero = CreateHero(4f, 1000);
            var boss = SpawnBoss(10f);

            yield return WaitForState(boss, BossState.Windup, 1f);
            var start = Time.time;
            var startX = boss.transform.position.x;
            Assert.IsTrue(boss.IsTelegraphVisible);

            while (boss.State == BossState.Windup)
            {
                Assert.AreEqual(startX, boss.transform.position.x, 0.001f);
                yield return null;
            }

            Assert.AreEqual(BossState.Charge, boss.State);
            Assert.AreEqual(definition.chargeWindup, Time.time - start, 0.1f);
            Assert.IsFalse(boss.IsTelegraphVisible);
        }

        [UnityTest]
        public IEnumerator Charge_DamagesHeroWithChargeDamage()
        {
            hero = CreateHero(4f, 1000);
            SpawnBoss(10f);

            yield return new WaitForSeconds(3f);

            Assert.AreEqual(1000 - definition.chargeDamage, hero.GetComponent<HealthComponent>().Current);
        }

        [UnityTest]
        public IEnumerator Charge_DamagesBaseWithChargeDamage()
        {
            var structure = baseMarker.AddComponent<BaseStructure>();
            structure.Initialize(ScriptableObject.CreateInstance<BaseDefinition>());
            SpawnBoss(lane.baseX + 6f);

            yield return new WaitForSeconds(3f);

            Assert.AreEqual(structure.Max - definition.chargeDamage, structure.Current);
        }

        [UnityTest]
        public IEnumerator Charge_StunsBossForStunAfterCharge()
        {
            hero = CreateHero(4f, 1000);
            var boss = SpawnBoss(10f);

            yield return WaitForState(boss, BossState.Stunned, 4f);
            var start = Time.time;
            while (boss.State == BossState.Stunned)
                yield return null;

            Assert.AreEqual(definition.stunAfterCharge, Time.time - start, 0.1f);
            Assert.AreEqual(BossState.Advance, boss.State);
        }

        [UnityTest]
        public IEnumerator Roar_DisorientsHeroInRadiusForDisorientDuration()
        {
            definition.roarInterval = 0.3f;
            definition.disorientDuration = 0.5f;
            definition.chargeRange = 4f;
            hero = CreateHero(10f - definition.roarRadius + 1f, 1000);
            var status = hero.GetComponent<StatusEffectComponent>();
            var boss = SpawnBoss(10f);

            yield return WaitForState(boss, BossState.Roar, 1f);
            Assert.IsTrue(status.IsDisoriented);

            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(status.IsDisoriented);

            yield return new WaitForSeconds(0.4f);
            Assert.IsFalse(status.IsDisoriented);
        }

        [UnityTest]
        public IEnumerator Roar_DoesNotAffectHeroOutsideRadius()
        {
            definition.roarInterval = 0.3f;
            hero = CreateHero(10f - definition.roarRadius - 2f, 1000);
            var boss = SpawnBoss(10f);

            yield return WaitForState(boss, BossState.Roar, 1f);

            Assert.IsFalse(hero.GetComponent<StatusEffectComponent>().IsDisoriented);
        }

        [UnityTest]
        public IEnumerator Roar_RequestsCameraShake()
        {
            definition.roarInterval = 0.3f;
            var shakes = 0;
            System.Action<CameraShakeRequested> handler = _ => shakes++;
            EventBus<CameraShakeRequested>.Subscribe(handler);
            var boss = SpawnBoss(10f);

            yield return WaitForState(boss, BossState.Roar, 1f);
            EventBus<CameraShakeRequested>.Unsubscribe(handler);

            Assert.AreEqual(1, shakes);
        }

        [UnityTest]
        public IEnumerator Enrage_AtThresholdMultipliesSpeed()
        {
            var boss = SpawnBoss(10f);
            var movement = boss.GetComponent<MovementComponent>();
            var health = boss.GetComponent<HealthComponent>();
            yield return null;

            health.TakeDamage(200, null);
            yield return null;
            Assert.IsFalse(boss.IsEnraged);
            Assert.AreEqual(1.6f, movement.moveSpeed, 0.001f);

            health.TakeDamage(100, null);
            yield return null;
            Assert.IsTrue(boss.IsEnraged);
            Assert.AreEqual(1.6f * definition.enrageSpeedMultiplier, movement.moveSpeed, 0.001f);

            var start = boss.transform.position.x;
            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(1.6f * definition.enrageSpeedMultiplier * 0.5f, start - boss.transform.position.x, 0.25f);
        }

        [UnityTest]
        public IEnumerator Respawn_ResetsEnrageAndState()
        {
            var boss = SpawnBoss(10f);
            var health = boss.GetComponent<HealthComponent>();
            yield return null;
            health.TakeDamage(1000, null);
            yield return new WaitForSeconds(boss.despawnDelay + 0.3f);

            var again = SpawnBoss(10f);
            yield return null;

            Assert.IsFalse(again.IsEnraged);
            Assert.AreEqual(BossState.Advance, again.State);
            Assert.AreEqual(600, again.GetComponent<HealthComponent>().Current);
        }

        [UnityTest]
        public IEnumerator Spawner_PublishesBossSpawned()
        {
            var spawner = new GameObject("Spawner").AddComponent<EnemySpawner>();
            spawner.pool = service;
            spawner.lane = lane;
            spawner.baseTarget = baseMarker.transform;
            BossSpawned received = default;
            System.Action<BossSpawned> handler = evt => received = evt;
            EventBus<BossSpawned>.Subscribe(handler);
            definition.displayName = "Licantropo Gigante";

            var boss = spawner.Spawn(definition);
            EventBus<BossSpawned>.Unsubscribe(handler);
            yield return null;

            Assert.AreSame(boss, received.Boss);
            Assert.AreEqual("Licantropo Gigante", received.DisplayName);
            Object.Destroy(spawner.gameObject);
        }

        [UnityTest]
        public IEnumerator WaveController_BossWave_EndsInVictoryOnlyWhenBossDies()
        {
            var wave = ScriptableObject.CreateInstance<WaveDefinition>();
            wave.boss = definition;
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.waves = new[] { wave };
            var spawner = new GameObject("Spawner").AddComponent<EnemySpawner>();
            spawner.pool = service;
            spawner.lane = lane;
            spawner.baseTarget = baseMarker.transform;
            var outcomes = new System.Collections.Generic.List<LevelOutcome>();
            System.Action<LevelEnded> handler = evt => outcomes.Add(evt.Outcome);
            EventBus<LevelEnded>.Subscribe(handler);

            var controllerObject = new GameObject("WaveController");
            controllerObject.SetActive(false);
            var controller = controllerObject.AddComponent<WaveController>();
            controller.level = level;
            controller.spawner = spawner;
            controller.startCountdown = 0.1f;
            controllerObject.SetActive(true);

            yield return new WaitForSeconds(0.5f);
            var boss = Object.FindFirstObjectByType<BossBrain>();
            Assert.IsNotNull(boss);
            Assert.AreEqual(0, outcomes.Count);

            boss.GetComponent<HealthComponent>().TakeDamage(10000, null);
            yield return null;
            yield return null;
            EventBus<LevelEnded>.Unsubscribe(handler);

            CollectionAssert.AreEqual(new[] { LevelOutcome.Victory }, outcomes);
            Object.Destroy(controllerObject);
            Object.Destroy(spawner.gameObject);
        }
    }
}
