using UnityEngine;

public class EnemyDirectInput : MonoBehaviour, IMoveInputProvider // Реализация IMoveInputProvider, которая делает врага тупым
{
    [SerializeField] private Transform target;
    
    [Tooltip("Минимальная дистанция до цели, при которой враг прекращает движение")]
    [SerializeField] private float stopDistance = 0.5f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public Vector2 GetInput()
    {
        if (target == null) return Vector2.zero;

        Vector2 offset = target.position - transform.position;

        // Если слишком близко — не двигаемся
        if (offset.sqrMagnitude < stopDistance * stopDistance)
        {
            return Vector2.zero;
        }

        return offset.normalized;
    }
}