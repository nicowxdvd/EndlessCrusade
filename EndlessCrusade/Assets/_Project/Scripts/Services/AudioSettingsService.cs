using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public static class AudioSettingsService
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Apply()
        {
            Reapply();
        }

        public static void Reapply()
        {
            if (SaveHost.Service == null || AudioManager.Instance == null)
                return;
            var settings = SaveHost.Service.Current.settings;
            AudioManager.Instance.SetVolumes(settings.musicVolume, settings.sfxVolume);
        }

        public static void Set(float musicVolume, float sfxVolume)
        {
            if (SaveHost.Service == null)
                return;
            var settings = SaveHost.Service.Current.settings;
            settings.musicVolume = Mathf.Clamp01(musicVolume);
            settings.sfxVolume = Mathf.Clamp01(sfxVolume);
            SaveHost.Service.Save();
            Reapply();
        }
    }
}
