using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>
    /// Единый роутер инпута:
    /// - движение
    /// - атака
    /// Можно вешать на любой объект.
    /// Работает по принципу "если модуль есть — используем".
    /// </summary>
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private MoveModule _moveModule;

        [Header("Attack")]
        [SerializeField] private AttackModule _attackModule;
        [SerializeField] private Transform _attackOrigin;
        [SerializeField] private MonoBehaviour _aimProviderBehaviour; // IAimProvider2D

        [Header("Attack Input")]
        [SerializeField] private int _attackMouseButton = 0; // ЛКМ
        [SerializeField] private bool _holdToAttack = true;  // как в Vampire Survivors

        private IAimProvider2D _aimProvider;
        private Vector2 _currentMoveDir;

        private void Awake()
        {
            // movement
            if (_moveModule == null)
                _moveModule = GetComponent<MoveModule>();

            // attack
            if (_attackModule == null)
                _attackModule = GetComponent<AttackModule>();

            if (_attackOrigin == null)
                _attackOrigin = transform;

            _aimProvider = _aimProviderBehaviour as IAimProvider2D;

            if (_moveModule == null)
                Debug.LogWarning($"[PlayerInputRouter] MoveModule не найден на {name}");

            if (_attackModule == null)
                Debug.LogWarning($"[PlayerInputRouter] AttackModule не найден на {name}");

            if (_aimProviderBehaviour != null && _aimProvider == null)
                Debug.LogWarning($"[PlayerInputRouter] {_aimProviderBehaviour.name} не реализует IAimProvider2D");
        }

        private void Update()
        {
            HandleMovement();
            HandleAttack();
        }

        // =========================
        // MOVEMENT
        // =========================
        private void HandleMovement()
        {
            if (_moveModule == null || !_moveModule.enabled)
                return;

            Vector2 newDir = Vector2.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) newDir.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) newDir.y -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) newDir.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) newDir.x += 1;

            if (newDir != _currentMoveDir)
            {
                _currentMoveDir = newDir;
                _moveModule.SetDirection(_currentMoveDir);
            }
        }

        // =========================
        // ATTACK
        // =========================
        private void HandleAttack()
        {
            if (_attackModule == null || !_attackModule.enabled)
                return;

            bool pressed = _holdToAttack
                ? Input.GetMouseButton(_attackMouseButton)
                : Input.GetMouseButtonDown(_attackMouseButton);

            if (!pressed)
                return;

            Vector2 dir = Vector2.right;

            if (_aimProvider != null)
            {
                _aimProvider.TryGetAimDirection(_attackOrigin.position, out dir);
                _attackModule.TryAttack(dir);
            }
             

        }
    }
}