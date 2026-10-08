using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Equipment Catalog")]
    public class EquipmentCatalog : ScriptableObject
    {
        public EquipmentDefinition[] equipment;
        public MiracleDefinition[] miracles;

        public EquipmentDefinition FindEquipment(string id)
        {
            if (equipment == null)
                return null;
            foreach (var item in equipment)
                if (item != null && item.id == id)
                    return item;
            return null;
        }

        public MiracleDefinition FindMiracle(string id)
        {
            if (miracles == null)
                return null;
            foreach (var item in miracles)
                if (item != null && item.id == id)
                    return item;
            return null;
        }
    }
}
