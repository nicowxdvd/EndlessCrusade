using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public BaseDefinition baseDefinition;
        public GameObject environmentPrefab;
        public StorySequence intro;
        public StorySequence outro;
        public WaveDefinition[] waves;
        public bool troopsEnabled;
        public bool tutorial;
    }
}
