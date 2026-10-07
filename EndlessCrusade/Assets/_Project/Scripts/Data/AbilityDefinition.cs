using UnityEngine;

namespace EC.Data
{
    public enum AbilityKind { ThrownArea, Buff, Heal }

    [CreateAssetMenu(menuName = "EC/Ability Definition")]
    public class AbilityDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public AbilityKind kind;
        public float cooldown = 12f;
        public int damage = 40;
        public float radius = 2.5f;
        public float undeadMultiplier = 2f;
        public float throwDistance = 6f;
        public Sprite icon;
        public GameObject projectilePrefab;
        public GameObject explosionPrefab;
    }
}
