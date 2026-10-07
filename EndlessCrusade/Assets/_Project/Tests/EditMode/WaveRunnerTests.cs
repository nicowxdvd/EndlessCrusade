using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Gameplay;
using NUnit.Framework;
using UnityEngine;

namespace EC.Tests.EditMode
{
    public class WaveRunnerTests
    {
        EnemyDefinition warg;
        EnemyDefinition bat;
        List<EnemyDefinition> spawns;
        List<Object> created;
        List<LevelOutcome> outcomes;
        List<int> started;

        [SetUp]
        public void SetUp()
        {
            created = new List<Object>();
            warg = Create<EnemyDefinition>();
            bat = Create<EnemyDefinition>();
            spawns = new List<EnemyDefinition>();
            outcomes = new List<LevelOutcome>();
            started = new List<int>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var item in created)
                Object.DestroyImmediate(item);
        }

        T Create<T>() where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            created.Add(instance);
            return instance;
        }

        WaveDefinition Wave(float delayBeforeNext, params SpawnEntry[] entries)
        {
            var wave = Create<WaveDefinition>();
            wave.entries = entries;
            wave.delayBeforeNext = delayBeforeNext;
            return wave;
        }

        static SpawnEntry Entry(EnemyDefinition enemy, int count, float interval, float startDelay = 0f)
        {
            return new SpawnEntry { enemy = enemy, count = count, interval = interval, startDelay = startDelay };
        }

        WaveRunner Runner(float countdown, params WaveDefinition[] waves)
        {
            var runner = new WaveRunner(waves, countdown);
            runner.WaveStarted += (index, total) => started.Add(index);
            runner.Ended += outcome => outcomes.Add(outcome);
            return runner;
        }

        void Tick(WaveRunner runner, float dt)
        {
            spawns.Clear();
            runner.Tick(dt, spawns);
        }

        [Test]
        public void Countdown_DelaysFirstWave()
        {
            var runner = Runner(3f, Wave(4f, Entry(warg, 1, 1f)));

            Tick(runner, 2.9f);
            Assert.AreEqual(0, started.Count);

            Tick(runner, 0.2f);
            Assert.AreEqual(new[] { 0 }, started);
        }

        [Test]
        public void Spawns_RespectCountIntervalAndStartDelay()
        {
            var runner = Runner(0f, Wave(4f, Entry(warg, 3, 1f, 2f)));
            Tick(runner, 0.1f);

            Tick(runner, 1.8f);
            Assert.AreEqual(0, spawns.Count);

            Tick(runner, 0.3f);
            Assert.AreEqual(1, spawns.Count);

            Tick(runner, 1f);
            Assert.AreEqual(1, spawns.Count);

            Tick(runner, 1f);
            Assert.AreEqual(1, spawns.Count);

            Tick(runner, 5f);
            Assert.AreEqual(0, spawns.Count);
        }

        [Test]
        public void Spawns_AreOrderedByTimeAcrossEntries()
        {
            var runner = Runner(0f, Wave(4f, Entry(warg, 2, 2f), Entry(bat, 2, 1f, 0.5f)));
            Tick(runner, 0f);

            Tick(runner, 10f);

            Assert.AreEqual(new[] { warg, bat, bat, warg }, spawns);
        }

        [Test]
        public void NextWave_StartsAfterClearAndDelay()
        {
            var runner = Runner(0f, Wave(4f, Entry(warg, 1, 1f)), Wave(4f, Entry(bat, 1, 1f)));
            Tick(runner, 0f);
            Tick(runner, 0f);
            Assert.AreEqual(1, runner.Alive);

            Tick(runner, 10f);
            Assert.AreEqual(new[] { 0 }, started);

            runner.NotifyEnemyDied();
            Tick(runner, 0.1f);
            Tick(runner, 3.8f);
            Assert.AreEqual(new[] { 0 }, started);

            Tick(runner, 0.3f);
            Assert.AreEqual(new[] { 0, 1 }, started);
        }

        [Test]
        public void Victory_AfterLastWaveWithZeroAlive()
        {
            var runner = Runner(0f, Wave(4f, Entry(warg, 2, 1f)));
            Tick(runner, 0f);
            Tick(runner, 5f);
            Assert.AreEqual(0, outcomes.Count);

            runner.NotifyEnemyDied();
            Tick(runner, 0.1f);
            Assert.AreEqual(0, outcomes.Count);

            runner.NotifyEnemyDied();
            Tick(runner, 0.1f);
            Assert.AreEqual(new[] { LevelOutcome.Victory }, outcomes);
            Assert.IsTrue(runner.IsFinished);
        }

        [Test]
        public void Defeat_PublishesOnce_AndStopsSpawning()
        {
            var runner = Runner(0f, Wave(4f, Entry(warg, 5, 1f)));
            Tick(runner, 0f);

            runner.Defeat();
            runner.Defeat();
            Tick(runner, 10f);

            Assert.AreEqual(new[] { LevelOutcome.Defeat }, outcomes);
            Assert.AreEqual(0, spawns.Count);
        }

        [Test]
        public void Defeat_AfterVictory_IsIgnored()
        {
            var runner = Runner(0f, Wave(4f));
            Tick(runner, 0f);
            Tick(runner, 0.1f);

            runner.Defeat();

            Assert.AreEqual(new[] { LevelOutcome.Victory }, outcomes);
        }

        [Test]
        public void NotifyEnemyDied_NeverGoesBelowZero()
        {
            var runner = Runner(0f, Wave(4f, Entry(warg, 1, 1f)));

            runner.NotifyEnemyDied();

            Assert.AreEqual(0, runner.Alive);
        }
    }
}
