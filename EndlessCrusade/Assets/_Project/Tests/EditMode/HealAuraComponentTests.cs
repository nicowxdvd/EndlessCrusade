using System.Collections.Generic;
using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class HealAuraComponentTests
    {
        readonly List<GameObject> created = new List<GameObject>();
        HealAuraComponent aura;
        HealthComponent priest;

        HealthComponent Unit(string name, Team team, float x, int max, int damage)
        {
            var go = new GameObject(name);
            created.Add(go);
            go.transform.position = new Vector3(x, 0f, 0f);
            var health = go.AddComponent<HealthComponent>();
            health.Initialize(max, team);
            TargetFinder.Register(health, go.transform);
            if (damage > 0)
                health.TakeDamage(damage, null);
            return health;
        }

        [SetUp]
        public void SetUp()
        {
            TargetFinder.Clear();
            priest = Unit("Priest", Team.Player, 0f, 70, 0);
            aura = priest.gameObject.AddComponent<HealAuraComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in created) Object.DestroyImmediate(go);
            created.Clear();
            TargetFinder.Clear();
        }

        [Test]
        public void Tick_BeforeInterval_DoesNotHeal()
        {
            var ally = Unit("Ally", Team.Player, 2f, 100, 50);
            aura.Tick(1.9f);
            Assert.AreEqual(50, ally.Current);
        }

        [Test]
        public void Tick_AtInterval_HealsWoundedAllyInRadius()
        {
            var ally = Unit("Ally", Team.Player, 2f, 100, 50);
            aura.Tick(1.9f);
            aura.Tick(0.2f);
            Assert.AreEqual(60, ally.Current);
        }

        [Test]
        public void Pulse_NeverExceedsMaxHealth()
        {
            var ally = Unit("Ally", Team.Player, 1f, 100, 4);
            aura.Pulse();
            Assert.AreEqual(100, ally.Current);
        }

        [Test]
        public void Pulse_IgnoresEnemiesAndAlliesOutOfRadius()
        {
            var enemy = Unit("Enemy", Team.Enemy, 1f, 100, 50);
            var far = Unit("Far", Team.Player, 6f, 100, 50);
            var healed = aura.Pulse();
            Assert.AreEqual(0, healed);
            Assert.AreEqual(50, enemy.Current);
            Assert.AreEqual(50, far.Current);
        }

        [Test]
        public void Pulse_HealsSelfWhenWounded()
        {
            priest.TakeDamage(30, null);
            aura.Pulse();
            Assert.AreEqual(50, priest.Current);
        }
    }
}
