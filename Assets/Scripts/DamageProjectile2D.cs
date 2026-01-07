using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>Простой trigger-снаряд: летит, при входе в цель наносит урон.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageProjectile2D : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField, Min(0.1f)] private float _speed = 10f;
        [SerializeField, Min(0.1f)] private float _lifeTime = 3f;

        private DamageInfo _info;
        private Vector2 _dir = Vector2.right;
        private float _dieTime;
        private bool _armed;

        private void Awake() => GetComponent<Collider2D>().isTrigger = true;

        public void Arm(DamageInfo info, Vector2 dir)
        {
            _info = info;
            _dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            _dieTime = Time.time + _lifeTime;
            _armed = true;
        }

        private void Update()
        {
            if (!_armed) return;
            transform.position += (Vector3)(_dir * _speed * Time.deltaTime);
            if (Time.time >= _dieTime) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_armed) return;
            if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;

            var dmg = other.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) return;

            var info = _info;
            info.hitPoint = other.ClosestPoint(transform.position);
            dmg.TryApplyDamage(info);

            Destroy(gameObject);
        }
    }
}
