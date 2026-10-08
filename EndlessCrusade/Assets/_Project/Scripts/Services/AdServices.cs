namespace EC.Services
{
    public static class AdServices
    {
        public static IAdService Service { get; private set; } = new FakeAdService();

        public static void Register(IAdService service)
        {
            Service = service;
        }
    }
}
