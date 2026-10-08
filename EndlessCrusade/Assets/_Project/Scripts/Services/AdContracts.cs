using System.Threading.Tasks;

namespace EC.Services
{
    public enum AdPlacement { ReviveHero, DoubleGold, FreeGems }

    public interface IAdService
    {
        bool IsReady(AdPlacement placement);
        Task<bool> ShowRewardedAsync(AdPlacement placement);
    }
}
