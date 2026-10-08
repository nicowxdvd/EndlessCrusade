using EC.Core;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class AbilityButton : MonoBehaviour
    {
        public int slot;
        public Button button;
        public Image cooldownFill;
        public TMPro.TMP_Text caption;

        void Start()
        {
            if (slot == 0)
                return;
            var miracles = EC.Data.HeroLoadoutHolder.Current.miracles;
            if (slot > miracles.Count)
            {
                gameObject.SetActive(false);
                return;
            }
            if (caption != null)
                caption.text = miracles[slot - 1].displayName;
        }

        void OnEnable()
        {
            EventBus<AbilityCooldownChanged>.Subscribe(OnCooldownChanged);
            if (button != null)
                button.onClick.AddListener(OnClick);
            Apply(0f);
        }

        void OnDisable()
        {
            EventBus<AbilityCooldownChanged>.Unsubscribe(OnCooldownChanged);
            if (button != null)
                button.onClick.RemoveListener(OnClick);
        }

        public void OnClick()
        {
            if (button != null && !button.interactable)
                return;
            EventBus<AbilityRequested>.Publish(new AbilityRequested(slot));
        }

        void OnCooldownChanged(AbilityCooldownChanged evt)
        {
            if (evt.Slot == slot)
                Apply(evt.Normalized);
        }

        void Apply(float normalized)
        {
            if (cooldownFill != null)
                cooldownFill.fillAmount = normalized;
            if (button != null)
                button.interactable = normalized <= 0f;
        }
    }
}
