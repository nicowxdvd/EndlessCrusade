using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class StressHarness : MonoBehaviour
    {
        public TroopSummoner summoner;
        public int troopCount = 8;
        public float reportAfterSeconds = 60f;
        public int sampleCapacity = 4096;

        FrameTimeSampler sampler;
        float elapsed;
        bool reported;

        void Start()
        {
            sampler = new FrameTimeSampler(sampleCapacity);
            if (summoner == null || summoner.troops == null || summoner.troops.Length == 0)
                return;

            summoner.maxActiveTroops = Mathf.Max(summoner.maxActiveTroops, troopCount);
            for (int i = 0; i < troopCount; i++)
                summoner.SpawnForStress(summoner.troops[i % summoner.troops.Length]);
        }

        void Update()
        {
            sampler.Add(Time.unscaledDeltaTime * 1000f);
            elapsed += Time.unscaledDeltaTime;
            if (!reported && elapsed >= reportAfterSeconds)
            {
                reported = true;
                Debug.Log("[Stress] frames=" + sampler.Count + " avg=" + sampler.Average().ToString("0.0") + "ms p95=" + sampler.Percentile(0.95f).ToString("0.0") + "ms");
            }
        }
    }
}
