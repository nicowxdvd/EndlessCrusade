using System.IO;
using EC.Core;
using EC.Services;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class AdRewardServiceTests
    {
        string dir;
        JsonSaveService save;
        CurrencyService currency;
        FakeAdService ads;
        AdRewardService service;
        long now;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_ads_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();
            currency = new CurrencyService(save);
            ads = new FakeAdService();
            now = 5000L * 60 * 60 * 1000;
            service = new AdRewardService(save, currency, ads, null, () => now);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void FreeGems_GrantsFiveAndStartsCooldown()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, "[Ads] Anuncio simulado: FreeGems");
            Assert.IsTrue(service.ClaimFreeGemsAsync().GetAwaiter().GetResult());
            Assert.AreEqual(5, currency.Balance(CurrencyType.Gems));
            Assert.IsFalse(service.CanClaimFreeGems());
        }

        [Test]
        public void FreeGems_WithoutRewardFromAd_GrantsNothing()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, "[Ads] Anuncio simulado: FreeGems");
            ads.Reward = false;
            Assert.IsFalse(service.ClaimFreeGemsAsync().GetAwaiter().GetResult());
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gems));
            Assert.IsTrue(service.CanClaimFreeGems());
        }

        [Test]
        public void DoubleGold_AddsRunGoldAgain()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, "[Ads] Anuncio simulado: DoubleGold");
            currency.Add(CurrencyType.Gold, 100);
            Assert.IsTrue(service.DoubleGoldAsync(100).GetAwaiter().GetResult());
            Assert.AreEqual(200, currency.Balance(CurrencyType.Gold));
        }

        [Test]
        public void AdNotReady_ShowsNothing()
        {
            ads.Ready = false;
            Assert.IsFalse(service.DoubleGoldAsync(50).GetAwaiter().GetResult());
            Assert.IsFalse(service.CanClaimFreeGems());
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gold));
        }
    }
}
