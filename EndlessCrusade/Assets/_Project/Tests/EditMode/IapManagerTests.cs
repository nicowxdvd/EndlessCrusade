using System.Collections.Generic;
using System.IO;
using EC.Core;
using EC.Data;
using EC.Services;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class IapManagerTests
    {
        readonly List<Object> created = new List<Object>();
        string dir;
        JsonSaveService save;
        CurrencyService currency;
        FakeIapService iap;
        IapManager manager;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_iap_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();
            currency = new CurrencyService(save);

            var small = ScriptableObject.CreateInstance<IapProductDefinition>();
            small.productId = "gems_pack_small";
            small.gemsGranted = 80;
            created.Add(small);

            iap = new FakeIapService();
            manager = new IapManager(save, currency, new[] { small }, new LogAnalyticsService());
            iap.ReceiptReceived += receipt => manager.Handle(receipt);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created) Object.DestroyImmediate(asset);
            created.Clear();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void Purchase_DeliversExactGemsAndConfirms()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[Analytics\] iap_purchase"));
            var outcome = iap.PurchaseAsync("gems_pack_small").GetAwaiter().GetResult();

            Assert.AreEqual(IapOutcome.Delivered, outcome);
            Assert.AreEqual(80, currency.Balance(CurrencyType.Gems));
            Assert.AreEqual(1, iap.ConfirmedCount);
        }

        [Test]
        public void SameTransaction_IsDeliveredOnlyOnce()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[Analytics\] iap_purchase"));
            iap.Emit("gems_pack_small", "tx-1");
            iap.Emit("gems_pack_small", "tx-1");

            Assert.AreEqual(80, currency.Balance(CurrencyType.Gems));
            Assert.AreEqual(2, iap.ConfirmedCount);
            Assert.Contains("tx-1", save.Current.profile.processedTransactionIds);
        }

        [Test]
        public void PendingReceipt_IsDeliveredWhenRecovered()
        {
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(@"\[Analytics\] iap_purchase"));
            iap.Emit("gems_pack_small", "tx-pending");
            Assert.AreEqual(80, currency.Balance(CurrencyType.Gems));
        }

        [Test]
        public void Cancelled_DoesNotChangeBalance()
        {
            iap.CancelNext = true;
            Assert.AreEqual(IapOutcome.Cancelled, iap.PurchaseAsync("gems_pack_small").GetAwaiter().GetResult());
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gems));
        }

        [Test]
        public void Offline_ReportsUnavailable()
        {
            iap.IsAvailable = false;
            Assert.AreEqual(IapOutcome.Unavailable, iap.PurchaseAsync("gems_pack_small").GetAwaiter().GetResult());
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gems));
        }

        [Test]
        public void UnknownProduct_IsIgnored()
        {
            iap.Emit("other", "tx-x");
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gems));
            Assert.IsEmpty(save.Current.profile.processedTransactionIds);
        }
    }
}
