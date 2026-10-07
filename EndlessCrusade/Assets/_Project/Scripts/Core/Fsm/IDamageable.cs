using UnityEngine;

namespace EC.Core
{
    public interface IDamageable
    {
        Team Team { get; }
        bool IsAlive { get; }
        void TakeDamage(int amount, GameObject source);
    }
}
