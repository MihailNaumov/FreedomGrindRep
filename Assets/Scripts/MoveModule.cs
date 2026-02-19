using System;
using UnityEditor.VersionControl;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MoveModule : MonoBehaviour, IMovementSource
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Разгон (0 = мгновенно, больше = дольше разгон)")]
    [SerializeField] private float acceleration = 0f;

    [Header("Замедление (0 = мгновенно, больше = дольше тормозит)")]
    [SerializeField] private float deceleration = 0.3f;

    private Rigidbody2D rb;
    private Vector2 desiredVelocity = Vector2.zero;
    private Vector2 extraForces = Vector2.zero;
    private EnemyStateController enemyController; // если это враг

    public event Action<Vector2> OnVelocityChanged;

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
                rb.velocity = desiredVelocity + extraForces;
            }
            else
            {
                float t = Time.fixedDeltaTime / acceleration;
                rb.velocity = Vector2.Lerp(rb.velocity, desiredVelocity + extraForces, t);
            }
        }
        else
        {
            if (deceleration == 0f)
            {
                rb.velocity = extraForces;
            }
            else
            {
                float t = Time.fixedDeltaTime / deceleration;
                rb.velocity = Vector2.Lerp(rb.velocity, extraForces, t);
            }
        }
        // if (enemyController != null) Debug.Log(desiredVelocity); // ТЕСТ
        extraForces = Vector2.Lerp(extraForces, Vector2.zero, Time.fixedDeltaTime * 5f);

        OnVelocityChanged?.Invoke(rb.velocity);
    }

    public void SetDirection(Vector2 direction)
    {
        desiredVelocity = direction.normalized * moveSpeed;
    }

    

    public void AddAvoidanceForce(Vector2 force)
    {
        extraForces += force;
    }

    private void OnDisable()
    {
        if (rb != null)
        rb.velocity = Vector2.zero;

        OnVelocityChanged?.Invoke(Vector2.zero);

    }
}