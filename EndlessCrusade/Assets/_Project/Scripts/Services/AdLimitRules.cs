using EC.Data;

namespace EC.Services
{
    public static class AdLimitRules
    {
        public const long FreeGemsIntervalMs = 4L * 60 * 60 * 1000;
        public const long DayMs = 24L * 60 * 60 * 1000;
        public const int FreeGemsAmount = 5;
        public const int MaxFreeGemsClaimsPerDay = 5;

        public static void RollDay(AdLimitsData ads, long nowUtc)
        {
            if (ads.dayStartUtc == 0 || nowUtc - ads.dayStartUtc >= DayMs)
            {
                ads.dayStartUtc = nowUtc;
                ads.freeGemsToday = 0;
            }
        }

        public static bool CanClaimFreeGems(AdLimitsData ads, long nowUtc)
        {
            RollDay(ads, nowUtc);
            if (ads.freeGemsToday >= MaxFreeGemsClaimsPerDay)
                return false;
            return ads.lastFreeGemsUtc == 0 || nowUtc - ads.lastFreeGemsUtc >= FreeGemsIntervalMs;
        }

        public static long MillisUntilFreeGems(AdLimitsData ads, long nowUtc)
        {
            RollDay(ads, nowUtc);
            if (ads.freeGemsToday >= MaxFreeGemsClaimsPerDay)
                return ads.dayStartUtc + DayMs - nowUtc;
            if (ads.lastFreeGemsUtc == 0)
                return 0;
            var remaining = ads.lastFreeGemsUtc + FreeGemsIntervalMs - nowUtc;
            return remaining > 0 ? remaining : 0;
        }

        public static void RegisterFreeGems(AdLimitsData ads, long nowUtc)
        {
            RollDay(ads, nowUtc);
            ads.freeGemsToday++;
            ads.lastFreeGemsUtc = nowUtc;
        }

        public static string FormatCountdown(long millis)
        {
            var totalSeconds = (int)(millis / 1000);
            return (totalSeconds / 3600) + ":" + ((totalSeconds % 3600) / 60).ToString("00") + ":" + (totalSeconds % 60).ToString("00");
        }
    }
}
