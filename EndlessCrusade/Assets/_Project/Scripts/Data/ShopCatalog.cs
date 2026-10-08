using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Shop Catalog")]
    public class ShopCatalog : ScriptableObject
    {
        public UpgradeDefinition[] upgrades;
        public ConsumableDefinition[] consumables;
    }
}
