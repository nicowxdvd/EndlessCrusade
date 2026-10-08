using UnityEngine;

namespace EC.Data
{
    [System.Serializable]
    public class StoryPanel
    {
        [TextArea] public string text;
        public Sprite image;
    }

    [CreateAssetMenu(menuName = "EC/Story Sequence")]
    public class StorySequence : ScriptableObject
    {
        public string id;
        public StoryPanel[] panels;
    }
}
