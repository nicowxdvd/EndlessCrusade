using System.Collections.Generic;
using EC.Data;

namespace EC.Services
{
    public class EquipmentService
    {
        readonly ISaveService save;
        readonly EquipmentCatalog catalog;

        public EquipmentService(ISaveService save, EquipmentCatalog catalog)
        {
            this.save = save;
            this.catalog = catalog;
        }

        EquipmentData Data => save.Current.equipment;

        public bool Owns(string id) { return Data.owned.Contains(id); }
        public bool IsEquipped(string id) { return Data.equipped.Contains(id); }

        public List<string> GrantChapterRewards(CampaignDefinition campaign)
        {
            var granted = new List<string>();
            if (campaign == null || campaign.chapters == null)
                return granted;

            var completed = save.Current.progress.completedLevels;
            foreach (var chapter in campaign.chapters)
            {
                if (chapter == null || chapter.rewardEquipmentIds == null || !TroopUnlockRules.IsChapterCompleted(chapter, completed))
                    continue;
                foreach (var id in chapter.rewardEquipmentIds)
                    if (Grant(id))
                        granted.Add(id);
            }
            if (granted.Count > 0)
                save.Save();
            return granted;
        }

        bool Grant(string id)
        {
            if (string.IsNullOrEmpty(id) || Owns(id))
                return false;
            Data.owned.Add(id);

            var item = catalog != null ? catalog.FindEquipment(id) : null;
            if (item == null)
            {
                Data.equipped.Add(id);
                return true;
            }
            if (!SlotInUse(item.slot))
                Data.equipped.Add(id);
            return true;
        }

        public bool Equip(string id)
        {
            var item = catalog != null ? catalog.FindEquipment(id) : null;
            if (item == null || !Owns(id))
                return false;
            for (int i = Data.equipped.Count - 1; i >= 0; i--)
            {
                var other = catalog.FindEquipment(Data.equipped[i]);
                if (other != null && other.slot == item.slot)
                    Data.equipped.RemoveAt(i);
            }
            Data.equipped.Add(id);
            save.Save();
            return true;
        }

        public bool Unequip(string id)
        {
            var removed = Data.equipped.Remove(id);
            if (removed)
                save.Save();
            return removed;
        }

        bool SlotInUse(EquipmentSlot slot)
        {
            foreach (var equippedId in Data.equipped)
            {
                var other = catalog.FindEquipment(equippedId);
                if (other != null && other.slot == slot)
                    return true;
            }
            return false;
        }
    }
}
