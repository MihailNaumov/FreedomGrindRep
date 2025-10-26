using UnityEditor.VersionControl;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveModule : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Разгон (0 = мгновенно, больше = дольше разгон)")]
    [SerializeField] private float acceleration = 0f;

    [Header("Замедление (0 = мгновенно, больше = дольше тормозит)")]
    [SerializeField] private float deceleration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 desiredVelocity = Vector2.zero;

    private EnemyStateController enemyController; // если это враг
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyController = GetComponent<EnemyStateController>();
    }

    private void FixedUpdate()
    {

        // Если есть внешний источник направления — берём оттуда
        if (enemyController != null)
        {
            var provider = enemyController.ActiveProvider;

            if (provider == null) // ТЕСТ, потом написать здесь просто $"[MoveModule] На объекте {gameObject.name} не найден ActiveProvider");
            {
                Debug.LogWarning($"[MoveModule] На объекте {gameObject.name} не найден ActiveProvider");
                desiredVelocity = Vector2.zero;
                rb.velocity = Vector2.zero; // ⬅️ явно остановим Rigidbody
                return;
            }

            if (provider is MonoBehaviour mb && !mb.enabled)
            {
                Debug.LogWarning($"[MoveModule] На объекте {gameObject.name} выключен ActiveProvider");
                desiredVelocity = Vector2.zero;
                rb.velocity = Vector2.zero; // ⬅️ обязательно
                return;
            }

            SetDirection(provider.GetInput());
        }

        // Движение с разгон/замедлением
        if (desiredVelocity != Vector2.zero)
        {
            if (acceleration == 0f)
            {
                rb.velocity = desiredVelocity;
            }
            else
            {
                float t = Time.deltaTime / acceleration;
                rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity, t);
            }
        }
        else
        {
            if (deceleration == 0f)
            {
                rb.velocity = Vector2.zero;
            }
            else
            {
                float t = Time.deltaTime / deceleration;
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, t);
            }
        }
        // if (enemyController != null) Debug.Log(desiredVelocity); // ТЕСТ
    }

    public void SetDirection(Vector2 direction)
    {
        desiredVelocity = direction.normalized * moveSpeed;
    }

    private void OnDisable()
    {
        if (rb != null)
            rb.velocity = Vector2.zero;
    }
}