using UnityEngine;

namespace EC.Gameplay
{
    public class RegenerationModule : MonoBehaviour, IEnemyModule
    {
        public float healthPerSecond = 3f;

        HealthComponent health;
        float accumulated;

        public void Initialize(EnemyBrain brain)
        {
            health = brain.Health;
            accumulated = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (health == null || !health.IsAlive)
                return;
            if (health.Current >= health.maxHealth)
            {
                accumulated = 0f;
                return;
            }

            accumulated += healthPerSecond * deltaTime;
            var whole = (int)(accumulated + 0.0001f);
            if (whole <= 0)
                return;
            accumulated -= whole;
            health.Heal(whole);
        }
    }
}
