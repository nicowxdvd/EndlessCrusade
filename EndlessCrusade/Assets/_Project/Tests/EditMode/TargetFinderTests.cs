using EC.Core;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class TargetFinderTests
    {
        class Stub : IDamageable
        {
            public Team Team { get; set; }
            public bool IsAlive { get; set; } = true;
            public void TakeDamage(int amount, GameObject source) { }
        }

        GameObject root;

        [SetUp]
        public void SetUp()
        {
            TargetFinder.Clear();
            root = new GameObject("Root");
        }

        [TearDown]
        public void TearDown()
        {
            TargetFinder.Clear();
            Object.DestroyImmediate(root);
        }

        Stub Add(Team team, float x, bool alive = true)
        {
            var go = new GameObject("Stub");
            go.transform.SetParent(root.transform);
            go.transform.position = new Vector3(x, 0f, 0f);
            var stub = new Stub { Team = team, IsAlive = alive };
            TargetFinder.Register(stub, go.transform);
            return stub;
        }

        [Test]
        public void FindNearest_ReturnsClosestByX()
        {
            Add(Team.Player, -10f);
            var near = Add(Team.Player, -3f);
            Add(Team.Player, -6f);

            Assert.AreSame(near, TargetFinder.FindNearest(Vector3.zero, Team.Enemy, 20f));
        }

        [Test]
        public void FindNearest_IgnoresSameTeam()
        {
            Add(Team.Enemy, 1f);
            var other = Add(Team.Player, 5f);

            Assert.AreSame(other, TargetFinder.FindNearest(Vector3.zero, Team.Enemy, 20f));
        }

        [Test]
        public void FindNearest_IgnoresDead()
        {
            Add(Team.Player, 1f, alive: false);
            var alive = Add(Team.Player, 5f);

            Assert.AreSame(alive, TargetFinder.FindNearest(Vector3.zero, Team.Enemy, 20f));
        }

        [Test]
        public void FindNearest_RespectsMaxDistance()
        {
            Add(Team.Player, 8f);

            Assert.IsNull(TargetFinder.FindNearest(Vector3.zero, Team.Enemy, 5f));
        }

        [Test]
        public void FindNearest_AfterUnregister_ReturnsNull()
        {
            var stub = Add(Team.Player, 2f);
            TargetFinder.Unregister(stub);

            Assert.IsNull(TargetFinder.FindNearest(Vector3.zero, Team.Enemy, 20f));
        }
    }
}
