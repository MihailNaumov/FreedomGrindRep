using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> Прицел в сторону мышки </summary>
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
                return false; // dir остаётся вправо

            Vector3 mp = Input.mousePosition;

            // ScreenToWorldPoint ждёт z = расстояние от камеры до плоскости, где находится origin
            float zDist = originWorld.z - _camera.transform.position.z;
            mp.z = zDist;

            Vector3 world = _camera.ScreenToWorldPoint(mp);

            Vector2 d = (Vector2)(world - originWorld);
            if (d.sqrMagnitude < 0.0001f)
                return false;

            dir = d.normalized;
            return true;
        }
    }
}