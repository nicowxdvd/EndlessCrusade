using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace EC.Services
{
    public class FakeAuthService : IAuthService
    {
        public string UserId { get; private set; }

        public Task<bool> SignInAnonymouslyAsync()
        {
            UserId = "fake-user";
            return Task.FromResult(true);
        }
    }

    public class FakeCloudStore : ICloudStore
    {
        readonly Dictionary<string, CloudDocument> documents = new Dictionary<string, CloudDocument>();

        public bool Fail { get; set; }

        public Task<CloudDocument> GetAsync(string uid)
        {
            if (Fail)
                return Task.FromException<CloudDocument>(new System.InvalidOperationException("sin red"));
            documents.TryGetValue(uid, out var document);
            return Task.FromResult(document);
        }

        public Task SetAsync(string uid, string json, long updatedAtUtc)
        {
            if (Fail)
                return Task.FromException(new System.InvalidOperationException("sin red"));
            documents[uid] = new CloudDocument(json, updatedAtUtc);
            return Task.CompletedTask;
        }
    }

    public class LogAnalyticsService : IAnalyticsService
    {
        public void Log(string eventName, IDictionary<string, object> parameters = null)
        {
            var detail = "";
            if (parameters != null)
                foreach (var pair in parameters)
                    detail += " " + pair.Key + "=" + pair.Value;
            Debug.Log("[Analytics] " + eventName + detail);
        }
    }
}
