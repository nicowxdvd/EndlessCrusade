using System.Collections.Generic;

namespace EC.Core
{
    public class StateMachine
    {
        readonly Dictionary<EntityState, IState> states = new Dictionary<EntityState, IState>();
        IState currentState;

        public EntityState Current { get; private set; }
        public bool HasState { get; private set; }

        public void Register(EntityState id, IState state)
        {
            states[id] = state;
        }

        public bool Transition(EntityState next)
        {
            if (HasState && Current == EntityState.Dead)
                return false;
            if (HasState && Current == next)
                return false;
            if (!states.TryGetValue(next, out var state))
                return false;

            currentState?.Exit();
            currentState = state;
            Current = next;
            HasState = true;
            currentState.Enter();
            return true;
        }

        public void Tick(float deltaTime)
        {
            currentState?.Tick(deltaTime);
        }
    }
}
