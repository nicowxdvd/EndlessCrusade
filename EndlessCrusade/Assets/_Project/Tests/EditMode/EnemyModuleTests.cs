using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class EnemyModuleTests
    {
        GameObject go;
        EnemyDefinition definition;
        EnemyBrain brain;
        HealthComponent health;

        [SetUp]
        public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.team = Team.Enemy;
            definition.maxHealth = 100;
            definition.moveSpeed = 2f;
            go = new GameObject("Enemy");
            go.SetActive(false);
            go.AddComponent<EntityController>().definition = definition;
            health = go.AddComponent<HealthComponent>();
            go.AddComponent<MovementComponent>();
            go.AddComponent<AttackComponent>();
            brain = go.AddComponent<EnemyBrain>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(definition);
        }

        T AddModule<T>() where T : Component, IEnemyModule
        {
            var module = go.AddComponent<T>();
            go.SetActive(true);
            health.Initialize(100, Team.Enemy);
            module.Initialize(brain);
            return module;
        }

        [Test]
        public void Regeneration_HealsPerSecond()
        {
            var module = AddModule<RegenerationModule>();
            health.TakeDamage(50, null);

            for (int i = 0; i < 10; i++)
                module.Tick(0.1f);

            Assert.AreEqual(53, health.Current);
        }

        [Test]
        public void Regeneration_DoesNotExceedMax()
        {
            var module = AddModule<RegenerationModule>();
            health.TakeDamage(2, null);

            for (int i = 0; i < 100; i++)
                module.Tick(0.1f);

            Assert.AreEqual(100, health.Current);
        }

        [Test]
        public void Dodge_ThousandHits_DodgesAroundQuarter()
        {
            var module = AddModule<DodgeModule>();
            var random = new System.Random(1234);
            module.RandomSource = () => (float)random.NextDouble();
            health.Initialize(100000, Team.Enemy);

            for (int i = 0; i < 1000; i++)
                health.TakeDamage(1, null);

            var dodged = 1000 - (100000 - health.Current);
            Assert.GreaterOrEqual(dodged, 220);
            Assert.LessOrEqual(dodged, 280);
        }

        [Test]
        public void Dodge_ForcedRoll_AvoidsDamage()
        {
            var module = AddModule<DodgeModule>();
            module.RandomSource = () => 0f;

            health.TakeDamage(10, null);

            Assert.AreEqual(100, health.Current);
        }

        [Test]
        public void Rage_BelowHalfHealth_BoostsSpeedMultiplier()
        {
            var module = AddModule<RageModule>();

            module.Tick(0.1f);
            Assert.AreEqual(1f, brain.SpeedMultiplier, 0.0001f);

            health.TakeDamage(51, null);
            module.Tick(0.1f);
            Assert.AreEqual(1.4f, brain.SpeedMultiplier, 0.0001f);
        }

        [Test]
        public void Blink_AfterInterval_MovesDistance()
        {
            var module = AddModule<BlinkModule>();
            go.GetComponent<EntityController>().Request(EntityState.Move);
            brain.Movement.Direction = -1f;
            var start = go.transform.position.x;

            module.Tick(5.9f);
            Assert.AreEqual(start, go.transform.position.x, 0.0001f);
            module.Tick(0.2f);

            Assert.AreEqual(start - 4f, go.transform.position.x, 0.0001f);
        }

        [Test]
        public void Lifesteal_HealsThirtyPercentOfDamage()
        {
            var module = AddModule<LifestealModule>();
            health.TakeDamage(60, null);
            var attack = go.GetComponent<AttackComponent>();
            var victim = new GameObject("Victim").AddComponent<HealthComponent>();
            victim.Initialize(1000, Team.Player);
            attack.damage = 20;
            attack.cooldown = 0f;
            go.GetComponent<EntityController>().Request(EntityState.Attack);

            attack.TryAttack(victim, 1f);
            attack.TryAttack(victim, 2f);
            attack.TryAttack(victim, 3f);

            Assert.AreEqual(40 + 18, health.Current);
            Object.DestroyImmediate(victim.gameObject);
        }
    }
}
