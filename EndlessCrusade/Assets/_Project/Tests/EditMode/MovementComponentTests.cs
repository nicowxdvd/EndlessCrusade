using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class MovementComponentTests
    {
        GameObject go;
        LaneConfig lane;
        MovementComponent movement;

        [SetUp]
        public void SetUp()
        {
            lane = ScriptableObject.CreateInstance<LaneConfig>();
            lane.laneHalfLength = 10f;
            go = new GameObject("Mover");
            movement = go.AddComponent<MovementComponent>();
            movement.lane = lane;
            movement.moveSpeed = 5f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(lane);
        }

        [Test]
        public void Step_PastRightLimit_ClampsToLaneHalfLength()
        {
            go.transform.position = new Vector3(9f, 0f, 0f);
            movement.Step(1f, 10f);
            Assert.AreEqual(10f, go.transform.position.x, 0.0001f);
        }

        [Test]
        public void Step_PastLeftLimit_ClampsToMinusLaneHalfLength()
        {
            go.transform.position = new Vector3(-9f, 0f, 0f);
            movement.Step(-1f, 10f);
            Assert.AreEqual(-10f, go.transform.position.x, 0.0001f);
        }
    }
}
