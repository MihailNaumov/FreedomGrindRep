using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>«аглушка цели: принимает урон и пишет лог.</summary>
    public sealed class DummyDamageable : MonoBehaviour, IDamageable
    {
        [SerializeField] private bool _isAlive = true;

        public bool IsAlive => _isAlive;

        public bool TryApplyDamage(DamageInfo info)
        {
            Debug.Log($"[DummyDamageable] took {info.amount} {info.type} from '{info.instigator.name}'", this);
            return true;
        }
    }
}
