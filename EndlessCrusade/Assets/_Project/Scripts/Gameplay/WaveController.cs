using System.Collections.Generic;
using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class WaveController : MonoBehaviour
    {
        public LevelDefinition level;
        public EnemySpawner spawner;
        public float startCountdown = 3f;
        public bool deferStart;
        public ReviveService revive;

        readonly List<EnemyDefinition> pending = new List<EnemyDefinition>();
        readonly HashSet<GameObject> spawned = new HashSet<GameObject>();
        WaveRunner runner;
        bool started;

        void OnEnable()
        {
            EventBus<WavesStartRequested>.Subscribe(OnWavesStartRequested);
            EventBus<EntityDied>.Subscribe(OnEntityDied);
            EventBus<BaseDestroyed>.Subscribe(OnBaseDestroyed);
        }

        void OnDisable()
        {
            EventBus<WavesStartRequested>.Unsubscribe(OnWavesStartRequested);
            EventBus<EntityDied>.Unsubscribe(OnEntityDied);
            EventBus<BaseDestroyed>.Unsubscribe(OnBaseDestroyed);
        }

        void Start()
        {
            runner = new WaveRunner(level.waves, startCountdown);
            runner.WaveStarted += OnWaveStarted;
            runner.Ended += OnEnded;
            started = !deferStart;
        }

        void OnWavesStartRequested(WavesStartRequested evt)
        {
            started = true;
        }

        void Update()
        {
            if (!started || runner == null || runner.IsFinished)
                return;
            pending.Clear();
            runner.Tick(Time.deltaTime, pending);
            foreach (var definition in pending)
                spawned.Add(spawner.Spawn(definition));
        }

        void OnEntityDied(EntityDied evt)
        {
            if (runner == null || evt.Source == null)
                return;
            if (spawned.Remove(evt.Source))
                runner.NotifyEnemyDied();
            else if (evt.Source.GetComponent<HeroController>() != null && (revive == null || !revive.TryRevive(evt.Source)))
                runner.Defeat();
        }

        void OnBaseDestroyed(BaseDestroyed evt)
        {
            runner?.Defeat();
        }

        void OnWaveStarted(int index, int total)
        {
            EventBus<WaveStarted>.Publish(new WaveStarted(index, total));
        }

        void OnEnded(LevelOutcome outcome)
        {
            EventBus<LevelEnded>.Publish(new LevelEnded(outcome));
        }
    }
}
