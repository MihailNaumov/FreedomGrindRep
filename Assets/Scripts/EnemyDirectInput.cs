using UnityEngine;

public class EnemyDirectInput : MonoBehaviour, IMoveInputProvider // ���������� IMoveInputProvider, ������� ������ ����� �����
{
    [SerializeField] private Transform target;
    
    [Tooltip("����������� ��������� �� ����, ��� ������� ���� ���������� ��������")]
    [SerializeField] private float stopDistance = 0.5f;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public Vector2 GetInput()
    {
        if (target == null) return Vector2.zero;

        Vector2 offset = target.position - transform.position;

        // ���� ������� ������ � �� ���������
        if (offset.sqrMagnitude < stopDistance * stopDistance)
        {
            return Vector2.zero;
        }

        return offset.normalized;
    }
}