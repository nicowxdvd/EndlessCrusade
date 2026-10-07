using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class BaseStructureTests
    {
        GameObject go;
        GameObject enemy;
        GameObject ally;
        BaseDefinition definition;
        BaseStructure structure;
        int changedCount;
        int lastCurrent;
        int destroyedCount;

        void OnChanged(BaseResistanceChanged evt) { changedCount++; lastCurrent = evt.Current; }
        void OnDestroyed(BaseDestroyed evt) { destroyedCount++; }

        [SetUp]
        public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<BaseDefinition>();
            definition.maxResistance = 500;
            go = new GameObject("Base");
            structure = go.AddComponent<BaseStructure>();
            enemy = new GameObject("Enemy");
            enemy.AddComponent<HealthComponent>().Initialize(10, Team.Enemy);
            ally = new GameObject("Ally");
            ally.AddComponent<HealthComponent>().Initialize(10, Team.Player);
            changedCount = 0;
            destroyedCount = 0;
            EventBus<BaseResistanceChanged>.Subscribe(OnChanged);
            EventBus<BaseDestroyed>.Subscribe(OnDestroyed);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<BaseResistanceChanged>.Unsubscribe(OnChanged);
            EventBus<BaseDestroyed>.Unsubscribe(OnDestroyed);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(enemy);
            Object.DestroyImmediate(ally);
            Object.DestroyImmediate(definition);
        }

        [TestCase(500, 500, 0)]
        [TestCase(331, 500, 0)]
        [TestCase(330, 500, 1)]
        [TestCase(329, 500, 1)]
        [TestCase(166, 500, 1)]
        [TestCase(165, 500, 2)]
        [TestCase(164, 500, 2)]
        [TestCase(0, 500, 2)]
        public void GetStage_RespectsThresholds(int current, int max, int expected)
        {
            Assert.AreEqual(expected, BaseStructure.GetStage(current, max));
        }

        [Test]
        public void Initialize_SetsFullResistanceAndPublishesEvent()
        {
            structure.Initialize(definition);

            Assert.AreEqual(500, structure.Current);
            Assert.AreEqual(1, changedCount);
            Assert.AreEqual(500, lastCurrent);
        }

        [Test]
        public void TakeDamage_FromEnemy_ReducesAndPublishes()
        {
            structure.Initialize(definition);
            changedCount = 0;

            structure.TakeDamage(20, enemy);

            Assert.AreEqual(480, structure.Current);
            Assert.AreEqual(1, changedCount);
        }

        [Test]
        public void TakeDamage_IgnoresPlayerNullSourceAndNonPositive()
        {
            structure.Initialize(definition);
            changedCount = 0;

            structure.TakeDamage(20, ally);
            structure.TakeDamage(20, null);
            structure.TakeDamage(0, enemy);
            structure.TakeDamage(-5, enemy);

            Assert.AreEqual(500, structure.Current);
            Assert.AreEqual(0, changedCount);
        }

        [Test]
        public void TakeDamage_ToZero_PublishesDestroyedOnce()
        {
            structure.Initialize(definition);

            structure.TakeDamage(600, enemy);
            structure.TakeDamage(10, enemy);

            Assert.AreEqual(0, structure.Current);
            Assert.IsFalse(structure.IsAlive);
            Assert.AreEqual(1, destroyedCount);
        }

        [Test]
        public void TakeDamage_AfterDestroyed_PublishesNothing()
        {
            structure.Initialize(definition);
            structure.TakeDamage(500, enemy);
            changedCount = 0;

            structure.TakeDamage(10, enemy);

            Assert.AreEqual(0, changedCount);
            Assert.AreEqual(1, destroyedCount);
        }

        [Test]
        public void Team_IsPlayer()
        {
            Assert.AreEqual(Team.Player, structure.Team);
        }
    }
}
