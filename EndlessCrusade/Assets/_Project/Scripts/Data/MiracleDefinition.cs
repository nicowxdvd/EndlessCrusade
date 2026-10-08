using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Miracle Definition")]
    public class MiracleDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public AbilityDefinition ability;
    }
}
