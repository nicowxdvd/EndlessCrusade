using System.Collections.Generic;
using EC.Core;
using UnityEngine;

namespace EC.Gameplay
{
    public static class TargetFinder
    {
        struct Entry
        {
            public IDamageable Target;
            public Transform Transform;
        }

        static readonly List<Entry> entries = new List<Entry>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            entries.Clear();
        }

        public static void Register(IDamageable target, Transform transform)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Target == target)
                    return;
            }
            entries.Add(new Entry { Target = target, Transform = transform });
        }

        public static void Unregister(IDamageable target)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i].Target == target)
                {
                    entries.RemoveAt(i);
                    return;
                }
            }
        }

        public static IDamageable FindNearest(Vector3 from, Team enemyOf, float maxDistance)
        {
            IDamageable best = null;
            var bestDistance = float.MaxValue;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Transform == null || entry.Target.Team == enemyOf || !entry.Target.IsAlive)
                    continue;
                var distance = Mathf.Abs(entry.Transform.position.x - from.x);
                if (distance > maxDistance || distance >= bestDistance)
                    continue;
                bestDistance = distance;
                best = entry.Target;
            }
            return best;
        }

        public static int Collect(Vector3 center, float radius, Team enemyOf, List<IDamageable> results)
        {
            results.Clear();
            var sqrRadius = radius * radius;
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry.Transform == null || entry.Target.Team == enemyOf || !entry.Target.IsAlive)
                    continue;
                var dx = entry.Transform.position.x - center.x;
                var dy = entry.Transform.position.y - center.y;
                if (dx * dx + dy * dy > sqrRadius)
                    continue;
                results.Add(entry.Target);
            }
            return results.Count;
        }
    }
}
