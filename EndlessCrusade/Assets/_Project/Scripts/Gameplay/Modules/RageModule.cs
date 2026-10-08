using UnityEngine;

namespace EC.Gameplay
{
    public class RageModule : MonoBehaviour, IEnemyModule
    {
        public float healthThreshold = 0.5f;
        public float speedBonus = 0.4f;

        EnemyBrain brain;

        public void Initialize(EnemyBrain owner)
        {
            brain = owner;
        }

        public void Tick(float deltaTime)
        {
            var health = brain.Health;
            var enraged = health.Current < health.maxHealth * healthThreshold;
            brain.SpeedMultiplier = enraged ? 1f + speedBonus : 1f;
        }
    }
}
