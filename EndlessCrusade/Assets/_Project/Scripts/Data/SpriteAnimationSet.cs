using System;
using UnityEngine;

namespace EC.Data
{
    [Serializable]
    public class SpriteClip
    {
        public string id;
        public Sprite[] frames;
        public float framesPerSecond = 10f;
        public bool loop = true;
    }

    [CreateAssetMenu(menuName = "EC/Sprite Animation Set")]
    public class SpriteAnimationSet : ScriptableObject
    {
        public SpriteClip[] clips;

        public SpriteClip Find(string id)
        {
            if (clips == null)
                return null;
            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i].id == id)
                    return clips[i];
            }
            return null;
        }
    }
}
