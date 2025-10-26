using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshInput : MonoBehaviour, IMoveInputProvider
{
    [Header("Ссылка на NavMeshAgent (назначь вручную в инспекторе)")]
    [SerializeField] private NavMeshAgent agent;

    private void Awake()
    {
        if (agent == null)
        {
            Debug.LogWarning($"[EnemyNavMeshInput] На объекте {gameObject.name} не назначен NavMeshAgent!");
            return;
        }

        // Настройки для 2D (отключаем вращение и ось Y)
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    public Vector2 GetInput()
    {
        if (agent == null)
        {
            return Vector2.zero;
        }

        // Возвращаем нормализованное направление из 3D в 2D (XY)
        return new Vector2(agent.desiredVelocity.x, agent.desiredVelocity.z).normalized;
    }
}