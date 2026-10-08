using EC.Core;
using UnityEngine;

namespace EC.UI
{
    public class ReviveOfferPanel : MonoBehaviour
    {
        public GameObject panel;

        void OnEnable()
        {
            EventBus<ReviveOffered>.Subscribe(OnOffered);
            EventBus<ReviveOfferClosed>.Subscribe(OnClosed);
        }

        void OnDisable()
        {
            EventBus<ReviveOffered>.Unsubscribe(OnOffered);
            EventBus<ReviveOfferClosed>.Unsubscribe(OnClosed);
        }

        void OnOffered(ReviveOffered evt)
        {
            panel.SetActive(true);
            Time.timeScale = 0f;
        }

        void OnClosed(ReviveOfferClosed evt)
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void Accept()
        {
            EventBus<ReviveOfferResponded>.Publish(new ReviveOfferResponded(true));
        }

        public void Decline()
        {
            EventBus<ReviveOfferResponded>.Publish(new ReviveOfferResponded(false));
        }
    }
}
