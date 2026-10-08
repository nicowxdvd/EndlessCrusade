using System;
using System.IO;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public class JsonSaveService : ISaveService
    {
        public const string FileName = "save.json";

        readonly string _path;
        readonly string _tmpPath;
        readonly string _bakPath;

        public SaveData Current { get; private set; }

        public JsonSaveService() : this(Application.persistentDataPath) { }

        public JsonSaveService(string directory)
        {
            _path = Path.Combine(directory, FileName);
            _tmpPath = _path + ".tmp";
            _bakPath = _path + ".bak";
        }

        public void Load()
        {
            Current = TryRead(_path, "save.json") ?? TryRead(_bakPath, "save.json.bak");
            if (Current != null) return;

            if (File.Exists(_path) || File.Exists(_bakPath))
                Debug.LogWarning("[Save] Guardado y respaldo inválidos, se crea uno nuevo");
            Current = CreateNew();
        }

        public void Save()
        {
            if (Current == null) Current = CreateNew();
            Current.updatedAtUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            try
            {
                string dir = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                File.WriteAllText(_tmpPath, JsonUtility.ToJson(Current));

                if (File.Exists(_path))
                {
                    if (IsReadable(_path)) File.Copy(_path, _bakPath, true);
                    File.Delete(_path);
                }
                File.Move(_tmpPath, _path);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] No se pudo guardar: " + e.Message);
            }
        }

        public void Reset()
        {
            Current = CreateNew();
            Save();
        }

        static SaveData CreateNew()
        {
            return new SaveData { playerId = Guid.NewGuid().ToString() };
        }

        static bool IsReadable(string path)
        {
            try { SaveMigrator.Migrate(File.ReadAllText(path)); return true; }
            catch { return false; }
        }

        static SaveData TryRead(string path, string label)
        {
            if (!File.Exists(path)) return null;
            try
            {
                return SaveMigrator.Migrate(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] " + label + " inválido: " + e.Message);
                return null;
            }
        }
    }
}
