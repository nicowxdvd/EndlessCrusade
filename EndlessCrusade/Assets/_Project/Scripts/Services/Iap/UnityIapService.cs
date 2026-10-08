using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace EC.Services.Iap
{
    public class UnityIapService : IIapService
    {
        readonly string[] productIds;
        readonly Dictionary<string, Product> catalog = new Dictionary<string, Product>();
        readonly Dictionary<string, TaskCompletionSource<IapOutcome>> waiting = new Dictionary<string, TaskCompletionSource<IapOutcome>>();

        StoreController store;
        bool connected;

        public event Action<IapReceipt> ReceiptReceived;

        public bool IsAvailable => connected && Application.internetReachability != NetworkReachability.NotReachable;

        public UnityIapService(params string[] productIds)
        {
            this.productIds = productIds;
        }

        public async Task InitializeAsync()
        {
            try
            {
                store = UnityIAPServices.StoreController();
                store.OnProductsFetched += OnProductsFetched;
                store.OnPurchasePending += OnPurchasePending;
                store.OnPurchaseFailed += OnPurchaseFailed;
                await store.Connect();
                connected = true;

                var definitions = new List<ProductDefinition>();
                foreach (var id in productIds)
                    definitions.Add(new ProductDefinition(id, ProductType.Consumable));
                store.FetchProducts(definitions);
                store.FetchPurchases();
            }
            catch (Exception e)
            {
                connected = false;
                Debug.LogWarning("[IAP] No se pudo inicializar: " + e.Message);
            }
        }

        public string LocalizedPrice(string productId)
        {
            return catalog.TryGetValue(productId, out var product) ? product.metadata.localizedPriceString : "—";
        }

        public Task<IapOutcome> PurchaseAsync(string productId)
        {
            if (!IsAvailable || !catalog.TryGetValue(productId, out var product))
                return Task.FromResult(IapOutcome.Unavailable);

            var source = new TaskCompletionSource<IapOutcome>();
            waiting[productId] = source;
            store.PurchaseProduct(product);
            return source.Task;
        }

        void OnProductsFetched(List<Product> products)
        {
            foreach (var product in products)
                catalog[product.definition.id] = product;
        }

        void OnPurchasePending(PendingOrder order)
        {
            var productId = order.CartOrdered.Items()[0].Product.definition.id;
            var transactionId = order.Info.TransactionID;
            ReceiptReceived?.Invoke(new IapReceipt(productId, transactionId, () => store.ConfirmPurchase(order)));
            if (waiting.TryGetValue(productId, out var source))
            {
                waiting.Remove(productId);
                source.TrySetResult(IapOutcome.Delivered);
            }
        }

        void OnPurchaseFailed(FailedOrder order)
        {
            var productId = order.CartOrdered.Items()[0].Product.definition.id;
            if (!waiting.TryGetValue(productId, out var source))
                return;
            waiting.Remove(productId);
            source.TrySetResult(order.FailureReason == PurchaseFailureReason.UserCancelled ? IapOutcome.Cancelled : IapOutcome.Failed);
        }
    }

    public static class IapInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            IapServices.Register(new UnityIapService("gems_pack_small", "gems_pack_medium", "gems_pack_large"));
        }
    }
}
