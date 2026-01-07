using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>Временный хитбокс: при входе в trigger применяет DamageInfo.</summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class DamageHitbox2D : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;

        private DamageInfo _info;
        private bool _armed;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void Arm(DamageInfo info)
        {
            _info = info;
            _armed = true;
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
