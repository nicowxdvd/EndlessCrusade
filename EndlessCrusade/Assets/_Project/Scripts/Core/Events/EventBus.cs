using System;
using System.Collections.Generic;
using UnityEngine;

namespace EC.Core
{
    public static class EventBus<T>
    {
        static Action<T> handlers;

        static EventBus()
        {
            EventBusRegistry.Register(Clear);
        }

        public static void Subscribe(Action<T> handler)
        {
            handlers += handler;
        }

        public static void Unsubscribe(Action<T> handler)
        {
            handlers -= handler;
        }

        public static void Publish(T evt)
        {
            handlers?.Invoke(evt);
        }

        static void Clear()
        {
            handlers = null;
        }
    }

    public static class EventBusRegistry
    {
        static readonly List<Action> clearActions = new List<Action>();

        public static void Register(Action clear)
        {
            clearActions.Add(clear);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ClearAll()
        {
            foreach (var clear in clearActions)
                clear();
        }
    }
}
