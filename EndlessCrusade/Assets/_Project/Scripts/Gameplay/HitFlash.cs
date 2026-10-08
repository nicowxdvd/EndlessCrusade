using UnityEngine;

namespace EC.Gameplay
{
    [RequireComponent(typeof(HealthComponent))]
    public class HitFlash : MonoBehaviour
    {
        public float duration = 0.12f;

        HealthComponent health;
        SpriteRenderer[] renderers;
        Color[] original;
        float remaining;

        void Awake()
        {
            health = GetComponent<HealthComponent>();
        }

        void OnEnable()
        {
            health.Damaged += Flash;
            health.Died += Flash;
        }

        void OnDisable()
        {
            health.Damaged -= Flash;
            health.Died -= Flash;
            Restore();
        }

        void Flash()
        {
            if (original == null)
            {
                renderers = GetComponentsInChildren<SpriteRenderer>();
                original = new Color[renderers.Length];
                for (var i = 0; i < renderers.Length; i++)
                    original[i] = renderers[i].color;
            }
            remaining = duration;
            for (var i = 0; i < renderers.Length; i++)
                renderers[i].color = original[i].grayscale > 0.5f ? new Color(1f, 0.2f, 0.2f, original[i].a) : new Color(1f, 1f, 1f, original[i].a);
        }

        void Update()
        {
            if (original == null)
                return;
            remaining -= Time.deltaTime;
            if (remaining <= 0f)
                Restore();
        }

        void Restore()
        {
            if (original == null)
                return;
            for (var i = 0; i < renderers.Length; i++)
                if (renderers[i] != null)
                    renderers[i].color = original[i];
            original = null;
            remaining = 0f;
        }
    }
}
