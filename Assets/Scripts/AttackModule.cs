using Unity.VisualScripting;
using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> Решает когда и чем атакуем (hitbox / projectile)</summary>
    public sealed class AttackModule : MonoBehaviour
    {
        public enum Mode { MeleeHitbox, Projectile }

        [SerializeField] private WeaponProfile _weapon;

        [SerializeField] private Transform _weaponRoot; // FireStaff в иерархии
        private Transform _spawnPoint;

        [Header("Core")]
        [SerializeField] private Mode _mode = Mode.MeleeHitbox;
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
        [SerializeField, Min(0.1f)] private float _projectileSpeed = 12f;
        [SerializeField, Min(0.1f)] private float _projectileLifetime = 3f;
        [SerializeField, Min(0)] private int _projectilePierce = 0;

        private float _nextAttackTime;

        private void Start()
        {
            Equip(_weapon);
        }

        public void SetAttackEnabled(bool enabled) => _attackEnabled = enabled;

        public bool TryAttack(Vector2 dir)
        {
            if (!_attackEnabled) return false;
            if (Time.time < _nextAttackTime) return false;
            _nextAttackTime = Time.time + _weapon.cooldown;

            if (_spawnPoint == null)
            {
                Debug.LogWarning("[AttackModule] Missing projectile spawn.", this);
                return false;
            }


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
            if (_projectilePrefab == null || _spawnPoint == null) return;

            var proj = Instantiate(_weapon.projectilePrefab, _spawnPoint.position, Quaternion.identity);

            // Собираем DamageInfo
            var info = BuildInfo(source: proj.gameObject);
            info.amount = _weapon.baseDamage;
            info.type = _weapon.damageType;

            proj.Init(
                owner: gameObject,
                info: info,
                dir: dir,
                speed: _weapon.speed,
                lifetime: _weapon.lifetime,
                pierce: _weapon.pierce
            );
        }

        public void Equip(WeaponProfile weapon)
        {
            _weapon = weapon;
            _spawnPoint = GetComponentInChildren<GunAimController2D>().SpawnPoint; // заглушка для спавнпоинта
          
        }
    }

}
