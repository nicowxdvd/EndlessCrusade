using System;
using EC.Core;
using EC.Data;

namespace EC.Services
{
    public class PachinkoService
    {
        public const long DayMs = 24L * 60 * 60 * 1000;

        readonly ISaveService save;
        readonly CurrencyService currency;
        readonly UpgradeService upgrades;
        readonly Random random;
        readonly Func<long> clock;

        public PachinkoService(ISaveService save, CurrencyService currency, UpgradeService upgrades, Random random, Func<long> clock)
        {
            this.save = save;
            this.currency = currency;
            this.upgrades = upgrades;
            this.random = random;
            this.clock = clock;
        }

        public int Tickets => currency.Balance(CurrencyType.Tickets);

        public static int Draw(PrizeTable table, Random random)
        {
            var total = table.TotalWeight;
            if (total <= 0)
                return -1;

            var roll = random.Next(total);
            for (int i = 0; i < table.entries.Length; i++)
            {
                roll -= Math.Max(0, table.entries[i].weight);
                if (roll < 0)
                    return i;
            }
            return table.entries.Length - 1;
        }

        public int TryPlay(PrizeTable table)
        {
            var index = Draw(table, random);
            if (index < 0 || !currency.TrySpend(CurrencyType.Tickets, 1))
                return -1;

            Deliver(table.entries[index]);
            return index;
        }

        public bool CanClaimDaily()
        {
            return DayIndex(clock()) > DayIndex(save.Current.profile.lastDailyClaimUtc) || save.Current.profile.lastDailyClaimUtc == 0;
        }

        public bool ClaimDaily()
        {
            if (!CanClaimDaily())
                return false;
            save.Current.profile.lastDailyClaimUtc = clock();
            currency.Add(CurrencyType.Tickets, 1);
            return true;
        }

        static long DayIndex(long utcMillis)
        {
            return utcMillis / DayMs;
        }

        void Deliver(PrizeEntry entry)
        {
            switch (entry.kind)
            {
                case PrizeKind.Gold: currency.Add(CurrencyType.Gold, entry.amount); break;
                case PrizeKind.Gems: currency.Add(CurrencyType.Gems, entry.amount); break;
                default: upgrades.GrantConsumable(entry.itemId, Math.Max(1, entry.amount)); break;
            }
        }
    }
}
