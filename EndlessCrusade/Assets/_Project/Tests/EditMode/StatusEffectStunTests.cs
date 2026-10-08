using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class StatusEffectStunTests
    {
        [Test]
        public void Stun_ExpiresAfterDuration()
        {
            var go = new GameObject("Stun");
            var status = go.AddComponent<StatusEffectComponent>();
            status.Apply(StatusEffectType.Stunned, 1f);

            Assert.IsTrue(status.IsStunned);
            status.Tick(0.6f);
            Assert.IsTrue(status.IsStunned);
            status.Tick(0.6f);
            Assert.IsFalse(status.IsStunned);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Clear_RemovesStunAndDisorientation()
        {
            var go = new GameObject("Stun");
            var status = go.AddComponent<StatusEffectComponent>();
            status.Apply(StatusEffectType.Stunned, 5f);
            status.Apply(StatusEffectType.Disoriented, 5f);
            status.Clear();

            Assert.IsFalse(status.IsStunned);
            Assert.IsFalse(status.IsDisoriented);
            Object.DestroyImmediate(go);
        }
    }
}
