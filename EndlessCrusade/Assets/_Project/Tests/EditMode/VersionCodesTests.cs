using System;
using EC.Core;
using NUnit.Framework;

namespace EC.Tests.EditMode
{
    public class VersionCodesTests
    {
        [Test]
        public void ToCode_FollowsMajorMinorPatchFormat()
        {
            Assert.AreEqual(10003, VersionCodes.ToCode("1.0.3"));
            Assert.AreEqual(100, VersionCodes.ToCode("0.1.0"));
            Assert.AreEqual(21507, VersionCodes.ToCode("2.15.7"));
        }

        [Test]
        public void ToCode_IsIncreasingWithEachRelease()
        {
            Assert.Less(VersionCodes.ToCode("1.0.3"), VersionCodes.ToCode("1.0.4"));
            Assert.Less(VersionCodes.ToCode("1.0.99"), VersionCodes.ToCode("1.1.0"));
            Assert.Less(VersionCodes.ToCode("1.99.99"), VersionCodes.ToCode("2.0.0"));
        }

        [TestCase("")]
        [TestCase("1.0")]
        [TestCase("1.0.x")]
        [TestCase("1.100.0")]
        [TestCase("-1.0.0")]
        public void ToCode_RejectsInvalidVersions(string version)
        {
            Assert.Throws<FormatException>(() => VersionCodes.ToCode(version));
        }
    }
}
