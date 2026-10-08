using System.IO;
using EC.Data;
using EC.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EC.Tests.EditMode
{
    public class CloudSaveServiceTests
    {
        string dir;
        JsonSaveService save;
        FakeAuthService auth;
        FakeCloudStore store;
        CloudSaveService cloud;

        [SetUp]
        public void SetUp()
        {
            dir = Path.Combine(Path.GetTempPath(), "ec_cloud_" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            save = new JsonSaveService(dir);
            save.Load();
            auth = new FakeAuthService();
            store = new FakeCloudStore();
            cloud = new CloudSaveService(auth, store, save, dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        string ConflictPath => Path.Combine(dir, CloudSaveService.ConflictFileName);

        static string JsonWith(int gold, long updatedAt)
        {
            var data = new SaveData { playerId = "p", updatedAtUtc = updatedAt };
            data.profile.gold = gold;
            return JsonUtility.ToJson(data);
        }

        [Test]
        public void Sync_WithoutCloudDocument_UploadsLocal()
        {
            save.Current.profile.gold = 12;
            save.Save();

            Assert.IsTrue(cloud.SyncAsync().GetAwaiter().GetResult());
            var doc = store.GetAsync("fake-user").GetAwaiter().GetResult();

            Assert.IsTrue(doc.Exists);
            Assert.AreEqual(save.Current.updatedAtUtc, doc.UpdatedAtUtc);
            Assert.AreEqual(12, JsonUtility.FromJson<SaveData>(doc.Json).profile.gold);
        }

        [Test]
        public void Sync_CloudNewer_RestoresCloudAndKeepsLocalAsConflict()
        {
            save.Current.profile.gold = 5;
            save.Save();
            var newer = save.Current.updatedAtUtc + 10000;
            store.SetAsync("fake-user", JsonWith(99, newer), newer).GetAwaiter().GetResult();

            Assert.IsTrue(cloud.SyncAsync().GetAwaiter().GetResult());

            Assert.AreEqual(99, save.Current.profile.gold);
            Assert.AreEqual(newer, save.Current.updatedAtUtc);
            Assert.IsTrue(File.Exists(ConflictPath));
            Assert.AreEqual(5, JsonUtility.FromJson<SaveData>(File.ReadAllText(ConflictPath)).profile.gold);
        }

        [Test]
        public void Sync_LocalNewer_UploadsLocalAndKeepsCloudAsConflict()
        {
            store.SetAsync("fake-user", JsonWith(1, 100), 100).GetAwaiter().GetResult();
            save.Current.profile.gold = 40;
            save.Save();

            Assert.IsTrue(cloud.SyncAsync().GetAwaiter().GetResult());
            var doc = store.GetAsync("fake-user").GetAwaiter().GetResult();

            Assert.AreEqual(40, JsonUtility.FromJson<SaveData>(doc.Json).profile.gold);
            Assert.AreEqual(1, JsonUtility.FromJson<SaveData>(File.ReadAllText(ConflictPath)).profile.gold);
        }

        [Test]
        public void Sync_SameTimestamp_ChangesNothing()
        {
            save.Save();
            var stamp = save.Current.updatedAtUtc;
            store.SetAsync("fake-user", JsonUtility.ToJson(save.Current), stamp).GetAwaiter().GetResult();

            Assert.IsTrue(cloud.SyncAsync().GetAwaiter().GetResult());
            Assert.IsFalse(File.Exists(ConflictPath));
        }

        [Test]
        public void Sync_WithStoreFailure_ReturnsFalseAndKeepsLocal()
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(@"\[Cloud\] Sincronización fallida"));
            save.Current.profile.gold = 7;
            store.Fail = true;

            Assert.IsFalse(cloud.SyncAsync().GetAwaiter().GetResult());
            Assert.AreEqual(7, save.Current.profile.gold);
        }
    }
}
