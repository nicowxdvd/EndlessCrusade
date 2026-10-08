using EC.Core;
using UnityEngine;

namespace EC.UI
{
    public class MainMenu : MonoBehaviour
    {
        public string mapScene = "CampaignMap";

        public void Play()
        {
            SceneFlow.Load(mapScene);
        }
    }
}
