using System;
using System.Collections.Generic;
using EC.Core;
using EC.Data;
using EC.Services;
using UnityEngine;

namespace EC.Gameplay
{
    public class ReviveService : MonoBehaviour
    {
        public const float ReviveHealthFraction = 0.5f;

        public static readonly List<Func<bool>> Sources = new List<Func<bool>>();

        bool used;
        GameObject pendingHero;
        Action onDeclined;

        public bool CanOfferAd => !used && pendingHero == null && SaveHost.Service != null && AdRewardService.Instance.IsReady(AdPlacement.ReviveHero);

        void OnEnable()
        {
            EventBus<ReviveOfferResponded>.Subscribe(OnResponded);
        }

        void OnDisable()
        {
            EventBus<ReviveOfferResponded>.Unsubscribe(OnResponded);
        }

        void OnResponded(ReviveOfferResponded evt)
        {
            if (evt.Accepted)
                AcceptAdOffer();
            else
                DeclineAdOffer();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSources()
        {
            Sources.Clear();
            Sources.Add(ConsumeElixir);
        }

        static bool ConsumeElixir()
        {
            return SaveHost.Service != null && UpgradeService.Instance.TryConsume(ConsumableDefinition.ReviveElixirId);
        }

        public void BeginAdOffer(GameObject hero, Action declined)
        {
            pendingHero = hero;
            onDeclined = declined;
            EventBus<ReviveOffered>.Publish(new ReviveOffered(hero));
        }

        public async void AcceptAdOffer()
        {
            var hero = pendingHero;
            if (hero == null)
                return;
            if (await AdRewardService.Instance.ShowAsync(AdPlacement.ReviveHero))
            {
                Resolve();
                used = true;
                Revive(hero);
                return;
            }
            DeclineAdOffer();
        }

        public void DeclineAdOffer()
        {
            if (pendingHero == null)
                return;
            var callback = onDeclined;
            Resolve();
            callback?.Invoke();
        }

        void Resolve()
        {
            pendingHero = null;
            onDeclined = null;
            EventBus<ReviveOfferClosed>.Publish(new ReviveOfferClosed());
        }

        void Revive(GameObject hero)
        {
            if (hero == null || !hero.TryGetComponent<EntityController>(out var controller))
                return;
            controller.ResetState();
            controller.Health.Revive(Mathf.RoundToInt(controller.Health.maxHealth * ReviveHealthFraction));
        }

        public bool TryRevive(GameObject hero)
        {
            if (used || hero == null)
                return false;
            if (!hero.TryGetComponent<EntityController>(out var controller) || controller.Health == null)
                return false;

            for (int i = 0; i < Sources.Count; i++)
            {
                if (!Sources[i]())
                    continue;
                used = true;
                Revive(hero);
                return true;
            }
            return false;
        }
    }
}
