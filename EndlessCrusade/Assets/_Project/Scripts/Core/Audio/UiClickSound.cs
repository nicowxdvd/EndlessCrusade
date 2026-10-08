using UnityEngine;
using UnityEngine.UI;

namespace EC.Core
{
    [RequireComponent(typeof(Button))]
    public class UiClickSound : MonoBehaviour
    {
        public string cueId = "ui_click";

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Play);
        }

        void Play()
        {
            EventBus<PlaySfxById>.Publish(new PlaySfxById(cueId));
        }
    }
}
