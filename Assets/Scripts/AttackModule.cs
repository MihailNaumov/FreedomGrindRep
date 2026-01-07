using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>
    /// AttackModule — управляет таймингом/формой атаки (кд, скорость, запуск).
    /// НЕ уменьшает HP напрямую. Он лишь создаёт "попытку урона",
    /// а цели принимают её через IDamageable (обычно HealthModule).
    /// </summary>

    /// <summary>Мини-ядро: таймер атаки и публичный TryAttack().</summary>
    public sealed class AttackModule : MonoBehaviour
    {
        public enum Mode { MeleeHitbox, Projectile }

        [SerializeField] private Mode _mode = Mode.MeleeHitbox;
        [SerializeField, Min(0.01f)] private float _cooldown = 0.5f;

        [SerializeField, Min(0f)] private float _baseDamage = 10f;
        [SerializeField] private DamageType _damageType = DamageType.Physical;

        [Header("Melee")]
        [SerializeField] private DamageHitbox2D _hitboxPrefab;
        [SerializeField] private Transform _hitboxSpawn;

        [Header("Projectile")]
        [SerializeField] private DamageProjectile2D _projectilePrefab;
        [SerializeField] private Transform _projectileSpawn;

        private float _nextAttackTime;

        public bool TryAttack(Vector2 dir)
        {
            if (Time.time < _nextAttackTime) return false;
            _nextAttackTime = Time.time + _cooldown;

            if (_mode == Mode.MeleeHitbox) FireHitbox();
            else FireProjectile(dir);

            return true;
        }
        private DamageInfo BuildInfo(GameObject source) => new DamageInfo
        {
            amount = _baseDamage,
            type = _damageType,
            instigator = gameObject,
            source = source,
            hitPoint = Vector2.zero
        };

        private void FireHitbox()
        {
            var hb = Instantiate(_hitboxPrefab, _hitboxSpawn.position, Quaternion.identity);
            hb.Arm(BuildInfo(hb.gameObject));
        }

        private void FireProjectile(Vector2 dir)
        {
            var pr = Instantiate(_projectilePrefab, _projectileSpawn.position, Quaternion.identity);
            pr.Arm(BuildInfo(pr.gameObject), dir);
        }
    }
}
