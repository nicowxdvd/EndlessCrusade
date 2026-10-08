using System.Threading.Tasks;
using UnityEngine;

namespace EC.Services
{
    public class FakeAdService : IAdService
    {
        public bool Ready { get; set; } = true;
        public bool Reward { get; set; } = true;
        public int Shown { get; private set; }

        public bool IsReady(AdPlacement placement)
        {
            return Ready;
        }

        public Task<bool> ShowRewardedAsync(AdPlacement placement)
        {
            if (!Ready)
                return Task.FromResult(false);
            Shown++;
            Debug.Log("[Ads] Anuncio simulado: " + placement);
            return Task.FromResult(Reward);
        }
    }
}
