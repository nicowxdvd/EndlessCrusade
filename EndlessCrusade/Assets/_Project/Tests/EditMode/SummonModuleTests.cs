using EC.Gameplay;
using NUnit.Framework;

namespace EC.Tests.EditMode
{
    public class SummonModuleTests
    {
        [Test]
        public void OffsetFor_CentersMinionsOnTheSummoner()
        {
            Assert.AreEqual(-1.2f, SummonModule.OffsetFor(0, 3, 1.2f), 0.0001f);
            Assert.AreEqual(0f, SummonModule.OffsetFor(1, 3, 1.2f), 0.0001f);
            Assert.AreEqual(1.2f, SummonModule.OffsetFor(2, 3, 1.2f), 0.0001f);
            Assert.AreEqual(0f, SummonModule.OffsetFor(0, 1, 1.2f), 0.0001f);
        }
    }
}
