using System;
using System.Collections.Generic;
using System.IO;
using EC.Core;
using EC.Data;
using EC.Services;
using EC.UI;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class PachinkoTests
    {
        readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
        string dir;
        JsonSaveService save;
        CurrencyService currency;
        UpgradeService upgrades;
        PrizeTable table;
        long now;

        PrizeEntry Entry(string id, PrizeKind kind, int amount, int weight, string item = null)
        {
            return new PrizeEntry { id = id, displayName = id, kind = kind, amount = amount, weight = weight, itemId = item };
        }

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_pk_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();
            currency = new CurrencyService(save);
            upgrades = new UpgradeService(save, currency);
            now = 3000L * PachinkoService.DayMs + 5000;

            table = ScriptableObject.CreateInstance<PrizeTable>();
            created.Add(table);
            table.entries = new[]
            {
                Entry("gold100", PrizeKind.Gold, 100, 40),
                Entry("gold500", PrizeKind.Gold, 500, 20),
                Entry("gems5", PrizeKind.Gems, 5, 20),
                Entry("gems25", PrizeKind.Gems, 25, 5),
                Entry("potion", PrizeKind.Consumable, 1, 10, ConsumableDefinition.HolyWaterPotionId),
                Entry("elixir", PrizeKind.Consumable, 1, 5, ConsumableDefinition.ReviveElixirId)
            };
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in created) UnityEngine.Object.DestroyImmediate(asset);
            created.Clear();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        PachinkoService Service(int seed = 1)
        {
            return new PachinkoService(save, currency, upgrades, new System.Random(seed), () => now);
        }

        [Test]
        public void Draw_TenThousandRollsMatchProbabilitiesWithinTwoPoints()
        {
            var random = new System.Random(12345);
            var counts = new int[table.entries.Length];
            for (int i = 0; i < 10000; i++)
                counts[PachinkoService.Draw(table, random)]++;

            for (int i = 0; i < counts.Length; i++)
                Assert.AreEqual(table.Probability(i) * 100f, counts[i] / 100f, 2f, table.entries[i].id);
        }

        [Test]
        public void Probabilities_SumToOne()
        {
            var sum = 0f;
            for (int i = 0; i < table.entries.Length; i++) sum += table.Probability(i);
            Assert.AreEqual(1f, sum, 0.0001f);
        }

        [Test]
        public void TryPlay_WithoutTickets_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, Service().TryPlay(table));
            Assert.AreEqual(0, currency.Balance(CurrencyType.Tickets));
        }

        [Test]
        public void TryPlay_ConsumesOneTicketAndDeliversDrawnPrize()
        {
            currency.Add(CurrencyType.Tickets, 2);
            var service = Service(7);
            var expected = PachinkoService.Draw(table, new System.Random(7));

            var index = service.TryPlay(table);

            Assert.AreEqual(expected, index);
            Assert.AreEqual(1, currency.Balance(CurrencyType.Tickets));
            var entry = table.entries[index];
            if (entry.kind == PrizeKind.Gold) Assert.AreEqual(entry.amount, currency.Balance(CurrencyType.Gold));
            if (entry.kind == PrizeKind.Gems) Assert.AreEqual(entry.amount, currency.Balance(CurrencyType.Gems));
            if (entry.kind == PrizeKind.Consumable) Assert.AreEqual(1, upgrades.GetCount(entry.itemId));
        }

        [Test]
        public void Daily_CanBeClaimedOncePerUtcDay()
        {
            var service = Service();
            Assert.IsTrue(service.ClaimDaily());
            Assert.AreEqual(1, currency.Balance(CurrencyType.Tickets));
            Assert.IsFalse(service.ClaimDaily());

            now += PachinkoService.DayMs;
            Assert.IsTrue(service.ClaimDaily());
            Assert.AreEqual(2, currency.Balance(CurrencyType.Tickets));
        }

        [Test]
        public void SlotX_SpreadsSlotsAcrossBoard()
        {
            Assert.AreEqual(-3.75f, PachinkoScreen.SlotX(0, 6, 4.5f), 0.0001f);
            Assert.AreEqual(3.75f, PachinkoScreen.SlotX(5, 6, 4.5f), 0.0001f);
            Assert.AreEqual(0f, PachinkoScreen.SlotX(0, 1, 4.5f), 0.0001f);
        }

        [Test]
        public void Steer_OnlyGuidesNearTheBottomAndReachesTarget()
        {
            Assert.AreEqual(1f, PachinkoBall.Steer(1f, 5f, 3f, -4.5f, 2f, 6f, 0.1f), 0.0001f);
            Assert.AreEqual(1.6f, PachinkoBall.Steer(1f, -3f, 3f, -4.5f, 2f, 6f, 0.1f), 0.0001f);
            Assert.AreEqual(3f, PachinkoBall.Steer(2.9f, -3f, 3f, -4.5f, 2f, 6f, 0.1f), 0.0001f);
        }

        [Test]
        public void FormatOdds_ListsRealPercentages()
        {
            var text = PachinkoScreen.FormatOdds(table);
            StringAssert.Contains("gold100: 40%", text);
            StringAssert.Contains("gems25: 5%", text);
        }
    }
}
