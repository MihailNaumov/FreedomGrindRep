using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> Решает когда и чем атакуем (hitbox / projectile)</summary>
    public sealed class AttackModule : MonoBehaviour
    {
        public enum Mode { MeleeHitbox, Projectile }

        [Header("Core")]
        [SerializeField] private Mode _mode = Mode.MeleeHitbox;
        [SerializeField, Min(0.01f)] private float _cooldown = 0.5f;
        [SerializeField] private bool _attackEnabled = true;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float _baseDamage = 10f;
        [SerializeField] private DamageType _damageType = DamageType.Physical;

        [Header("Melee Hitbox")]
        [SerializeField] private DamageHitbox2D _hitboxPrefab;
        [SerializeField] private Transform _hitboxSpawn;
        [SerializeField, Min(0.01f)] private float _hitboxLifetime = 0.12f;
        [SerializeField] private bool _hitboxDestroyOnFirstHit = false;
        [SerializeField] private bool _hitboxFollowSpawn = true;

        [Header("Projectile")]
        [SerializeField] private DamageProjectile2D _projectilePrefab;
        [SerializeField] private Transform _projectileSpawn;
        [SerializeField, Min(0.1f)] private float _projectileSpeed = 12f;
        [SerializeField, Min(0.1f)] private float _projectileLifetime = 3f;
        [SerializeField, Min(0)] private int _projectilePierce = 0;

        private float _nextAttackTime;

        public void SetAttackEnabled(bool enabled) => _attackEnabled = enabled;

        public bool TryAttack(Vector2 dir)
        {
            if (!_attackEnabled) return false;
            if (Time.time < _nextAttackTime) return false;
            _nextAttackTime = Time.time + _cooldown;

            if (_mode == Mode.MeleeHitbox) FireHitbox(dir);
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

        private void FireHitbox(Vector2 dir)
        {
            if (_hitboxPrefab == null || _hitboxSpawn == null) return;

            var hb = Instantiate(_hitboxPrefab, _hitboxSpawn.position, Quaternion.identity);
            hb.Init(
                info: BuildInfo(hb.gameObject),
                owner: gameObject,
                follow: _hitboxFollowSpawn ? _hitboxSpawn : null,
                lifetime: _hitboxLifetime,
                destroyOnFirstHit: _hitboxDestroyOnFirstHit
            );
        }

        private void FireProjectile(Vector2 dir)
        {
            if (_projectilePrefab == null || _projectileSpawn == null) return;

            var pr = Instantiate(_projectilePrefab, _projectileSpawn.position, Quaternion.identity);
            pr.Init(
                info: BuildInfo(pr.gameObject),
                owner: gameObject,
                dir: dir,
                speed: _projectileSpeed,
                lifetime: _projectileLifetime,
                pierce: _projectilePierce
            );
        }
    }
}
