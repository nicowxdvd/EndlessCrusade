using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Campaign Definition")]
    public class CampaignDefinition : ScriptableObject
    {
        public ChapterDefinition[] chapters;
    }
}
