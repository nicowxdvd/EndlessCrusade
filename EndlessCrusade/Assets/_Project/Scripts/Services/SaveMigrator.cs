using System;
using System.Collections.Generic;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public static class SaveMigrator
    {
        [Serializable]
        class VersionProbe
        {
            public int version;
        }

        static readonly List<Func<SaveData, SaveData>> Steps = new List<Func<SaveData, SaveData>>
        {
            MigrateV0ToV1
        };

        public static int ReadVersion(string json)
        {
            return JsonUtility.FromJson<VersionProbe>(json).version;
        }

        public static SaveData Migrate(string json)
        {
            int from = ReadVersion(json);
            if (from < 0 || from > SaveData.CurrentVersion)
                throw new InvalidOperationException("Versión de guardado no soportada: " + from);

            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) throw new InvalidOperationException("Guardado ilegible");

            for (int v = from; v < SaveData.CurrentVersion; v++)
            {
                data = Steps[v](data);
                data.version = v + 1;
            }
            return Normalize(data);
        }

        static SaveData MigrateV0ToV1(SaveData data)
        {
            return data;
        }

        static SaveData Normalize(SaveData d)
        {
            if (string.IsNullOrEmpty(d.playerId)) d.playerId = Guid.NewGuid().ToString();
            if (d.profile == null) d.profile = new ProfileData();
            if (d.profile.processedTransactionIds == null) d.profile.processedTransactionIds = new List<string>();
            if (d.progress == null) d.progress = new ProgressData();
            if (d.progress.completedLevels == null) d.progress.completedLevels = new List<string>();
            if (d.upgrades == null) d.upgrades = new List<UpgradeLevel>();
            if (d.consumables == null) d.consumables = new List<ItemCount>();
            if (d.equipment == null) d.equipment = new EquipmentData();
            if (d.equipment.owned == null) d.equipment.owned = new List<string>();
            if (d.equipment.equipped == null) d.equipment.equipped = new List<string>();
            if (d.settings == null) d.settings = new SettingsData();
            return d;
        }
    }
}
