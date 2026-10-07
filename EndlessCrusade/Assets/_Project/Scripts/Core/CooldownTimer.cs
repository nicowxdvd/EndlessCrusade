namespace EC.Core
{
    public class CooldownTimer
    {
        public float Duration { get; private set; }
        public float Remaining { get; private set; }
        public bool IsReady => Remaining <= 0f;
        public float Normalized => Duration <= 0f ? 0f : Remaining / Duration;

        public CooldownTimer(float duration)
        {
            Duration = duration;
        }

        public bool TryStart()
        {
            if (!IsReady)
                return false;
            Remaining = Duration;
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (Remaining <= 0f)
                return;
            Remaining -= deltaTime;
            if (Remaining < 0f)
                Remaining = 0f;
        }

        public void Reset()
        {
            Remaining = 0f;
        }
    }
}
