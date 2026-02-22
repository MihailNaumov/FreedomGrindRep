using System.Collections.Generic;
using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> ƒальний урон, Rigidbody2D, pierce, мир-коллизи€ </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class DamageProjectile2D : MonoBehaviour
    {
        public enum FacingAxis { Right, Up }
        [SerializeField] private FacingAxis _facingAxis = FacingAxis.Right;
        [SerializeField] private bool _rotateOnInit = true;

        [Header("Masks")]
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _worldMask;

        private DamageInfo _info;
        private GameObject _owner;

        private Rigidbody2D _rb;
        private Vector2 _dir;
        private float _dieTime;

        private int _pierce;
        private int _hitsDone;

        private readonly HashSet<int> _hitTargets = new();

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        public void Init(DamageInfo info, GameObject owner, Vector2 dir, float speed, float lifetime, int pierce)
        {
            _info = info;
            _owner = owner;

            _dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            if (_rotateOnInit)
            {
                if (_facingAxis == FacingAxis.Right) transform.right = _dir;
                else transform.up = _dir;
            }

            _rb.velocity = _dir * Mathf.Max(0.1f, speed);

            _dieTime = Time.time + Mathf.Max(0.1f, lifetime);
            _pierce = Mathf.Max(0, pierce);
            _hitsDone = 0;
        }

        private void Update()
        {
            if (Time.time >= _dieTime)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            int layerBit = 1 << other.gameObject.layer;

            // не врезаемс€ в владельца (и его детей)
            if (_owner != null && other.GetComponentInParent<Transform>() == _owner.transform)
                return;

            // мир/стены
            if ((_worldMask.value & layerBit) != 0)
            {
                Destroy(gameObject);
                return;
            }

            // не цель
            if ((_targetMask.value & layerBit) == 0)
                return;

            var dmg = other.GetComponentInParent<IDamageable>();
            if (dmg == null || !dmg.IsAlive) return;

            int id = other.GetInstanceID();
            if (_hitTargets.Contains(id)) return;
            _hitTargets.Add(id);

            var info = _info;
            info.hitPoint = other.ClosestPoint(transform.position);
            dmg.TryApplyDamage(info);

            _hitsDone++;
            if (_hitsDone > _pierce)
                Destroy(gameObject);
        }
    }
}
