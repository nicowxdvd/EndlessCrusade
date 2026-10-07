using TMPro;
using UnityEngine;

namespace EC.UI
{
    [CreateAssetMenu(menuName = "EC/UI Theme")]
    public class UiTheme : ScriptableObject
    {
        public Color background = new Color32(0x0B, 0x0B, 0x10, 0xFF);
        public Color blood = new Color32(0x8B, 0x00, 0x00, 0xFF);
        public Color gold = new Color32(0xC9, 0xA2, 0x4B, 0xFF);
        public Color text = new Color32(0xD8, 0xD8, 0xDC, 0xFF);
        public TMP_FontAsset titleFont;
        public TMP_FontAsset bodyFont;
    }
}
