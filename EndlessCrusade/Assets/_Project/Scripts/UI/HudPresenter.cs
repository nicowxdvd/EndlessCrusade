using EC.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class HudPresenter : MonoBehaviour
    {
        public Image heroBar;
        public Image baseBar;
        public TMP_Text waveLabel;

        GameObject hero;

        void OnEnable()
        {
            EventBus<HeroSpawned>.Subscribe(OnHeroSpawned);
            EventBus<HealthChanged>.Subscribe(OnHealthChanged);
            EventBus<BaseResistanceChanged>.Subscribe(OnBaseResistanceChanged);
            EventBus<WaveStarted>.Subscribe(OnWaveStarted);
        }

        void OnDisable()
        {
            EventBus<HeroSpawned>.Unsubscribe(OnHeroSpawned);
            EventBus<HealthChanged>.Unsubscribe(OnHealthChanged);
            EventBus<BaseResistanceChanged>.Unsubscribe(OnBaseResistanceChanged);
            EventBus<WaveStarted>.Unsubscribe(OnWaveStarted);
        }

        void OnHeroSpawned(HeroSpawned evt)
        {
            hero = evt.Hero;
            SetFill(heroBar, 1f);
        }

        void OnHealthChanged(HealthChanged evt)
        {
            if (hero == null || evt.Source != hero)
                return;
            SetFill(heroBar, Ratio(evt.Current, evt.Max));
        }

        void OnBaseResistanceChanged(BaseResistanceChanged evt)
        {
            SetFill(baseBar, Ratio(evt.Current, evt.Max));
        }

        void OnWaveStarted(WaveStarted evt)
        {
            if (waveLabel != null)
                waveLabel.text = FormatWave(evt.Index, evt.Total);
        }

        public static float Ratio(int current, int max)
        {
            return max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
        }

        public static string FormatWave(int index, int total)
        {
            return "Oleada " + (index + 1) + " / " + total;
        }

        static void SetFill(Image bar, float value)
        {
            if (bar != null)
                bar.fillAmount = value;
        }
    }
}
