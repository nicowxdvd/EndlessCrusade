using UnityEngine;

namespace EC.Gameplay
{
    public interface IDamageModifier
    {
        int Modify(int amount, GameObject source);
    }
}
