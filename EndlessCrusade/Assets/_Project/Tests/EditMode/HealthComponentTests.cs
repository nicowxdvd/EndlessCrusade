using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class HealthComponentTests
    {
        GameObject go;
        HealthComponent health;
        int diedCount;
        int changedCount;

        void OnDied(EntityDied evt) { diedCount++; }
        void OnChanged(HealthChanged evt) { changedCount++; }

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Health");
            health = go.AddComponent<HealthComponent>();
            health.Initialize(30, Team.Player);
            diedCount = 0;
            changedCount = 0;
            EventBus<EntityDied>.Subscribe(OnDied);
            EventBus<HealthChanged>.Subscribe(OnChanged);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus<EntityDied>.Unsubscribe(OnDied);
            EventBus<HealthChanged>.Unsubscribe(OnChanged);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void NonLethalDamage_ReducesHealthAndPublishesChange()
        {
            health.TakeDamage(10, null);
            Assert.AreEqual(20, health.Current);
            Assert.AreEqual(1, changedCount);
            Assert.AreEqual(0, diedCount);
            Assert.IsTrue(health.IsAlive);
        }

        [Test]
        public void LethalDamage_PublishesEntityDiedOnce()
        {
            health.TakeDamage(100, null);
            health.TakeDamage(100, null);
            Assert.AreEqual(1, diedCount);
            Assert.AreEqual(1, changedCount);
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void Health_NeverGoesBelowZero()
        {
            health.TakeDamage(999, null);
            Assert.AreEqual(0, health.Current);
        }

        [Test]
        public void EventBus_ClearAll_RemovesSubscribers()
        {
            EventBusRegistry.ClearAll();
            health.TakeDamage(100, null);
            Assert.AreEqual(0, diedCount);
        }
    }
}
