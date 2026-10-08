using System;
using EC.Core;
using EC.Data;
using EC.Services;
using TMPro;
using UnityEngine;

namespace EC.UI
{
    public class EmporiumScreen : MonoBehaviour
    {
        enum Tab { Hero, Troop, Base, Consumables, Gems }

        public ShopCatalog catalog;
        public RectTransform listRoot;
        public GameObject rowTemplate;
        public TMP_Text balanceLabel;
        public TMP_Text messageLabel;
        public string backScene = "CampaignMap";

        Tab tab = Tab.Hero;

        void Start()
        {
            rowTemplate.SetActive(false);
            IapServices.Bind(catalog);
            Refresh();
        }

        public void ShowHero() { Show(Tab.Hero); }
        public void ShowTroops() { Show(Tab.Troop); }
        public void ShowBase() { Show(Tab.Base); }
        public void ShowConsumables() { Show(Tab.Consumables); }
        public void ShowGems() { Show(Tab.Gems); }

        public void Back()
        {
            SceneFlow.Load(backScene);
        }

        void Show(Tab next)
        {
            tab = next;
            messageLabel.text = "";
            Refresh();
        }

        public static string CurrencyName(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Gold: return "Oro";
                case CurrencyType.Gems: return "Reliquias";
                default: return "Boletos";
            }
        }

        public static string PurchaseMessage(PurchaseResult result)
        {
            switch (result)
            {
                case PurchaseResult.MaxLevel: return "Ya está al máximo";
                case PurchaseResult.NotEnoughFunds: return "Fondos insuficientes";
                default: return "";
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

            var currency = CurrencyService.Instance;
            balanceLabel.text = "Oro " + currency.Balance(CurrencyType.Gold) + "   Reliquias " + currency.Balance(CurrencyType.Gems);

            if (tab == Tab.Gems)
            {
                foreach (var product in catalog.iapProducts)
                    AddProductRow(product);
                return;
            }

            if (tab == Tab.Consumables)
            {
                foreach (var consumable in catalog.consumables)
                    AddConsumableRow(consumable);
                return;
            }

            var category = (UpgradeCategory)(int)tab;
            foreach (var upgrade in catalog.upgrades)
                if (upgrade != null && upgrade.category == category)
                    AddUpgradeRow(upgrade);
        }

        void AddUpgradeRow(UpgradeDefinition upgrade)
        {
            var service = UpgradeService.Instance;
            var level = service.GetLevel(upgrade.id);
            var row = NewRow(upgrade.displayName, "Nivel " + level + " / " + upgrade.maxLevel);
            var maxed = level >= upgrade.maxLevel;
            row.buyLabel.text = maxed ? "Máximo" : service.NextCost(upgrade) + " " + CurrencyName(upgrade.currency);
            row.buyButton.interactable = !maxed;
            row.buyButton.onClick.AddListener(() => Buy(() => service.TryBuy(upgrade)));
        }

        void AddConsumableRow(ConsumableDefinition consumable)
        {
            var service = UpgradeService.Instance;
            var count = service.GetCount(consumable.id);
            var row = NewRow(consumable.displayName, "Tienes " + count + " / " + consumable.maxStack);
            var full = count >= consumable.maxStack;
            row.buyLabel.text = full ? "Lleno" : consumable.cost + " " + CurrencyName(consumable.currency);
            row.buyButton.interactable = !full;
            row.buyButton.onClick.AddListener(() => Buy(() => service.TryBuyConsumable(consumable)));
        }

        void AddProductRow(IapProductDefinition product)
        {
            var iap = IapServices.Service;
            var row = NewRow(product.displayName, product.gemsGranted + " reliquias");
            row.buyLabel.text = iap.IsAvailable ? iap.LocalizedPrice(product.productId) : "Sin conexión";
            row.buyButton.interactable = iap.IsAvailable;
            row.buyButton.onClick.AddListener(() => BuyProduct(product));
        }

        async void BuyProduct(IapProductDefinition product)
        {
            var outcome = await IapServices.Service.PurchaseAsync(product.productId);
            Refresh();
            messageLabel.text = IapMessage(outcome);
        }

        public static string IapMessage(IapOutcome outcome)
        {
            switch (outcome)
            {
                case IapOutcome.Cancelled: return "Compra cancelada";
                case IapOutcome.Failed: return "La compra falló";
                case IapOutcome.Unavailable: return "Tienda no disponible";
                default: return "";
            }
        }

        EmporiumRow NewRow(string title, string info)
        {
            var instance = Instantiate(rowTemplate, listRoot);
            instance.SetActive(true);
            var row = instance.GetComponent<EmporiumRow>();
            row.nameLabel.text = title;
            row.infoLabel.text = info;
            return row;
        }

        void Buy(Func<PurchaseResult> purchase)
        {
            var result = purchase();
            Refresh();
            messageLabel.text = PurchaseMessage(result);
        }
    }
}
