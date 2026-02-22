using UnityEngine;

namespace FreedomGrind.Combat
{
    [CreateAssetMenu(menuName = "FreedomGrind/Combat/Weapon Profile")]
    public sealed class WeaponProfile : ScriptableObject
    {

        [Header("Weapon Name")]
        public string weaponName;

        [Header("Projectile Prefab")]
        public DamageProjectile2D projectilePrefab;

        [Header("Damage")]
        public DamageType damageType = DamageType.Physical;
        [Min(0f)] public float baseDamage = 10f;

        [Header("Rate")]
        [Min(0.01f)] public float cooldown = 0.5f;

        [Header("Projectile Params")]
        [Min(0.1f)] public float speed = 10f;
        [Min(0.05f)] public float lifetime = 2f;
        [Min(0)] public int pierce = 0;
    }
}