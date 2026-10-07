using System;
using System.Collections.Generic;
using EC.Core;
using EC.Data;

namespace EC.Gameplay
{
    public class WaveRunner
    {
        enum Phase { Countdown, Spawning, Delay, Finished }

        readonly struct ScheduledSpawn
        {
            public readonly float Time;
            public readonly EnemyDefinition Enemy;

            public ScheduledSpawn(float time, EnemyDefinition enemy) { Time = time; Enemy = enemy; }
        }

        readonly WaveDefinition[] waves;
        readonly List<ScheduledSpawn> schedule = new List<ScheduledSpawn>();
        Phase phase = Phase.Countdown;
        float timer;
        float waveTime;
        int nextSpawn;
        int alive;

        public int CurrentWave { get; private set; } = -1;
        public int TotalWaves => waves.Length;
        public int Alive => alive;
        public bool IsFinished => phase == Phase.Finished;

        public event Action<int, int> WaveStarted;
        public event Action<LevelOutcome> Ended;

        public WaveRunner(WaveDefinition[] waves, float startCountdown)
        {
            this.waves = waves ?? new WaveDefinition[0];
            timer = startCountdown;
        }

        public void Tick(float deltaTime, List<EnemyDefinition> toSpawn)
        {
            switch (phase)
            {
                case Phase.Countdown:
                case Phase.Delay:
                    timer -= deltaTime;
                    if (timer <= 0f)
                        StartNextWave();
                    break;
                case Phase.Spawning:
                    waveTime += deltaTime;
                    while (nextSpawn < schedule.Count && schedule[nextSpawn].Time <= waveTime)
                    {
                        toSpawn.Add(schedule[nextSpawn].Enemy);
                        alive++;
                        nextSpawn++;
                    }
                    if (nextSpawn >= schedule.Count && alive == 0)
                        FinishWave();
                    break;
            }
        }

        public void NotifyEnemyDied()
        {
            if (alive > 0)
                alive--;
        }

        public void Defeat()
        {
            End(LevelOutcome.Defeat);
        }

        void StartNextWave()
        {
            CurrentWave++;
            if (CurrentWave >= waves.Length)
            {
                End(LevelOutcome.Victory);
                return;
            }
            BuildSchedule(waves[CurrentWave]);
            waveTime = 0f;
            nextSpawn = 0;
            phase = Phase.Spawning;
            WaveStarted?.Invoke(CurrentWave, waves.Length);
        }

        void FinishWave()
        {
            if (CurrentWave >= waves.Length - 1)
            {
                End(LevelOutcome.Victory);
                return;
            }
            timer = waves[CurrentWave].delayBeforeNext;
            phase = Phase.Delay;
        }

        void BuildSchedule(WaveDefinition wave)
        {
            schedule.Clear();
            if (wave.boss != null)
                schedule.Add(new ScheduledSpawn(wave.bossStartDelay, wave.boss));
            foreach (var entry in wave.entries ?? new SpawnEntry[0])
            {
                if (entry.enemy == null)
                    continue;
                for (int i = 0; i < entry.count; i++)
                    schedule.Add(new ScheduledSpawn(entry.startDelay + i * entry.interval, entry.enemy));
            }
            var ordered = new List<ScheduledSpawn>(schedule.Count);
            foreach (var item in schedule)
            {
                var index = ordered.Count;
                while (index > 0 && ordered[index - 1].Time > item.Time)
                    index--;
                ordered.Insert(index, item);
            }
            schedule.Clear();
            schedule.AddRange(ordered);
        }

        void End(LevelOutcome outcome)
        {
            if (phase == Phase.Finished)
                return;
            phase = Phase.Finished;
            Ended?.Invoke(outcome);
        }
    }
}
