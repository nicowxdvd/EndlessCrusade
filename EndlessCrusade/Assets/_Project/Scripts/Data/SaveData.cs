using System;
using System.Collections.Generic;

namespace EC.Data
{
    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public string playerId;
        public long updatedAtUtc;
        public ProfileData profile = new ProfileData();
        public ProgressData progress = new ProgressData();
        public List<UpgradeLevel> upgrades = new List<UpgradeLevel>();
        public List<ItemCount> consumables = new List<ItemCount>();
        public EquipmentData equipment = new EquipmentData();
        public SettingsData settings = new SettingsData();
    }

    [Serializable]
    public class ProfileData
    {
        public int gold;
        public int gems;
        public int tickets;
        public long lastDailyClaimUtc;
        public List<string> processedTransactionIds = new List<string>();
        public AdLimitsData ads = new AdLimitsData();
    }

    [Serializable]
    public class ProgressData
    {
        public List<string> completedLevels = new List<string>();
    }

    [Serializable]
    public class UpgradeLevel
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class ItemCount
    {
        public string id;
        public int count;
    }

    [Serializable]
    public class EquipmentData
    {
        public List<string> owned = new List<string>();
        public List<string> equipped = new List<string>();
    }

    [Serializable]
    public class SettingsData
    {
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
    }

    [Serializable]
    public class AdLimitsData
    {
        public long lastFreeGemsUtc;
        public int freeGemsToday;
        public long dayStartUtc;
    }
}
