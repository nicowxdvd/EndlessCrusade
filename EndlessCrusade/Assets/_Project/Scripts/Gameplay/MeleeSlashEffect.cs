using UnityEngine;

namespace EC.Gameplay
{
    public class MeleeSlashEffect : MonoBehaviour
    {
        const float Lifetime = 0.15f;

        static Sprite whiteSprite;

        SpriteRenderer slash;
        Color baseColor;
        float elapsed;

        public static void Spawn(Vector3 origin, Vector3 target, bool whip)
        {
            if (whiteSprite == null)
            {
                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            }

            var go = new GameObject(whip ? "WhipSlash" : "SwordSlash");
            var length = Mathf.Max(Mathf.Abs(target.x - origin.x), 0.6f);
            go.transform.position = new Vector3((origin.x + target.x) * 0.5f, target.y, target.z);
            go.transform.localScale = whip ? new Vector3(length, 0.15f, 1f) : new Vector3(length, 0.7f, 1f);
            var effect = go.AddComponent<MeleeSlashEffect>();
            effect.baseColor = whip ? new Color(1f, 0.6f, 0.15f, 1f) : new Color(1f, 1f, 1f, 1f);
            effect.slash = go.AddComponent<SpriteRenderer>();
            effect.slash.sprite = whiteSprite;
            effect.slash.color = effect.baseColor;
            effect.slash.sortingOrder = 10;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / Lifetime);
            var color = baseColor;
            color.a = 1f - t;
            slash.color = color;
            if (t >= 1f)
                Destroy(gameObject);
        }
    }
}
