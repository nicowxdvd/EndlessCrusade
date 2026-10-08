using EC.Data;
using EC.Services;
using NUnit.Framework;

namespace EC.Tests.EditMode
{
    public class AdLimitRulesTests
    {
        const long Hour = 60L * 60 * 1000;

        [Test]
        public void FirstClaim_IsAvailable()
        {
            Assert.IsTrue(AdLimitRules.CanClaimFreeGems(new AdLimitsData(), 1000 * Hour));
        }

        [Test]
        public void Claim_BlocksForFourHours()
        {
            var ads = new AdLimitsData();
            var now = 1000 * Hour;
            AdLimitRules.RegisterFreeGems(ads, now);

            Assert.IsFalse(AdLimitRules.CanClaimFreeGems(ads, now + 3 * Hour));
            Assert.AreEqual(Hour, AdLimitRules.MillisUntilFreeGems(ads, now + 3 * Hour));
            Assert.IsTrue(AdLimitRules.CanClaimFreeGems(ads, now + 4 * Hour));
        }

        [Test]
        public void Claims_StopAtDailyMaximum_AndResetNextDay()
        {
            var ads = new AdLimitsData();
            var start = 1000 * Hour;
            for (int i = 0; i < AdLimitRules.MaxFreeGemsClaimsPerDay; i++)
            {
                var at = start + i * 4 * Hour;
                Assert.IsTrue(AdLimitRules.CanClaimFreeGems(ads, at));
                AdLimitRules.RegisterFreeGems(ads, at);
            }

            var afterFifth = start + 5 * 4 * Hour - 1;
            Assert.IsFalse(AdLimitRules.CanClaimFreeGems(ads, start + 20 * Hour));
            Assert.IsTrue(AdLimitRules.CanClaimFreeGems(ads, start + 25 * Hour));
            Assert.AreEqual(0, ads.freeGemsToday);
            Assert.Greater(afterFifth, start);
        }

        [Test]
        public void FormatCountdown_ShowsHoursMinutesSeconds()
        {
            Assert.AreEqual("3:25:09", AdLimitRules.FormatCountdown(3 * Hour + 25 * 60 * 1000 + 9 * 1000));
        }
    }
}
