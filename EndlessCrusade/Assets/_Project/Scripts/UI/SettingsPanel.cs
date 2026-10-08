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

        public void Open()
        {
            var settings = SaveHost.Service.Current.settings;
            musicSlider.SetValueWithoutNotify(settings.musicVolume);
            sfxSlider.SetValueWithoutNotify(settings.sfxVolume);
            panel.SetActive(true);
        }

        public void Close()
        {
            panel.SetActive(false);
        }

        public void OnSliderChanged()
        {
            AudioSettingsService.Set(musicSlider.value, sfxSlider.value);
        }
    }
}
