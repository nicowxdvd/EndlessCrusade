using UnityEngine;

namespace EC.Data
{
    public enum EnemyKind { Ground, Flying }

    [CreateAssetMenu(menuName = "EC/Enemy Definition")]
    public class EnemyDefinition : UnitDefinition
    {
        public EnemyKind kind;
        public int goldDrop = 5;
        public float chargeRange = 5f;
        public float chargeSpeedMultiplier = 2.2f;
        public float chargeDuration = 0.8f;
        public float flightHeight = 2.2f;
        public float bobAmplitude = 0.4f;
        public float bobFrequency = 2f;
    }
}
