using System;
using UnityEngine;

namespace EC.Data
{
    public enum PrizeKind { Gold, Gems, Consumable }

    [Serializable]
    public class PrizeEntry
    {
        public string id;
        public string displayName;
        public PrizeKind kind;
        public int amount;
        public string itemId;
        public int weight;
    }

    [CreateAssetMenu(menuName = "EC/Prize Table")]
    public class PrizeTable : ScriptableObject
    {
        public PrizeEntry[] entries;

        public int TotalWeight
        {
            get
            {
                var total = 0;
                if (entries == null)
                    return total;
                foreach (var entry in entries)
                    total += Mathf.Max(0, entry.weight);
                return total;
            }
        }

        public float Probability(int index)
        {
            var total = TotalWeight;
            return total <= 0 ? 0f : Mathf.Max(0, entries[index].weight) / (float)total;
        }
    }
}
