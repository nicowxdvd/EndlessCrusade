using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Chapter Definition")]
    public class ChapterDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public LevelDefinition[] levels;
        public string[] rewardEquipmentIds;
        public Sprite mapArt;
    }
}
