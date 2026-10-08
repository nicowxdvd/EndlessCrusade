using UnityEngine;

namespace EC.Core
{
    public readonly struct BossSpawned
    {
        public readonly GameObject Boss;
        public readonly string DisplayName;

        public BossSpawned(GameObject boss, string displayName) { Boss = boss; DisplayName = displayName; }
    }

    public readonly struct CameraShakeRequested
    {
        public readonly float Intensity;
        public readonly float Duration;

        public CameraShakeRequested(float intensity, float duration) { Intensity = intensity; Duration = duration; }
    }

    public readonly struct BossPhaseChanged
    {
        public readonly GameObject Boss;
        public readonly int Phase;

        public BossPhaseChanged(GameObject boss, int phase) { Boss = boss; Phase = phase; }
    }
}
