using EC.Core;
using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Consumable Definition")]
    public class ConsumableDefinition : ScriptableObject
    {
        public const string HolyWaterPotionId = "holy_water_potion";
        public const string ReviveElixirId = "elixir_revive";

        public string id;
        public string displayName;
        public CurrencyType currency;
        public int cost;
        public int maxStack = 9;
    }
}
