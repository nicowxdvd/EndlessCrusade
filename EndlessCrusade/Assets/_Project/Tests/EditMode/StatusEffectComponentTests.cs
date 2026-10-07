using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class StatusEffectComponentTests
    {
        GameObject go;
        StatusEffectComponent status;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Status");
            status = go.AddComponent<StatusEffectComponent>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Disoriented_LastsExactlyItsDuration()
        {
            status.Apply(StatusEffectType.Disoriented, 3f);
            Assert.IsTrue(status.IsDisoriented);

            status.Tick(2.9f);
            Assert.IsTrue(status.IsDisoriented);

            status.Tick(0.2f);
            Assert.IsFalse(status.IsDisoriented);
        }

        [Test]
        public void Reapplying_KeepsTheLongestRemaining()
        {
            status.Apply(StatusEffectType.Disoriented, 3f);
            status.Tick(2f);
            status.Apply(StatusEffectType.Disoriented, 3f);
            Assert.AreEqual(3f, status.DisorientedRemaining, 0.001f);

            status.Apply(StatusEffectType.Disoriented, 1f);
            Assert.AreEqual(3f, status.DisorientedRemaining, 0.001f);
        }

        [Test]
        public void ZeroDuration_DoesNothing()
        {
            status.Apply(StatusEffectType.Disoriented, 0f);
            Assert.IsFalse(status.IsDisoriented);
        }

        [Test]
        public void Clear_RemovesTheEffect()
        {
            status.Apply(StatusEffectType.Disoriented, 3f);
            status.Clear();
            Assert.IsFalse(status.IsDisoriented);
        }
    }

    public class CameraShakerTests
    {
        [Test]
        public void Amplitude_DecaysLinearlyToZero()
        {
            Assert.AreEqual(0.4f, CameraShaker.Amplitude(0.4f, 0.5f, 0.5f), 0.001f);
            Assert.AreEqual(0.2f, CameraShaker.Amplitude(0.4f, 0.25f, 0.5f), 0.001f);
            Assert.AreEqual(0f, CameraShaker.Amplitude(0.4f, 0f, 0.5f));
            Assert.AreEqual(0f, CameraShaker.Amplitude(0.4f, 1f, 0f));
        }
    }
}
