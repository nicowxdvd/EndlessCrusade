using EC.Core;
using EC.Data;
using UnityEngine;

namespace EC.Services
{
    public class CurrencyService
    {
        readonly ISaveService save;

        static CurrencyService instance;

        public static CurrencyService Instance
        {
            get
            {
                if (instance == null || instance.save != SaveHost.Service)
                    instance = new CurrencyService(SaveHost.Service);
                return instance;
            }
        }

        public CurrencyService(ISaveService save)
        {
            this.save = save;
        }

        public int Balance(CurrencyType type)
        {
            var profile = save.Current.profile;
            switch (type)
            {
                case CurrencyType.Gold: return profile.gold;
                case CurrencyType.Gems: return profile.gems;
                default: return profile.tickets;
            }
        }

        public void Add(CurrencyType type, int amount)
        {
            if (amount <= 0)
                return;
            Set(type, Balance(type) + amount);
        }

        public bool TrySpend(CurrencyType type, int amount)
        {
            if (amount < 0 || Balance(type) < amount)
                return false;
            if (amount > 0)
                Set(type, Balance(type) - amount);
            return true;
        }

        void Set(CurrencyType type, int value)
        {
            var profile = save.Current.profile;
            switch (type)
            {
                case CurrencyType.Gold: profile.gold = value; break;
                case CurrencyType.Gems: profile.gems = value; break;
                default: profile.tickets = value; break;
            }
            save.Save();
            EventBus<CurrencyChanged>.Publish(new CurrencyChanged(type, value));
        }
    }
}
