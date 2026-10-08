using EC.Core;
using EC.Data;

namespace EC.Services
{
    public enum PurchaseResult { Success, MaxLevel, NotEnoughFunds }

    public class UpgradeService
    {
        readonly ISaveService save;
        readonly CurrencyService currency;

        static UpgradeService instance;

        public static UpgradeService Instance
        {
            get
            {
                if (instance == null || instance.save != SaveHost.Service)
                    instance = new UpgradeService(SaveHost.Service, CurrencyService.Instance);
                return instance;
            }
        }

        public UpgradeService(ISaveService save, CurrencyService currency)
        {
            this.save = save;
            this.currency = currency;
        }

        public int GetLevel(string id)
        {
            foreach (var entry in save.Current.upgrades)
                if (entry.id == id)
                    return entry.level;
            return 0;
        }

        public int NextCost(UpgradeDefinition definition)
        {
            return definition.CostAtLevel(GetLevel(definition.id));
        }

        public PurchaseResult TryBuy(UpgradeDefinition definition)
        {
            var level = GetLevel(definition.id);
            if (level >= definition.maxLevel)
                return PurchaseResult.MaxLevel;
            if (!currency.TrySpend(definition.currency, definition.CostAtLevel(level)))
                return PurchaseResult.NotEnoughFunds;

            SetLevel(definition.id, level + 1);
            save.Save();
            EventBus<UpgradePurchased>.Publish(new UpgradePurchased(definition.id, level + 1));
            return PurchaseResult.Success;
        }

        public int GetCount(string id)
        {
            foreach (var entry in save.Current.consumables)
                if (entry.id == id)
                    return entry.count;
            return 0;
        }

        public PurchaseResult TryBuyConsumable(ConsumableDefinition definition)
        {
            var count = GetCount(definition.id);
            if (count >= definition.maxStack)
                return PurchaseResult.MaxLevel;
            if (!currency.TrySpend(definition.currency, definition.cost))
                return PurchaseResult.NotEnoughFunds;

            SetCount(definition.id, count + 1);
            save.Save();
            return PurchaseResult.Success;
        }

        public void GrantConsumable(string id, int amount)
        {
            if (amount <= 0)
                return;
            SetCount(id, GetCount(id) + amount);
            save.Save();
        }

        public bool TryConsume(string id)
        {
            var count = GetCount(id);
            if (count <= 0)
                return false;
            SetCount(id, count - 1);
            save.Save();
            return true;
        }

        void SetLevel(string id, int level)
        {
            foreach (var entry in save.Current.upgrades)
            {
                if (entry.id != id)
                    continue;
                entry.level = level;
                return;
            }
            save.Current.upgrades.Add(new UpgradeLevel { id = id, level = level });
        }

        void SetCount(string id, int count)
        {
            foreach (var entry in save.Current.consumables)
            {
                if (entry.id != id)
                    continue;
                entry.count = count;
                return;
            }
            save.Current.consumables.Add(new ItemCount { id = id, count = count });
        }
    }
}
