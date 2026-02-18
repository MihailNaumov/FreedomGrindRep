using System.Collections.Generic;
using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> Ближний урон, окно удара, анти-мультихит (hitbox / projectile)</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageHitbox2D : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;

        private DamageInfo _info;
        private bool _armed;

        private GameObject _owner;
        private Transform _follow;
        private float _dieTime;
        private bool _destroyOnFirstHit;

        private readonly HashSet<int> _hitTargets = new(); // instanceID целей

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Init(DamageInfo info, GameObject owner, Transform follow, float lifetime, bool destroyOnFirstHit)
        {
            _info = info;
            _owner = owner;
            _follow = follow;
            _destroyOnFirstHit = destroyOnFirstHit;

            _dieTime = Time.time + Mathf.Max(0.01f, lifetime);
            _armed = true;
        }

        private void Update()
        {
            if (!_armed) return;

            if (_follow != null)
                transform.position = _follow.position;

            if (Time.time >= _dieTime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_armed) return;
            if ((_targetMask.value & (1 << other.gameObject.layer)) == 0) return;

            // не бьём владельца (и его детей)
            if (_owner != null && other.GetComponentInParent<Transform>() == _owner.transform) return;

            var dmg = other.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) return;

            int id = other.GetInstanceID();
            if (_hitTargets.Contains(id)) return; // анти-мультихит
            _hitTargets.Add(id);

            var info = _info;
            info.hitPoint = other.ClosestPoint(transform.position);
            dmg.TryApplyDamage(info);

            if (_destroyOnFirstHit)
                Destroy(gameObject);
        }
    }
}
