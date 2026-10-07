using UnityEngine;

namespace EC.Gameplay
{
    public enum StatusEffectType { Disoriented }

    public class StatusEffectComponent : MonoBehaviour
    {
        float disorientedLeft;

        public bool IsDisoriented => disorientedLeft > 0f;
        public float DisorientedRemaining => disorientedLeft;

        public void Apply(StatusEffectType type, float duration)
        {
            if (duration <= 0f)
                return;
            if (type == StatusEffectType.Disoriented)
                disorientedLeft = Mathf.Max(disorientedLeft, duration);
        }

        public void Clear()
        {
            disorientedLeft = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (disorientedLeft > 0f)
                disorientedLeft = Mathf.Max(0f, disorientedLeft - deltaTime);
        }

        void Update()
        {
            Tick(Time.deltaTime);
        }
    }
}
