using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>
    /// Вешается на Gun (статичная пустышка).
    /// Крутит gun_pivot так, чтобы оружие смотрело в сторону прицеливания.
    /// Ничего не спавнит — только прицеливание/поворот.
    /// </summary>
    public sealed class GunAimController2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _gunSpriteRenderer;
        [SerializeField] private bool _flipSpriteOnLeft = true;

        public enum FlipMode { FlipX, FlipY }
        [SerializeField] private FlipMode _flipMode = FlipMode.FlipY;

        [Header("Refs")]
        [SerializeField] private Transform _owner; // игрок (центр вращения)
        [SerializeField] private Transform _gunPivot; // то, что крутим
        [SerializeField] private Transform _spawnPoint; // дуло

        [SerializeField] private MonoBehaviour _aimProviderBehaviour; // MouseAimProvider2D
        private IAimProvider2D _aim;

        [Header("Rotation")]
        [Tooltip("Какая ось gun_pivot должна смотреть на цель: Right или Up.")]
        [SerializeField] private AimAxis _pivotAimAxis = AimAxis.Right;

        [Tooltip("Если спрайт в исходной позе смотрит не туда, добавь корректировку угла.")]
        [SerializeField] private float _angleOffsetDeg = 0f;

        [Tooltip("Если включить, будет сглаживание поворота.")]
        [SerializeField] private bool _smooth = false;

        [SerializeField, Min(0f)] private float _smoothSpeed = 25f;

        private Vector2 _lastDir = Vector2.right;

        public Transform SpawnPoint => _spawnPoint;

        public enum AimAxis { Right, Up }

        private void Awake()
        {
            if (_owner == null) _owner = transform.root; // если Gun под игроком — обычно ок
            _aim = _aimProviderBehaviour as IAimProvider2D;

            if (_gunSpriteRenderer == null)
            {

            }
        }

        private void LateUpdate()
        {
            if (_owner == null || _gunPivot == null) return;

            if (_aim != null)
                _aim.TryGetAimDirection(_owner.position, out _lastDir);

            ApplyFlip(_lastDir);
            // угол направления (0° = вправо)
            float ang = Mathf.Atan2(_lastDir.y, _lastDir.x) * Mathf.Rad2Deg + _angleOffsetDeg;

            // если хотим, чтобы "вверх" смотрел в цель — сдвигаем на -90
            if (_pivotAimAxis == AimAxis.Up)
                ang -= 90f;

            Quaternion target = Quaternion.Euler(0f, 0f, ang);

            if (_smooth)
                _gunPivot.rotation = Quaternion.Slerp(_gunPivot.rotation, target, _smoothSpeed * Time.deltaTime);
            else
                _gunPivot.rotation = target;
        }

        private void ApplyFlip(Vector2 dir)
        {
            if (!_flipSpriteOnLeft || _gunSpriteRenderer == null) return;

            bool isLeft = dir.x < 0f;

            if (_flipMode == FlipMode.FlipX)
                _gunSpriteRenderer.flipX = isLeft;
            else
                _gunSpriteRenderer.flipY = isLeft;
        }
    }
}