using EC.Core;
using EC.Data;
using EC.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class ConsumableButton : MonoBehaviour
    {
        public Button button;
        public TMP_Text label;
        public string consumableId = ConsumableDefinition.HolyWaterPotionId;
        public string caption = "Poma";

        void Start()
        {
            button.onClick.AddListener(Use);
            Refresh();
        }

        public void Use()
        {
            if (SaveHost.Service == null || !UpgradeService.Instance.TryConsume(consumableId))
                return;
            EventBus<AbilityCooldownResetRequested>.Publish(new AbilityCooldownResetRequested());
            Refresh();
        }

        void Refresh()
        {
            var count = SaveHost.Service != null ? UpgradeService.Instance.GetCount(consumableId) : 0;
            label.text = caption + " x" + count;
            button.interactable = count > 0;
        }
    }
}
