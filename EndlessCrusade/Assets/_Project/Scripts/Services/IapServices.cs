using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public static class IapServices
    {
        public static IIapService Service { get; private set; } = new FakeIapService();

        static IapManager manager;
        static ShopCatalog bound;

        public static void Register(IIapService service)
        {
            Service = service;
            manager = null;
            bound = null;
        }

        public static void Bind(ShopCatalog catalog)
        {
            if (bound == catalog && manager != null)
                return;
            bound = catalog;
            manager = new IapManager(SaveHost.Service, CurrencyService.Instance, catalog.iapProducts, CloudServices.Analytics);
            Service.ReceiptReceived += receipt => manager.Handle(receipt);
            Service.InitializeAsync();
        }
    }
}
