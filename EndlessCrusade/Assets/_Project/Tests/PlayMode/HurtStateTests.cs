using System.Collections;
using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class HurtStateTests
    {
        [UnityTest]
        public IEnumerator NonLethalHit_GoesToHurtAndReturnsToIdleAfterHurtDuration()
        {
            var go = new GameObject("Hurt");
            go.SetActive(false);
            var controller = go.AddComponent<EntityController>();
            var health = go.AddComponent<HealthComponent>();
            health.Initialize(30, Team.Player);
            go.AddComponent<MovementComponent>();
            controller.hurtDuration = 0.1f;
            go.SetActive(true);

            health.TakeDamage(5, null);
            Assert.AreEqual(EntityState.Hurt, controller.State);

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(EntityState.Idle, controller.State);

            Object.Destroy(go);
        }
    }
}
