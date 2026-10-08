using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Crashlytics;
using Firebase.Firestore;
using UnityEngine;

namespace EC.Services.Firebase
{
    public class FirebaseAuthService : IAuthService
    {
        public string UserId { get; private set; }

        public async Task<bool> SignInAnonymouslyAsync()
        {
            try
            {
                if (await FirebaseApp.CheckAndFixDependenciesAsync() != DependencyStatus.Available)
                    return false;
                var auth = FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser == null)
                    await auth.SignInAnonymouslyAsync();
                UserId = auth.CurrentUser != null ? auth.CurrentUser.UserId : null;
                return UserId != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Auth] Inicio de sesión anónimo fallido: " + e.Message);
                return false;
            }
        }
    }

    public class FirestoreCloudStore : ICloudStore
    {
        const string Collection = "users";

        public async Task<CloudDocument> GetAsync(string uid)
        {
            var snapshot = await FirebaseFirestore.DefaultInstance.Collection(Collection).Document(uid).GetSnapshotAsync();
            if (!snapshot.Exists)
                return default;
            snapshot.TryGetValue("save", out string json);
            snapshot.TryGetValue("updatedAtUtc", out long updatedAtUtc);
            return new CloudDocument(json, updatedAtUtc);
        }

        public Task SetAsync(string uid, string json, long updatedAtUtc)
        {
            var data = new Dictionary<string, object> { { "save", json }, { "updatedAtUtc", updatedAtUtc } };
            return FirebaseFirestore.DefaultInstance.Collection(Collection).Document(uid).SetAsync(data);
        }
    }

    public class FirebaseAnalyticsService : IAnalyticsService
    {
        public void Log(string eventName, IDictionary<string, object> parameters = null)
        {
            if (parameters == null)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var list = new List<Parameter>();
            foreach (var pair in parameters)
            {
                if (pair.Value is int i) list.Add(new Parameter(pair.Key, i));
                else if (pair.Value is long l) list.Add(new Parameter(pair.Key, l));
                else if (pair.Value is double d) list.Add(new Parameter(pair.Key, d));
                else list.Add(new Parameter(pair.Key, Convert.ToString(pair.Value)));
            }
            FirebaseAnalytics.LogEvent(eventName, list.ToArray());
        }
    }

    public static class FirebaseInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            CloudServices.Register(new FirebaseAuthService(), new FirestoreCloudStore(), new FirebaseAnalyticsService());
#if UNITY_ANDROID && !UNITY_EDITOR
            Crashlytics.ReportUncaughtExceptionsAsFatal = true;
#endif
        }
    }
}
