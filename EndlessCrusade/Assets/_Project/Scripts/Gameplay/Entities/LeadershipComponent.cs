using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public class LeadershipComponent : MonoBehaviour
    {
        public float max = 100f;
        public float regenPerSecond = 4f;
        public float startAmount = 50f;

        float current;
        bool initialized;

        public float Current
        {
            get
            {
                EnsureInitialized();
                return current;
            }
        }

        void Start()
        {
            EnsureInitialized();
            Publish();
        }

        void Update()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            EnsureInitialized();
            if (current >= max)
                return;
            current = Mathf.Min(max, current + regenPerSecond * deltaTime);
            Publish();
        }

        public bool TrySpend(float amount)
        {
            EnsureInitialized();
            if (amount > current)
                return false;
            current -= amount;
            Publish();
            return true;
        }

        public void SetCurrent(float value)
        {
            EnsureInitialized();
            current = Mathf.Clamp(value, 0f, max);
            Publish();
        }

        void EnsureInitialized()
        {
            if (initialized)
                return;
            initialized = true;
            current = Mathf.Clamp(startAmount, 0f, max);
        }

        void Publish()
        {
            EventBus<LeadershipChanged>.Publish(new LeadershipChanged(current, max));
        }
    }
}
