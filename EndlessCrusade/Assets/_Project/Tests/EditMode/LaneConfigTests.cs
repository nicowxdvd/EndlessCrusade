using EC.Data;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class LaneConfigTests
    {
        [Test]
        public void Defaults_MatchSpec()
        {
            var config = ScriptableObject.CreateInstance<LaneConfig>();

            Assert.AreEqual(20f, config.laneHalfLength);
            Assert.AreEqual(-18f, config.baseX);
            Assert.AreEqual(18f, config.spawnX);
            Assert.AreEqual(-12f, config.heroStartX);
            Assert.AreEqual(0f, config.groundY);
            Assert.AreEqual(6f, config.laneDepth);
            Assert.AreEqual(12f, config.cameraPitch);
            Assert.AreEqual(4.5f, config.cameraOrthoSize);

            Object.DestroyImmediate(config);
        }
    }
}
