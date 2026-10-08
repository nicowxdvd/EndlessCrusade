using System.Collections.Generic;
using EC.Core;
using EC.Data;

namespace EC.Services
{
    public class IapManager
    {
        readonly ISaveService save;
        readonly CurrencyService currency;
        readonly IEnumerable<IapProductDefinition> products;
        readonly IAnalyticsService analytics;

        public IapManager(ISaveService save, CurrencyService currency, IEnumerable<IapProductDefinition> products, IAnalyticsService analytics)
        {
            this.save = save;
            this.currency = currency;
            this.products = products;
            this.analytics = analytics;
        }

        public bool Handle(IapReceipt receipt)
        {
            var processed = save.Current.profile.processedTransactionIds;
            if (processed.Contains(receipt.TransactionId))
            {
                receipt.Confirm?.Invoke();
                return false;
            }

            var product = Find(receipt.ProductId);
            if (product == null)
                return false;

            processed.Add(receipt.TransactionId);
            currency.Add(CurrencyType.Gems, product.gemsGranted);
            save.Save();
            receipt.Confirm?.Invoke();
            analytics?.Log("iap_purchase", new Dictionary<string, object> { { "product_id", product.productId }, { "gems", product.gemsGranted } });
            return true;
        }

        IapProductDefinition Find(string productId)
        {
            foreach (var product in products)
                if (product != null && product.productId == productId)
                    return product;
            return null;
        }
    }
}
