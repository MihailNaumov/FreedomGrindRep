using UnityEngine;

namespace FreedomGrind.Combat
{
    public sealed class NearestEnemyAimProvider2D : MonoBehaviour, IAimProvider2D
    {
        [SerializeField] private LayerMask _enemyMask;
        [SerializeField, Min(0.1f)] private float _radius = 12f;

        private readonly Collider2D[] _results = new Collider2D[32];
        private ContactFilter2D _filter;

        private void Awake()
        {
            _filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _enemyMask,
                useTriggers = true
            };
        }

        public bool TryGetAimDirection(Vector3 originWorld, out Vector2 dir)
        {
            dir = Vector2.right;

            int count = Physics2D.OverlapCircle((Vector2)originWorld, _radius, _filter, _results);
            if (count <= 0) return true; // нет врагов — пусть стреляет вправо, потом изменить на стреляет в место последнего направления !!!!!!

            float best = float.PositiveInfinity;
            Transform bestT = null;

            for (int i = 0; i < count; i++)
            {
                var c = _results[i];
                if (c == null) continue;

                float d2 = ((Vector2)(c.transform.position - originWorld)).sqrMagnitude;
                if (d2 < best)
                {
                    best = d2;
                    bestT = c.transform;
                }
            }

            if (bestT == null) return true;

            Vector2 d = (Vector2)(bestT.position - originWorld);
            if (d.sqrMagnitude < 0.0001f) return true;

            dir = d.normalized;
            return true;
        }
    }
}