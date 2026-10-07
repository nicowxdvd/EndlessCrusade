using UnityEngine;

namespace EC.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rect;
        Rect applied;
        Vector2Int screenSize;

        void OnEnable()
        {
            rect = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != applied || screenSize.x != Screen.width || screenSize.y != Screen.height)
                Apply();
        }

        void Apply()
        {
            applied = Screen.safeArea;
            screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize.x == 0 || screenSize.y == 0)
                return;
            rect.anchorMin = new Vector2(applied.xMin / screenSize.x, applied.yMin / screenSize.y);
            rect.anchorMax = new Vector2(applied.xMax / screenSize.x, applied.yMax / screenSize.y);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
