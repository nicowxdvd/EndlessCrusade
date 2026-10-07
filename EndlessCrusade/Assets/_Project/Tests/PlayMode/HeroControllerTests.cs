using System.Collections;
using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class HeroControllerTests
    {
        readonly List<Object> created = new List<Object>();
        HeroDefinition definition;
        LaneConfig lane;

        [SetUp]
        public void SetUp()
        {
            lane = ScriptableObject.CreateInstance<LaneConfig>();
            lane.laneHalfLength = 10f;
            definition = ScriptableObject.CreateInstance<HeroDefinition>();
            definition.team = Team.Player;
            definition.maxHealth = 120;
            definition.moveSpeed = 3f;
            definition.sword = new MeleeAttackDefinition { id = "sword", damage = 18, range = 1.4f, cooldown = 0.5f };
            definition.whip = new MeleeAttackDefinition { id = "whip", damage = 10, range = 3f, cooldown = 0.9f };
            created.Add(lane);
            created.Add(definition);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in created)
                Object.DestroyImmediate(obj);
            created.Clear();
        }

        GameObject SpawnHero(float x)
        {
            var go = new GameObject("Hero");
            go.SetActive(false);
            go.AddComponent<EntityController>().definition = definition;
            go.AddComponent<HealthComponent>();
            go.AddComponent<MovementComponent>().lane = lane;
            go.AddComponent<AttackComponent>();
            go.AddComponent<HeroController>();
            go.transform.position = new Vector3(x, 0f, 0f);
            go.SetActive(true);
            created.Add(go);
            return go;
        }

        HealthComponent SpawnEnemy(float x, int health = 100)
        {
            var go = new GameObject("Enemy");
            go.SetActive(false);
            go.AddComponent<HealthComponent>().Initialize(health, Team.Enemy);
            go.transform.position = new Vector3(x, 0f, 0f);
            go.SetActive(true);
            created.Add(go);
            return go.GetComponent<HealthComponent>();
        }

        [Test]
        public void Sword_HitsAtExactRange()
        {
            var hero = SpawnHero(0f).GetComponent<HeroController>();
            var target = SpawnEnemy(1.4f);

            hero.Apply(false, false, true, false, 0f);

            Assert.AreEqual(82, target.Current);
        }

        [Test]
        public void Sword_DoesNotReachTargetAtTwoUnits()
        {
            var hero = SpawnHero(0f).GetComponent<HeroController>();
            var target = SpawnEnemy(2f);

            hero.Apply(false, false, true, false, 0f);

            Assert.AreEqual(100, target.Current);
        }

        [Test]
        public void Whip_HitsAtThreeUnits()
        {
            var hero = SpawnHero(0f).GetComponent<HeroController>();
            var target = SpawnEnemy(3f);

            hero.Apply(false, false, false, true, 0f);

            Assert.AreEqual(90, target.Current);
        }

        [Test]
        public void Attacks_HaveIndependentCooldowns()
        {
            var hero = SpawnHero(0f).GetComponent<HeroController>();
            var target = SpawnEnemy(1f);

            hero.Apply(false, false, true, false, 0f);
            Assert.AreEqual(82, target.Current);

            hero.Apply(false, false, true, false, 0.3f);
            Assert.AreEqual(82, target.Current);

            hero.Apply(false, false, false, true, 0.3f);
            Assert.AreEqual(72, target.Current);

            hero.Apply(false, false, true, false, 0.5f);
            Assert.AreEqual(54, target.Current);

            hero.Apply(false, false, false, true, 1.0f);
            Assert.AreEqual(54, target.Current);

            hero.Apply(false, false, false, true, 1.2f);
            Assert.AreEqual(44, target.Current);
        }

        [Test]
        public void Attack_GoesTowardFacingSide()
        {
            var hero = SpawnHero(0f).GetComponent<HeroController>();
            var left = SpawnEnemy(-1f);
            var right = SpawnEnemy(1f);

            hero.Apply(false, false, true, false, 0f);
            Assert.AreEqual(100, left.Current);
            Assert.AreEqual(82, right.Current);

            hero.Apply(true, false, true, false, 1f);
            Assert.AreEqual(82, left.Current);
            Assert.AreEqual(82, right.Current);
            Assert.AreEqual(-1, hero.Facing);
        }

        [Test]
        public void Attack_HitsOnlyNearestTarget_AndReturnsToIdle()
        {
            var heroGo = SpawnHero(0f);
            var hero = heroGo.GetComponent<HeroController>();
            var first = SpawnEnemy(1f);
            var second = SpawnEnemy(1.2f);

            hero.Apply(false, false, true, false, 0f);

            Assert.AreEqual(82, first.Current);
            Assert.AreEqual(100, second.Current);
            Assert.AreEqual(EntityState.Idle, heroGo.GetComponent<EntityController>().State);
        }

        [Test]
        public void BothDirections_CancelMovement_AndKeepFacing()
        {
            var heroGo = SpawnHero(0f);
            var hero = heroGo.GetComponent<HeroController>();

            hero.Apply(true, false, false, false, 0f);
            Assert.AreEqual(-1, hero.Facing);

            hero.Apply(true, true, false, false, 0.1f);
            Assert.AreEqual(-1, hero.Facing);
            Assert.AreEqual(0f, heroGo.GetComponent<MovementComponent>().Direction);
            Assert.AreEqual(EntityState.Idle, heroGo.GetComponent<EntityController>().State);
        }

        [UnityTest]
        public IEnumerator Moves_AndStaysInsideLaneBounds()
        {
            var heroGo = SpawnHero(9.9f);
            var hero = heroGo.GetComponent<HeroController>();
            var start = heroGo.transform.position.x;

            for (var i = 0; i < 20; i++)
            {
                hero.Apply(false, true, false, false, Time.time);
                yield return null;
            }

            Assert.GreaterOrEqual(heroGo.transform.position.x, start);
            Assert.LessOrEqual(heroGo.transform.position.x, 10f);

            for (var i = 0; i < 20; i++)
            {
                hero.Apply(true, false, false, false, Time.time);
                yield return null;
            }

            Assert.Less(heroGo.transform.position.x, start);
        }

        [Test]
        public void DeadHero_IgnoresInput()
        {
            var heroGo = SpawnHero(0f);
            var hero = heroGo.GetComponent<HeroController>();
            var target = SpawnEnemy(1f);
            heroGo.GetComponent<HealthComponent>().TakeDamage(1000, null);

            hero.Apply(true, false, true, true, 0f);

            Assert.AreEqual(EntityState.Dead, heroGo.GetComponent<EntityController>().State);
            Assert.AreEqual(100, target.Current);
            Assert.AreEqual(1, hero.Facing);
        }

        [UnityTest]
        public IEnumerator HeroSpawned_IsPublishedOnce()
        {
            var calls = 0;
            GameObject published = null;
            void Handler(HeroSpawned evt)
            {
                calls++;
                published = evt.Hero;
            }

            EventBus<HeroSpawned>.Subscribe(Handler);
            var heroGo = SpawnHero(0f);
            yield return null;
            yield return null;
            EventBus<HeroSpawned>.Unsubscribe(Handler);

            Assert.AreEqual(1, calls);
            Assert.AreSame(heroGo, published);
        }
    }
}
