using EC.Core;
using EC.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EC.UI
{
    public class HeroCommandButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public HeroCommandKind kind;
        public EquipmentSlot requiredSlot;

        bool held;

        void Start()
        {
            gameObject.SetActive(HeroLoadoutHolder.Current.Has(requiredSlot));
        }

        void OnDisable()
        {
            Release();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            held = true;
            EventBus<HeroCommand>.Publish(new HeroCommand(kind, true));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        void Release()
        {
            if (!held)
                return;
            held = false;
            EventBus<HeroCommand>.Publish(new HeroCommand(kind, false));
        }
    }
}
