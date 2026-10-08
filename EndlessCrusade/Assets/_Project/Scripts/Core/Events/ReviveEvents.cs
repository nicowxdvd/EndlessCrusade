using UnityEngine;

namespace EC.Core
{
    public readonly struct ReviveOffered
    {
        public readonly GameObject Hero;

        public ReviveOffered(GameObject hero) { Hero = hero; }
    }

    public readonly struct ReviveOfferClosed { }
}

namespace EC.Core
{
    public readonly struct ReviveOfferResponded
    {
        public readonly bool Accepted;

        public ReviveOfferResponded(bool accepted) { Accepted = accepted; }
    }
}
