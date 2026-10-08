using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/IAP Product")]
    public class IapProductDefinition : ScriptableObject
    {
        public string productId;
        public string displayName;
        public int gemsGranted;
    }
}
