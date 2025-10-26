using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshInput : MonoBehaviour, IMoveInputProvider
{
    [Header("������ �� NavMeshAgent (������� ������� � ����������)")]
    [SerializeField] private NavMeshAgent agent;

    private void Awake()
    {
        if (agent == null)
        {
            Debug.LogWarning($"[EnemyNavMeshInput] �� ������� {gameObject.name} �� �������� NavMeshAgent!");
            return;
        }

        // ��������� ��� 2D (��������� �������� � ��� Y)
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public Vector2 GetInput()
    {
        if (agent == null)
        {
            return Vector2.zero;
        }

        // ���������� ��������������� ����������� �� 3D � 2D (XY)
        return new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.z).normalized;
    }
}