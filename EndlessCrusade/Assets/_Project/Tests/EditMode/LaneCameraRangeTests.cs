using EC.Gameplay;
using NUnit.Framework;

namespace EC.Tests.EditMode
{
    public class LaneCameraRangeTests
    {
        [Test]
        public void MaxCenterX_16by9_Is12()
        {
            Assert.AreEqual(12f, LaneCameraClamp.MaxCenterX(20f, 4.5f, 16f / 9f), 0.0001f);
        }

        [Test]
        public void MaxCenterX_18by9_Is11()
        {
            Assert.AreEqual(11f, LaneCameraClamp.MaxCenterX(20f, 4.5f, 18f / 9f), 0.0001f);
        }

        [Test]
        public void MaxCenterX_ViewWiderThanLane_IsZero()
        {
            Assert.AreEqual(0f, LaneCameraClamp.MaxCenterX(20f, 4.5f, 10f));
        }
    }
}
