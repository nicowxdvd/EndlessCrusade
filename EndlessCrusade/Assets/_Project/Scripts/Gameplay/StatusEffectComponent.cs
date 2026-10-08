using UnityEngine;

namespace EC.Gameplay
{
    public enum StatusEffectType { Disoriented, Stunned }

    public class StatusEffectComponent : MonoBehaviour
    {
        float disorientedLeft;
        float stunnedLeft;

        public bool IsDisoriented => disorientedLeft > 0f;
        public float DisorientedRemaining => disorientedLeft;
        public bool IsStunned => stunnedLeft > 0f;

        public void Apply(StatusEffectType type, float duration)
        {
            if (duration <= 0f)
                return;
            if (type == StatusEffectType.Disoriented)
                disorientedLeft = Mathf.Max(disorientedLeft, duration);
            else if (type == StatusEffectType.Stunned)
                stunnedLeft = Mathf.Max(stunnedLeft, duration);
        }

        public void Clear()
        {
            disorientedLeft = 0f;
            stunnedLeft = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (disorientedLeft > 0f)
                disorientedLeft = Mathf.Max(0f, disorientedLeft - deltaTime);
            if (stunnedLeft > 0f)
                stunnedLeft = Mathf.Max(0f, stunnedLeft - deltaTime);
        }

        void Update()
        {
            Tick(Time.deltaTime);
        }
    }
}
