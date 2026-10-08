namespace EC.Services
{
    public static class CloudServices
    {
        public static IAuthService Auth { get; private set; } = new FakeAuthService();
        public static ICloudStore Store { get; private set; } = new FakeCloudStore();
        public static IAnalyticsService Analytics { get; private set; } = new LogAnalyticsService();

        public static void Register(IAuthService auth, ICloudStore store, IAnalyticsService analytics)
        {
            Auth = auth;
            Store = store;
            Analytics = analytics;
        }
    }
}
