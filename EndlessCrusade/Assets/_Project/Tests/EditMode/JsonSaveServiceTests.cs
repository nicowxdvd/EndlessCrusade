using System.IO;
using EC.Data;
using EC.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.EditMode
{
    public class JsonSaveServiceTests
    {
        string _dir;

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), "ec_save_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
        }

        string SavePath => Path.Combine(_dir, "save.json");

        [Test]
        public void Load_WithoutFile_CreatesNewData()
        {
            var a = new JsonSaveService(_dir);
            a.Load();
            Assert.AreEqual(SaveData.CurrentVersion, a.Current.version);
            Assert.IsFalse(string.IsNullOrEmpty(a.Current.playerId));

            var dir2 = Path.Combine(_dir, "otro");
            var b = new JsonSaveService(dir2);
            b.Load();
            Assert.AreNotEqual(a.Current.playerId, b.Current.playerId);
        }

        [Test]
        public void SaveLoad_RoundTrip_KeepsData()
        {
            var s = new JsonSaveService(_dir);
            s.Load();
            s.Current.profile.gold = 120;
            s.Current.profile.processedTransactionIds.Add("tx1");
            s.Current.progress.completedLevels.Add("level_01");
            s.Current.upgrades.Add(new UpgradeLevel { id = "hp", level = 3 });
            s.Current.consumables.Add(new ItemCount { id = "potion", count = 2 });
            s.Current.equipment.equipped.Add("sword");
            s.Current.settings.musicVolume = 0.3f;
            s.Save();

            var l = new JsonSaveService(_dir);
            l.Load();
            Assert.AreEqual(s.Current.playerId, l.Current.playerId);
            Assert.AreEqual(120, l.Current.profile.gold);
            Assert.AreEqual("tx1", l.Current.profile.processedTransactionIds[0]);
            Assert.AreEqual("level_01", l.Current.progress.completedLevels[0]);
            Assert.AreEqual(3, l.Current.upgrades[0].level);
            Assert.AreEqual(2, l.Current.consumables[0].count);
            Assert.AreEqual("sword", l.Current.equipment.equipped[0]);
            Assert.AreEqual(0.3f, l.Current.settings.musicVolume, 0.0001f);
        }

        [Test]
        public void Save_UpdatesTimestamp()
        {
            var s = new JsonSaveService(_dir);
            s.Load();
            Assert.AreEqual(0, s.Current.updatedAtUtc);
            s.Save();
            long first = s.Current.updatedAtUtc;
            Assert.Greater(first, 0);
            System.Threading.Thread.Sleep(5);
            s.Save();
            Assert.Greater(s.Current.updatedAtUtc, first);
        }

        [Test]
        public void CorruptSave_RecoversFromBackup()
        {
            var s = new JsonSaveService(_dir);
            s.Load();
            s.Current.profile.gold = 50;
            s.Save();
            s.Current.profile.gold = 75;
            s.Save();
            File.WriteAllText(SavePath, "{ esto no es json");

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Save\] save\.json inválido"));
            var l = new JsonSaveService(_dir);
            l.Load();
            Assert.AreEqual(50, l.Current.profile.gold);
        }

        [Test]
        public void BothCorrupt_StartsFreshWithoutThrowing()
        {
            File.WriteAllText(SavePath, "basura");
            File.WriteAllText(SavePath + ".bak", "basura");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"save\.json inválido"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"save\.json\.bak inválido"));
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"se crea uno nuevo"));

            var l = new JsonSaveService(_dir);
            Assert.DoesNotThrow(() => l.Load());
            Assert.AreEqual(SaveData.CurrentVersion, l.Current.version);
            Assert.AreEqual(0, l.Current.profile.gold);
        }

        [Test]
        public void Save_DoesNotOverwriteGoodBackupWithCorruptSave()
        {
            var s = new JsonSaveService(_dir);
            s.Load();
            s.Current.profile.gold = 10;
            s.Save();
            s.Save();
            File.WriteAllText(SavePath, "roto");
            s.Save();

            LogAssert.NoUnexpectedReceived();
            var l = new JsonSaveService(_dir);
            l.Load();
            Assert.AreEqual(10, l.Current.profile.gold);
        }

        [Test]
        public void Migrate_Version0_UpgradesToCurrent()
        {
            File.WriteAllText(SavePath, "{\"version\":0,\"playerId\":\"abc\",\"profile\":{\"gold\":7}}");
            var l = new JsonSaveService(_dir);
            l.Load();
            Assert.AreEqual(SaveData.CurrentVersion, l.Current.version);
            Assert.AreEqual("abc", l.Current.playerId);
            Assert.AreEqual(7, l.Current.profile.gold);
        }

        [Test]
        public void Migrate_MissingVersionField_TreatedAsZero()
        {
            Assert.AreEqual(0, SaveMigrator.ReadVersion("{\"playerId\":\"x\"}"));
        }

        [Test]
        public void Reset_CreatesFreshData()
        {
            var s = new JsonSaveService(_dir);
            s.Load();
            s.Current.profile.gold = 99;
            string oldId = s.Current.playerId;
            s.Reset();
            Assert.AreEqual(0, s.Current.profile.gold);
            Assert.AreNotEqual(oldId, s.Current.playerId);
        }

        [Test]
        public void Migrate_Version1_AddsAdLimits()
        {
            var data = SaveMigrator.Migrate("{\"version\":1,\"playerId\":\"abc\",\"profile\":{\"gold\":7}}");
            Assert.AreEqual(SaveData.CurrentVersion, data.version);
            Assert.AreEqual(7, data.profile.gold);
            Assert.IsNotNull(data.profile.ads);
            Assert.AreEqual(0, data.profile.ads.freeGemsToday);
        }
    }
}
