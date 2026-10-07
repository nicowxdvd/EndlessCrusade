using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Boss Definition")]
    public class BossDefinition : EnemyDefinition
    {
        public float chargeWindup = 1f;
        public int chargeDamage = 35;
        public float stunAfterCharge = 1.5f;
        public float roarInterval = 12f;
        public float roarRadius = 8f;
        public float disorientDuration = 3f;
        public float enrageThreshold = 0.5f;
        public float enrageSpeedMultiplier = 1.4f;
    }
}
