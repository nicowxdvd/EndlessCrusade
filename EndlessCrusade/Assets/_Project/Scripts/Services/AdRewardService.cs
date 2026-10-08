using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EC.Core;
using EC.Data;

namespace EC.Services
{
    public class AdRewardService
    {
        readonly ISaveService save;
        readonly CurrencyService currency;
        readonly IAdService ads;
        readonly IAnalyticsService analytics;
        readonly Func<long> clock;

        static AdRewardService instance;

        public static AdRewardService Instance
        {
            get
            {
                if (instance == null || instance.save != SaveHost.Service || instance.ads != AdServices.Service)
                    instance = new AdRewardService(SaveHost.Service, CurrencyService.Instance, AdServices.Service, CloudServices.Analytics, () => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                return instance;
            }
        }

        public AdRewardService(ISaveService save, CurrencyService currency, IAdService ads, IAnalyticsService analytics, Func<long> clock)
        {
            this.save = save;
            this.currency = currency;
            this.ads = ads;
            this.analytics = analytics;
            this.clock = clock;
        }

        public bool IsReady(AdPlacement placement)
        {
            return ads.IsReady(placement);
        }

        public async Task<bool> ShowAsync(AdPlacement placement)
        {
            if (!ads.IsReady(placement) || !await ads.ShowRewardedAsync(placement))
                return false;
            analytics?.Log("ad_reward", new Dictionary<string, object> { { "placement", placement.ToString() } });
            return true;
        }

        public bool CanClaimFreeGems()
        {
            return ads.IsReady(AdPlacement.FreeGems) && AdLimitRules.CanClaimFreeGems(save.Current.profile.ads, clock());
        }

        public long MillisUntilFreeGems()
        {
            return AdLimitRules.MillisUntilFreeGems(save.Current.profile.ads, clock());
        }

        public async Task<bool> ClaimFreeGemsAsync()
        {
            if (!CanClaimFreeGems() || !await ShowAsync(AdPlacement.FreeGems))
                return false;
            AdLimitRules.RegisterFreeGems(save.Current.profile.ads, clock());
            currency.Add(CurrencyType.Gems, AdLimitRules.FreeGemsAmount);
            save.Save();
            return true;
        }

        public async Task<bool> DoubleGoldAsync(int runGold)
        {
            if (runGold <= 0 || !await ShowAsync(AdPlacement.DoubleGold))
                return false;
            currency.Add(CurrencyType.Gold, runGold);
            return true;
        }
    }
}
