using System;
using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>
    /// HealthModule Ч единственна€ точка правды про HP.
    /// ”рон принимаетс€ через IDamageable.TryApplyDamage().
    /// ¬изуал/звук/дроп Ч через событи€ (подписчики снаружи).
    /// </summary>
    public sealed class HealthModule : MonoBehaviour, IDamageable
    {
        [Header("HP")]
        [SerializeField, Min(1f)] private float _maxHp = 100f;
        [SerializeField] private bool _spawnFullHp = true;
        [SerializeField, Min(0f)] private float _startHp = 100f;

        [Header("Damage")]
        [SerializeField, Min(0f)] private float _invulnAfterHit = 0.05f;

        // по индексам DamageType (Physical/Earth/Water/...)
        
        [System.Serializable]
        public struct DamageResistancePercent
        {
            [Header("Damage Type Resistance (%)")]

            public float Physical;
            public float Earth;
            public float Water;
            public float Fire;
            public float Air;
        }

        [SerializeField]
        private DamageResistancePercent _typeResistancePercent;

        private float _invulnUntil;

        private float _hp;
        private bool _isAlive = true;

        public float MaxHp => _maxHp;
        public float CurrentHp => _hp;
        public bool IsAlive => _isAlive;

        /// <summary>oldHp, newHp</summary>
        public event Action<float, float> OnHpChanged;

        /// <summary> info, newHp</summary>
        public event Action<DamageInfo, float> OnDamaged; // попытка нанести урон / контакт

        public event Action<DamageInfo, float> OnBlocked; // блок при попытке урона

        /// <summary>healAmount, newHp</summary>
        public event Action<float, float> OnHealed;

        /// <summary>последний урон, который добил</summary>
        public event Action<DamageInfo> OnZeroHp;

        private void Awake()
        {
            _maxHp = Mathf.Max(1f, _maxHp);

            _hp = _spawnFullHp
                ? _maxHp
                : Mathf.Clamp(_startHp, 0f, _maxHp);

            _isAlive = _hp > 0f;
        }
        private bool ChangeHp(float delta, DamageInfo? cause, out float oldHp, out float newHp)
        {
            oldHp = _hp;
            newHp = Mathf.Clamp(_hp + delta, 0f, _maxHp);

            if (Mathf.Approximately(oldHp, newHp))
                return false;

            _hp = newHp;
            OnHpChanged?.Invoke(oldHp, newHp);

            if (_hp <= 0f && _isAlive)
            {
                _isAlive = false;
                OnZeroHp?.Invoke(cause ?? default);
            }

            return true;
        }
        private float GetResistancePercent(DamageType type)
        {
            return type switch
            {
                DamageType.Physical => _typeResistancePercent.Physical,
                DamageType.Earth => _typeResistancePercent.Earth,
                DamageType.Water => _typeResistancePercent.Water,
                DamageType.Fire => _typeResistancePercent.Fire,
                DamageType.Air => _typeResistancePercent.Air,
                _ => 0f
            };
        }

        /// <summary> 
        /// !!! ¬ Ѕ”ƒ”ў≈ћ ѕ–»ƒ≈“—я  –”“»“№ —ёƒј отдельный УDamageResolverФ класс при попадании ¬ ў»“, »ћћ”Ќ, » ƒ–”√»≈ ¬ј–»јЌ“џ !!!
        ///</summary>
        public bool TryApplyDamage(DamageInfo info) // удар обработан
        {
            if (!_isAlive) return false;
            if (_invulnAfterHit > 0f && Time.time < _invulnUntil) return false;

            if (info.amount <= 0f) return false;
            if (float.IsNaN(info.amount) || float.IsInfinity(info.amount)) return false;

            float resistancePct = GetResistancePercent(info.type);   // резист/у€звимость в процентах
            float resistance = resistancePct / 100f;          // перевод в долю
            float dmg = info.amount * (1f - resistance);

            if (float.IsNaN(dmg) || float.IsInfinity(dmg)) return false;



            if (dmg < 0f)
            {
                _invulnUntil = Time.time + _invulnAfterHit;

                OnDamaged?.Invoke(info, _hp);
                Heal(-dmg);// хил (healAmount > 0)
                return true;
            }

            if (Mathf.Approximately(dmg, 0f))
            {
                _invulnUntil = Time.time + _invulnAfterHit;
                OnDamaged?.Invoke(info, _hp);          // удар был
                OnBlocked?.Invoke(info, _hp);          // отдельный ивент "0 урона" (если надо)
                return true;
            }

            if (!ChangeHp(-dmg, info, out _, out float newHp))
            return false;

            _invulnUntil = Time.time + _invulnAfterHit;

            OnDamaged?.Invoke(info, newHp);
            return true;
        }

        private static float Sanitize(float v)
        {
            if (float.IsNaN(v) || float.IsInfinity(v))
                return 0f;

            return v;
        }

        private void OnValidate()
        {
            _typeResistancePercent.Physical = Sanitize(_typeResistancePercent.Physical);
            _typeResistancePercent.Earth = Sanitize(_typeResistancePercent.Earth);
            _typeResistancePercent.Water = Sanitize(_typeResistancePercent.Water);
            _typeResistancePercent.Fire = Sanitize(_typeResistancePercent.Fire);
            _typeResistancePercent.Air = Sanitize(_typeResistancePercent.Air);
        }
        public bool Heal(float amount)
        {
            if (!_isAlive) return false;
            if (amount <= 0f) return false;
            if (float.IsNaN(amount) || float.IsInfinity(amount)) return false;

            if (!ChangeHp(+amount, null, out _, out float newHp))
                return false;

            OnHealed?.Invoke(amount, newHp);
            return true;
        }

        public void ReviveFull()
        {
            _isAlive = true;
            _invulnUntil = 0f;
            ChangeHp(_maxHp - _hp, null, out _, out _);
        }

        public void SetMaxHp(float newMax, bool keepPercent = true)
        {
            newMax = Mathf.Max(1f, newMax);

            float percent = (_maxHp > 0f) ? (_hp / _maxHp) : 1f;
            _maxHp = newMax;

            float target = keepPercent ? (_maxHp * percent) : Mathf.Min(_hp, _maxHp);
            ChangeHp(target - _hp, null, out _, out _);
        }
    }
}
