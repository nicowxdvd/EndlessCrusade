using System.IO;
using EC.Core;
using EC.Data;
using EC.Services;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class CurrencyServiceTests
    {
        string dir;
        JsonSaveService save;
        CurrencyService currency;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_cur_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();
            currency = new CurrencyService(save);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        [Test]
        public void Add_IncreasesBalanceAndPublishesEvent()
        {
            CurrencyChanged received = default;
            void Handler(CurrencyChanged e) { received = e; }
            EventBus<CurrencyChanged>.Subscribe(Handler);
            currency.Add(CurrencyType.Gold, 30);
            EventBus<CurrencyChanged>.Unsubscribe(Handler);

            Assert.AreEqual(30, currency.Balance(CurrencyType.Gold));
            Assert.AreEqual(CurrencyType.Gold, received.Type);
            Assert.AreEqual(30, received.Balance);
        }

        [Test]
        public void Add_IgnoresNegativeAmount()
        {
            currency.Add(CurrencyType.Gems, 5);
            currency.Add(CurrencyType.Gems, -3);
            Assert.AreEqual(5, currency.Balance(CurrencyType.Gems));
        }

        [Test]
        public void TrySpend_WithoutFunds_ReturnsFalseAndKeepsBalance()
        {
            currency.Add(CurrencyType.Gold, 10);
            Assert.IsFalse(currency.TrySpend(CurrencyType.Gold, 11));
            Assert.AreEqual(10, currency.Balance(CurrencyType.Gold));
        }

        [Test]
        public void TrySpend_WithFunds_Subtracts()
        {
            currency.Add(CurrencyType.Tickets, 3);
            Assert.IsTrue(currency.TrySpend(CurrencyType.Tickets, 2));
            Assert.AreEqual(1, currency.Balance(CurrencyType.Tickets));
        }

        [Test]
        public void Balances_PersistAfterReload()
        {
            currency.Add(CurrencyType.Gold, 77);
            currency.Add(CurrencyType.Gems, 4);
            var reloaded = new JsonSaveService(dir);
            reloaded.Load();
            var other = new CurrencyService(reloaded);
            Assert.AreEqual(77, other.Balance(CurrencyType.Gold));
            Assert.AreEqual(4, other.Balance(CurrencyType.Gems));
        }

        static LevelDefinition Level(string id)
        {
            var level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.id = id;
            level.reward = new LevelReward { goldFirstClear = 150, goldReplay = 60, gemsFirstClear = 5, ticketsFirstClear = 1 };
            return level;
        }

        [Test]
        public void Apply_FirstVictory_GrantsFullRewardAndMarksCompleted()
        {
            var level = Level("lv_test");
            var summary = RunRewards.Apply(level, LevelOutcome.Victory, 40, save, currency);

            Assert.IsTrue(summary.FirstClear);
            Assert.AreEqual(190, currency.Balance(CurrencyType.Gold));
            Assert.AreEqual(5, currency.Balance(CurrencyType.Gems));
            Assert.AreEqual(1, currency.Balance(CurrencyType.Tickets));
            Assert.Contains("lv_test", save.Current.progress.completedLevels);
            Object.DestroyImmediate(level);
        }

        [Test]
        public void Apply_Replay_GrantsOnlyReplayGold()
        {
            var level = Level("lv_test");
            save.Current.progress.completedLevels.Add("lv_test");
            var summary = RunRewards.Apply(level, LevelOutcome.Victory, 10, save, currency);

            Assert.IsFalse(summary.FirstClear);
            Assert.AreEqual(70, currency.Balance(CurrencyType.Gold));
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gems));
            Assert.AreEqual(0, currency.Balance(CurrencyType.Tickets));
            Object.DestroyImmediate(level);
        }

        [Test]
        public void Apply_Defeat_KeepsHalfGoldAndDoesNotComplete()
        {
            var level = Level("lv_test");
            RunRewards.Apply(level, LevelOutcome.Defeat, 41, save, currency);

            Assert.AreEqual(20, currency.Balance(CurrencyType.Gold));
            Assert.AreEqual(0, currency.Balance(CurrencyType.Gems));
            Assert.IsFalse(save.Current.progress.completedLevels.Contains("lv_test"));
            Object.DestroyImmediate(level);
        }
    }
}
