using UnityEngine;

namespace EC.Gameplay
{
    public class LifestealModule : MonoBehaviour, IEnemyModule
    {
        [Range(0f, 1f)] public float fraction = 0.3f;

        HealthComponent health;
        AttackComponent attack;
        float accumulated;

        public void Initialize(EnemyBrain brain)
        {
            health = brain.Health;
            accumulated = 0f;
            if (attack != null)
                attack.Hit -= OnHit;
            attack = brain.Attack;
            attack.Hit += OnHit;
        }

        public void Tick(float deltaTime) { }

        void OnHit(int damage)
        {
            accumulated += damage * fraction;
            var whole = (int)(accumulated + 0.0001f);
            if (whole <= 0)
                return;
            accumulated -= whole;
            health.Heal(whole);
        }

        void OnDestroy()
        {
            if (attack != null)
                attack.Hit -= OnHit;
        }
    }
}
