using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>
    /// Фастовый скрипт: при смерти выключает указанные модули (MoveModule/AttackModule/AI и т.д.).
    /// Дебаг-склейка, потом можно заменить DeathModule-ом.
    /// </summary>
    public sealed class DisableModulesOnDeath : MonoBehaviour
    {
        [SerializeField] private HealthModule _health;

        [Header("What to disable on death")]
        [SerializeField] private Behaviour[] _disableBehaviours;

        private void Awake()
        {
            if (_health == null) _health = GetComponentInParent<HealthModule>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.OnZeroHp += HandleZeroHp;
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnZeroHp -= HandleZeroHp;
        }

        private void HandleZeroHp(DamageInfo info)
        {
            if (_disableBehaviours == null) return;

            for (int i = 0; i < _disableBehaviours.Length; i++)
            {
                var b = _disableBehaviours[i];
                if (b != null) b.enabled = false;
            }
        }
    }
}
