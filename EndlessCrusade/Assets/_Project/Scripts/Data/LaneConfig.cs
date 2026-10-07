using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Lane Config")]
    public class LaneConfig : ScriptableObject
    {
        public float laneHalfLength = 20f;
        public float baseX = -18f;
        public float spawnX = 18f;
        public float heroStartX = -12f;
        public float groundY = 0f;
        public float laneDepth = 6f;
        public float cameraPitch = 12f;
        public float cameraOrthoSize = 4.5f;
    }
}
