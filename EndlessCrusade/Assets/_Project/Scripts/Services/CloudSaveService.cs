using System;
using System.IO;
using System.Threading.Tasks;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public class CloudSaveService : ICloudSaveService
    {
        public const string ConflictFileName = "save.conflict.json";

        readonly IAuthService auth;
        readonly ICloudStore store;
        readonly ISaveService save;
        readonly string conflictDirectory;

        public CloudSaveService(IAuthService auth, ICloudStore store, ISaveService save, string conflictDirectory)
        {
            this.auth = auth;
            this.store = store;
            this.save = save;
            this.conflictDirectory = conflictDirectory;
        }

        public async Task<bool> SyncAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(auth.UserId) && !await auth.SignInAnonymouslyAsync())
                    return false;

                var uid = auth.UserId;
                var cloud = await store.GetAsync(uid);
                var local = save.Current;
                var localJson = JsonUtility.ToJson(local);

                if (!cloud.Exists)
                {
                    await store.SetAsync(uid, localJson, local.updatedAtUtc);
                    return true;
                }

                if (cloud.UpdatedAtUtc > local.updatedAtUtc)
                {
                    var restorer = save as ISaveRestorer;
                    if (restorer == null)
                        return false;
                    var data = SaveMigrator.Migrate(cloud.Json);
                    WriteConflict(localJson);
                    restorer.Restore(data);
                }
                else if (cloud.UpdatedAtUtc < local.updatedAtUtc)
                {
                    WriteConflict(cloud.Json);
                    await store.SetAsync(uid, localJson, local.updatedAtUtc);
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Cloud] Sincronización fallida: " + e.Message);
                return false;
            }
        }

        void WriteConflict(string json)
        {
            try
            {
                Directory.CreateDirectory(conflictDirectory);
                File.WriteAllText(Path.Combine(conflictDirectory, ConflictFileName), json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Cloud] No se pudo guardar el conflicto: " + e.Message);
            }
        }
    }
}
