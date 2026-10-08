using EC.Data;
using UnityEngine;

namespace EC.Gameplay
{
    public class HeroVisualController : MonoBehaviour
    {
        public SpriteRenderer armorLayer;
        public SpriteRenderer shieldLayer;
        public SpriteRenderer weaponLayer;

        void Start()
        {
            Refresh(HeroLoadoutHolder.Current);
        }

        public void Refresh(HeroLoadout loadout)
        {
            SetLayer(armorLayer, loadout.armor);
            SetLayer(shieldLayer, loadout.shield);
            SetLayer(weaponLayer, loadout.heavyWeapon != null ? loadout.heavyWeapon : loadout.rangedWeapon);
        }

        static void SetLayer(SpriteRenderer layer, EquipmentDefinition item)
        {
            if (layer == null)
                return;
            var sprite = item != null && item.layerSprites != null && item.layerSprites.Length > 0 ? item.layerSprites[0] : null;
            layer.sprite = sprite;
            layer.enabled = sprite != null;
        }
    }
}
