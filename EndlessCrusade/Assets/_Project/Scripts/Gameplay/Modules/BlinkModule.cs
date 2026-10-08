using UnityEngine;

namespace EC.Gameplay
{
    public class BlinkModule : MonoBehaviour, IEnemyModule
    {
        public float distance = 4f;
        public float interval = 6f;

        EnemyBrain brain;
        float elapsed;

        public void Initialize(EnemyBrain owner)
        {
            brain = owner;
            elapsed = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (brain == null || brain.Controller.State != EntityState.Move)
                return;

            elapsed += deltaTime;
            if (elapsed < interval)
                return;

            elapsed = 0f;
            brain.Movement.Step(brain.Movement.Direction, 1f, distance);
        }
    }
}
