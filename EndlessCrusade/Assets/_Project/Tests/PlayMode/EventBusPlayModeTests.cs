using EC.Core;
using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EC.Tests.PlayMode
{
    public static class EventBusSeed
    {
        public static int Calls;

        public static void OnDied(EntityDied evt)
        {
            Calls++;
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void SubscribeInEditMode()
        {
            EventBus<EntityDied>.Subscribe(OnDied);
        }
#endif
    }

    public class EventBusPlayModeTests
    {
        [Test]
        public void SubscriberFromBeforePlayMode_IsClearedOnEnteringPlayMode()
        {
            EventBusSeed.Calls = 0;
            var go = new GameObject("Probe");

            EventBus<EntityDied>.Publish(new EntityDied(go));

            Object.DestroyImmediate(go);
            Assert.AreEqual(0, EventBusSeed.Calls);
        }
    }
}
