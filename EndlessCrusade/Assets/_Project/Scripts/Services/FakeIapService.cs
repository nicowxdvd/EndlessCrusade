using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EC.Services
{
    public class FakeIapService : IIapService
    {
        public bool IsAvailable { get; set; } = true;
        public bool CancelNext { get; set; }
        public int ConfirmedCount { get; private set; }

        public event Action<IapReceipt> ReceiptReceived;

        readonly Dictionary<string, string> prices = new Dictionary<string, string>();

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public void SetPrice(string productId, string price)
        {
            prices[productId] = price;
        }

        public string LocalizedPrice(string productId)
        {
            return prices.TryGetValue(productId, out var price) ? price : "—";
        }

        public Task<IapOutcome> PurchaseAsync(string productId)
        {
            if (!IsAvailable)
                return Task.FromResult(IapOutcome.Unavailable);
            if (CancelNext)
            {
                CancelNext = false;
                return Task.FromResult(IapOutcome.Cancelled);
            }

            Emit(productId, Guid.NewGuid().ToString("N"));
            return Task.FromResult(IapOutcome.Delivered);
        }

        public void Emit(string productId, string transactionId)
        {
            ReceiptReceived?.Invoke(new IapReceipt(productId, transactionId, () => ConfirmedCount++));
        }
    }
}
