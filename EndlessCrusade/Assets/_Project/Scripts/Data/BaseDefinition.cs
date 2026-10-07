using UnityEngine;

namespace EC.Data
{
    [CreateAssetMenu(menuName = "EC/Base Definition")]
    public class BaseDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public int maxResistance = 500;
        public GameObject prefab;
        public Sprite[] damageStages;

        void OnValidate()
        {
            if (damageStages == null || damageStages.Length != 3)
                Debug.LogWarning($"[Base] '{name}' debe tener exactamente 3 sprites en damageStages", this);
        }
    }
}
