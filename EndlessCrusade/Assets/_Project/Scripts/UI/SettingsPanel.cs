using EC.Services;
using UnityEngine;
using UnityEngine.UI;

namespace EC.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        public GameObject panel;
        public Slider musicSlider;
        public Slider sfxSlider;
        public TMPro.TMP_Text qualityLabel;

        public void Open()
        {
            var settings = SaveHost.Service.Current.settings;
            musicSlider.SetValueWithoutNotify(settings.musicVolume);
            sfxSlider.SetValueWithoutNotify(settings.sfxVolume);
            RefreshQuality();
            panel.SetActive(true);
        }

        public void Close()
        {
            panel.SetActive(false);
        }

        public static string QualityCaption(int qualityOverride)
        {
            switch (qualityOverride)
            {
                case 1: return "Calidad: Baja";
                case 2: return "Calidad: Media";
                case 3: return "Calidad: Alta";
                default: return "Calidad: Auto";
            }
        }

        public static int NextQuality(int qualityOverride)
        {
            return (qualityOverride + 1) % 4;
        }

        public void CycleQuality()
        {
            var next = NextQuality(SaveHost.Service.Current.settings.qualityOverride);
            QualityBootstrapper.SetOverride(next);
            RefreshQuality();
        }

        void RefreshQuality()
        {
            if (qualityLabel != null)
                qualityLabel.text = QualityCaption(SaveHost.Service.Current.settings.qualityOverride);
        }

        public void OnSliderChanged()
        {
            AudioSettingsService.Set(musicSlider.value, sfxSlider.value);
        }
    }
}
