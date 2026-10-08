using EC.Core;
using EC.Data;
using EC.Services;
using TMPro;
using UnityEngine;

namespace EC.UI
{
    public class EquipmentScreen : MonoBehaviour
    {
        public EquipmentCatalog catalog;
        public RectTransform listRoot;
        public GameObject rowTemplate;
        public TMP_Text emptyLabel;
        public string backScene = "CampaignMap";

        EquipmentService service;

        void Start()
        {
            service = new EquipmentService(SaveHost.Service, catalog);
            rowTemplate.SetActive(false);
            Refresh();
        }

        public void Back()
        {
            SceneFlow.Load(backScene);
        }

        public static string SlotName(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Armor: return "Armadura";
                case EquipmentSlot.Shield: return "Escudo";
                case EquipmentSlot.HeavyWeapon: return "Arma pesada";
                default: return "Arma a distancia";
            }
        }

        void Refresh()
        {
            for (int i = listRoot.childCount - 1; i >= 0; i--)
            {
                var child = listRoot.GetChild(i).gameObject;
                if (child != rowTemplate)
                    Destroy(child);
            }

            var shown = 0;
            foreach (var item in catalog.equipment)
            {
                if (item == null || !service.Owns(item.id))
                    continue;
                shown++;
                var equipped = service.IsEquipped(item.id);
                var instance = Instantiate(rowTemplate, listRoot);
                instance.SetActive(true);
                var row = instance.GetComponent<EmporiumRow>();
                row.nameLabel.text = item.displayName;
                row.infoLabel.text = SlotName(item.slot) + (equipped ? " (equipado)" : "");
                row.buyLabel.text = equipped ? "Desequipar" : "Equipar";
                var id = item.id;
                row.buyButton.onClick.AddListener(() =>
                {
                    if (service.IsEquipped(id)) service.Unequip(id); else service.Equip(id);
                    Refresh();
                });
            }
            emptyLabel.text = shown == 0 ? "Aún no has recuperado equipo" : "";
        }
    }
}
