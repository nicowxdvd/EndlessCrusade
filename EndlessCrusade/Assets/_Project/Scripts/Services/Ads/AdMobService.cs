using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using UnityEngine;

namespace EC.Services.Ads
{
    public class AdMobService : IAdService
    {
        const string TestRewardedUnit = "ca-app-pub-3940256099942544/5224354917";

        readonly Dictionary<AdPlacement, RewardedAd> loaded = new Dictionary<AdPlacement, RewardedAd>();
        readonly string unitId;
        bool initialized;

        public AdMobService(string unitId = TestRewardedUnit)
        {
            this.unitId = unitId;
        }

        public void Initialize()
        {
            var parameters = new ConsentRequestParameters();
            ConsentInformation.Update(parameters, updateError =>
            {
                if (updateError != null)
                {
                    Debug.LogWarning("[Ads] Consentimiento: " + updateError.Message);
                    return;
                }
                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null)
                        Debug.LogWarning("[Ads] Formulario: " + formError.Message);
                    if (ConsentInformation.CanRequestAds())
                        StartAds();
                });
            });
            if (ConsentInformation.CanRequestAds())
                StartAds();
        }

        void StartAds()
        {
            if (initialized)
                return;
            initialized = true;
            MobileAds.Initialize(status =>
            {
                foreach (AdPlacement placement in System.Enum.GetValues(typeof(AdPlacement)))
                    Preload(placement);
            });
        }

        void Preload(AdPlacement placement)
        {
            RewardedAd.Load(unitId, new AdRequest(), (ad, error) =>
            {
                if (error != null || ad == null)
                    return;
                loaded[placement] = ad;
            });
        }

        public bool IsReady(AdPlacement placement)
        {
            return loaded.TryGetValue(placement, out var ad) && ad.CanShowAd();
        }

        public Task<bool> ShowRewardedAsync(AdPlacement placement)
        {
            if (!loaded.TryGetValue(placement, out var ad) || !ad.CanShowAd())
                return Task.FromResult(false);

            var source = new TaskCompletionSource<bool>();
            var rewarded = false;
            ad.OnAdFullScreenContentClosed += () =>
            {
                loaded.Remove(placement);
                Preload(placement);
                source.TrySetResult(rewarded);
            };
            ad.OnAdFullScreenContentFailed += error => source.TrySetResult(false);
            ad.Show(reward => rewarded = true);
            return source.Task;
        }
    }

    public static class AdInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            var service = new AdMobService();
            AdServices.Register(service);
            service.Initialize();
        }
    }
}
