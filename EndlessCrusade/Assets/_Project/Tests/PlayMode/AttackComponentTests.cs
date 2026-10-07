using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.PlayMode
{
    public class AttackComponentTests
    {
        GameObject attackerGo;
        GameObject targetGo;
        AttackComponent attack;
        EntityController attackerController;
        HealthComponent targetHealth;

        [SetUp]
        public void SetUp()
        {
            attackerGo = Build(Team.Player, 100);
            targetGo = Build(Team.Enemy, 100);
            attack = attackerGo.GetComponent<AttackComponent>();
            attack.damage = 10;
            attack.cooldown = 1f;
            attackerController = attackerGo.GetComponent<EntityController>();
            targetHealth = targetGo.GetComponent<HealthComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(attackerGo);
            Object.Destroy(targetGo);
        }

        static GameObject Build(Team team, int health)
        {
            var go = new GameObject(team.ToString());
            go.SetActive(false);
            go.AddComponent<HealthComponent>().Initialize(health, team);
            go.AddComponent<AttackComponent>();
            go.AddComponent<EntityController>();
            go.SetActive(true);
            return go;
        }

        [Test]
        public void TryAttack_BeforeCooldownElapses_IsRejected()
        {
            attackerController.Request(EntityState.Attack);

            Assert.IsTrue(attack.TryAttack(targetHealth, 0f));
            Assert.IsFalse(attack.TryAttack(targetHealth, 0.5f));
            Assert.AreEqual(90, targetHealth.Current);

            Assert.IsTrue(attack.TryAttack(targetHealth, 1.0f));
            Assert.AreEqual(80, targetHealth.Current);
        }

        [Test]
        public void TryAttack_WhenAttackerIsDead_IsRejected()
        {
            attackerGo.GetComponent<HealthComponent>().TakeDamage(1000, null);
            Assert.AreEqual(EntityState.Dead, attackerController.State);

            Assert.IsFalse(attack.TryAttack(targetHealth, 0f));
            Assert.AreEqual(100, targetHealth.Current);
        }

        [Test]
        public void Request_WhenDead_IsRejected()
        {
            attackerGo.GetComponent<HealthComponent>().TakeDamage(1000, null);

            Assert.IsFalse(attackerController.Request(EntityState.Move));
            Assert.AreEqual(EntityState.Dead, attackerController.State);
        }
    }
}
