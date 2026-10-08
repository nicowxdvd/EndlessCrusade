using System;
using System.Threading.Tasks;

namespace EC.Services
{
    public enum IapOutcome { Delivered, Cancelled, Failed, Unavailable }

    public readonly struct IapReceipt
    {
        public readonly string ProductId;
        public readonly string TransactionId;
        public readonly Action Confirm;

        public IapReceipt(string productId, string transactionId, Action confirm)
        {
            ProductId = productId;
            TransactionId = transactionId;
            Confirm = confirm;
        }
    }

    public interface IIapService
    {
        bool IsAvailable { get; }
        event Action<IapReceipt> ReceiptReceived;
        Task InitializeAsync();
        Task<IapOutcome> PurchaseAsync(string productId);
        string LocalizedPrice(string productId);
    }
}
