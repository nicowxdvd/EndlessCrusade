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
            AnchorsFor(applied, screenSize.x, screenSize.y, out var min, out var max);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void AnchorsFor(Rect safeArea, int width, int height, out Vector2 min, out Vector2 max)
        {
            min = new Vector2(safeArea.xMin / width, safeArea.yMin / height);
            max = new Vector2(safeArea.xMax / width, safeArea.yMax / height);
        }
    }
}
