using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Troop Definition")]
    public class TroopDefinition : UnitDefinition
    {
        public int leadershipCost = 30;
        public float summonCooldown = 6f;
        public string unlockChapterId;
    }
}
