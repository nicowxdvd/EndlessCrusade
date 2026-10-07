using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Wave Definition")]
    public class WaveDefinition : ScriptableObject
    {
        public SpawnEntry[] entries;
        public float delayBeforeNext = 4f;
    }
}
