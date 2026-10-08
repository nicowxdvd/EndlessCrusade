using System.Collections;
using EC.Core;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EC.Tests.PlayMode
{
    public class PoolServiceTests
    {
        class Probe : MonoBehaviour, IPoolable
        {
            public int spawns;
            public int despawns;
            public void OnSpawn() { spawns++; }
            public void OnDespawn() { despawns++; }
        }

        GameObject serviceObject;
        PoolService service;
        GameObject prefab;

        [SetUp]
        public void SetUp()
        {
            serviceObject = new GameObject("PoolService");
            service = serviceObject.AddComponent<PoolService>();
            prefab = new GameObject("TestPrefab");
            prefab.AddComponent<Probe>();
            prefab.SetActive(false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(serviceObject);
            Object.Destroy(prefab);
        }

        int CountInstances()
        {
            int count = 0;
            foreach (var pooled in Object.FindObjectsByType<PooledObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (pooled.Prefab == prefab) count++;
            return count;
        }

        [Test]
        public void Get_AfterPrewarm_DoesNotInstantiateNewObjects()
        {
            service.Prewarm(prefab, 10);
            int createdAfterPrewarm = CountInstances();

            var taken = new GameObject[10];
            for (int i = 0; i < 10; i++) taken[i] = service.Get(prefab, Vector3.zero, Quaternion.identity);

            Assert.AreEqual(10, createdAfterPrewarm);
            Assert.AreEqual(createdAfterPrewarm, CountInstances());
        }

        [Test]
        public void Get_WithEmptyPool_InstantiatesNewInstance()
        {
            var instance = service.Get(prefab, new Vector3(1, 2, 3), Quaternion.identity);

            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.activeSelf);
            Assert.AreEqual(new Vector3(1, 2, 3), instance.transform.position);
        }

        [Test]
        public void Release_DeactivatesAndReturnsToPool()
        {
            service.Prewarm(prefab, 1);
            var first = service.Get(prefab, Vector3.zero, Quaternion.identity);

            service.Release(first);
            var second = service.Get(prefab, Vector3.zero, Quaternion.identity);

            Assert.AreSame(first, second);
            service.Release(second);
            Assert.IsFalse(second.activeSelf);
        }

        [Test]
        public void SpawnAndDespawn_AreCalledOncePerCycle()
        {
            var instance = service.Get(prefab, Vector3.zero, Quaternion.identity);
            var probe = instance.GetComponent<Probe>();
            Assert.AreEqual(1, probe.spawns);
            Assert.AreEqual(0, probe.despawns);

            service.Release(instance);
            Assert.AreEqual(1, probe.spawns);
            Assert.AreEqual(1, probe.despawns);

            instance = service.Get(prefab, Vector3.zero, Quaternion.identity);
            Assert.AreEqual(2, probe.spawns);
        }

        [Test]
        public void Prewarm_DoesNotCallSpawnOrDespawn()
        {
            service.Prewarm(prefab, 1);
            var instance = service.Get(prefab, Vector3.zero, Quaternion.identity);
            var probe = instance.GetComponent<Probe>();

            Assert.AreEqual(1, probe.spawns);
            Assert.AreEqual(0, probe.despawns);
        }

        [Test]
        public void Release_ForeignObject_WarnsAndDestroysWithoutException()
        {
            var foreign = new GameObject("Foreign");

            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("no fue creado por el pool"));
            Assert.DoesNotThrow(() => service.Release(foreign));
        }

        [UnityTest]
        public IEnumerator Release_ForeignObject_IsDestroyed()
        {
            var foreign = new GameObject("Foreign");
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("no fue creado por el pool"));

            service.Release(foreign);
            yield return null;

            Assert.IsTrue(foreign == null);
        }

        [Test]
        public void ActiveCount_TracksInstancesInUse()
        {
            service.Prewarm(prefab, 2);
            var a = service.Get(prefab, Vector3.zero, Quaternion.identity);
            service.Get(prefab, Vector3.zero, Quaternion.identity);
            Assert.AreEqual(2, service.GetActiveCount(prefab));

            service.Release(a);
            Assert.AreEqual(1, service.GetActiveCount(prefab));
        }

        [UnityTest]
        public IEnumerator Service_SurvivesSceneLoad()
        {
            yield return null;
            var holder = new GameObject("PoolHolder");
            var persistent = holder.AddComponent<PoolService>();
            yield return null;
            Assert.AreEqual("DontDestroyOnLoad", holder.scene.name);

            var load = SceneManager.LoadSceneAsync("LaneSandbox");
            while (!load.isDone) yield return null;

            Assert.IsTrue(persistent != null);
            Object.Destroy(holder);
        }

        [UnityTest]
        public IEnumerator BootScene_PoolServiceSurvivesLoadOfMain()
        {
            Object.Destroy(serviceObject);
            yield return null;

            var load = SceneManager.LoadSceneAsync("Boot");
            while (!load.isDone) yield return null;
            for (int i = 0; i < 5; i++) yield return null;

            Assert.AreEqual("Main", SceneManager.GetActiveScene().name);
            var found = Object.FindFirstObjectByType<PoolService>();
            Assert.IsNotNull(found);
            Assert.AreEqual("DontDestroyOnLoad", found.gameObject.scene.name);
            Object.Destroy(found.gameObject);
        }

        [UnityTest]
        public IEnumerator ThousandCycles_AllocateZeroBytes()
        {
            service.Prewarm(prefab, 1);
            var instance = service.Get(prefab, Vector3.zero, Quaternion.identity);
            service.Release(instance);
            yield return null;

            var position = Vector3.zero;
            var rotation = Quaternion.identity;
            long threadBefore = System.GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                var go = service.Get(prefab, position, rotation);
                service.Release(go);
            }
            long threadDelta = System.GC.GetAllocatedBytesForCurrentThread() - threadBefore;

            using (var recorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame"))
            {
                Assert.IsTrue(recorder.Valid, "GC Allocated In Frame no disponible");
                yield return null;
                for (int i = 0; i < 1000; i++)
                {
                    var go = service.Get(prefab, position, rotation);
                    service.Release(go);
                }
                yield return null;
                UnityEngine.Debug.Log("[GC] thread=" + threadDelta + " frame=" + recorder.LastValue);
            }

            Assert.AreEqual(0, threadDelta, "El bucle Get/Release asigno memoria");
        }
    }
}
