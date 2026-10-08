using System.Collections.Generic;
using System.Threading.Tasks;

namespace EC.Services
{
    public interface IAuthService
    {
        string UserId { get; }
        Task<bool> SignInAnonymouslyAsync();
    }

    public interface ICloudSaveService
    {
        Task<bool> SyncAsync();
    }

    public interface ICloudStore
    {
        Task<CloudDocument> GetAsync(string uid);
        Task SetAsync(string uid, string json, long updatedAtUtc);
    }

    public interface IAnalyticsService
    {
        void Log(string eventName, IDictionary<string, object> parameters = null);
    }

    public readonly struct CloudDocument
    {
        public readonly bool Exists;
        public readonly string Json;
        public readonly long UpdatedAtUtc;

        public CloudDocument(string json, long updatedAtUtc)
        {
            Exists = true;
            Json = json;
            UpdatedAtUtc = updatedAtUtc;
        }
    }
}
