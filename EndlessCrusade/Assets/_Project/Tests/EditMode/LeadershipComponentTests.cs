using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class LeadershipComponentTests
    {
        GameObject go;
        LeadershipComponent leadership;

        [SetUp]
        public void SetUp()
        {
            go = new GameObject("Leadership");
            leadership = go.AddComponent<LeadershipComponent>();
            leadership.SetCurrent(0f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Regenerates4PerSecond()
        {
            leadership.Tick(1f);
            Assert.AreEqual(4f, leadership.Current, 0.0001f);
            leadership.Tick(2.5f);
            Assert.AreEqual(14f, leadership.Current, 0.0001f);
        }

        [Test]
        public void DoesNotExceedMax()
        {
            leadership.SetCurrent(98f);
            leadership.Tick(10f);
            Assert.AreEqual(100f, leadership.Current, 0.0001f);
            leadership.Tick(10f);
            Assert.AreEqual(100f, leadership.Current, 0.0001f);
        }

        [Test]
        public void TrySpend_RejectsWhenInsufficient()
        {
            leadership.SetCurrent(20f);
            Assert.IsFalse(leadership.TrySpend(30f));
            Assert.AreEqual(20f, leadership.Current, 0.0001f);
            Assert.IsTrue(leadership.TrySpend(15f));
            Assert.AreEqual(5f, leadership.Current, 0.0001f);
        }

        [Test]
        public void PublishesLeadershipChanged()
        {
            LeadershipChanged last = default;
            System.Action<LeadershipChanged> handler = e => last = e;
            EventBus<LeadershipChanged>.Subscribe(handler);
            leadership.Tick(1f);
            EventBus<LeadershipChanged>.Unsubscribe(handler);
            Assert.AreEqual(4f, last.Current, 0.0001f);
            Assert.AreEqual(100f, last.Max, 0.0001f);
        }
    }
}
