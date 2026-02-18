using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> ѕрицел в сторону мышки </summary>
    public sealed class MouseAimProvider2D : MonoBehaviour, IAimProvider2D
    {
        [SerializeField] private Camera _camera;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        public bool TryGetAimDirection(Vector3 originWorld, out Vector2 dir)
        {
            dir = Vector2.right;

            if (_camera == null)
                return false; // не смогли прицелитьс€, но dir остаЄтс€ вправо

            Vector3 world = _camera.ScreenToWorldPoint(Input.mousePosition);
            world.z = originWorld.z;

            Vector2 d = (Vector2)(world - originWorld);
            if (d.sqrMagnitude < 0.0001f)
                return false; // мышь почти в origin, тоже считаем "не прицелилс€"

            dir = d.normalized;
            return true;
        }
    }
}