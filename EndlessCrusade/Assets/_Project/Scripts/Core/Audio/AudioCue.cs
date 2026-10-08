using System;
using UnityEngine;

namespace EC.Core
{
    [CreateAssetMenu(menuName = "EC/Audio Cue")]
    public class AudioCue : ScriptableObject
    {
        public string id;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
        public bool loop;
        public float minInterval = 0.05f;

        [NonSerialized] public float lastPlayTime = float.NegativeInfinity;

        public bool HasClips => clips != null && clips.Length > 0;
    }
}
