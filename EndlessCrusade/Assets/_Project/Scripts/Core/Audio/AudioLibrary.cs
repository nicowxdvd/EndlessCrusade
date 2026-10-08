using System.Collections.Generic;
using UnityEngine;

namespace EC.Core
{
    [CreateAssetMenu(menuName = "EC/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        public AudioCue[] cues;

        Dictionary<string, AudioCue> byId;

        public AudioCue Find(string id)
        {
            if (string.IsNullOrEmpty(id) || cues == null)
                return null;
            if (byId == null)
            {
                byId = new Dictionary<string, AudioCue>();
                foreach (var cue in cues)
                    if (cue != null && !string.IsNullOrEmpty(cue.id))
                        byId[cue.id] = cue;
            }
            return byId.TryGetValue(id, out var found) ? found : null;
        }
    }
}
