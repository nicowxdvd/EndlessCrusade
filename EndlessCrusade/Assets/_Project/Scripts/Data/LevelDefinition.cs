using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public BaseDefinition baseDefinition;
        public WaveDefinition[] waves;
        public bool troopsEnabled;
    }
}
