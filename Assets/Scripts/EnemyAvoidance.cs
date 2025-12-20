using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAvoidance : MonoBehaviour
{
    private MoveModule moveModule;

    private void Awake()
    {
        // Ищем MoveModule на родителе
        moveModule = GetComponentInParent<MoveModule>();
        if (moveModule == null)
        {
            Debug.LogWarning($"[EnemyAvoidance] MoveModule не найден у родителя {transform.parent.name}");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other.gameObject != this.gameObject)
        {
            Vector2 awayFromOther = (transform.position - other.transform.position).normalized;
            moveModule.AddAvoidanceForce(awayFromOther * 0.3f); // подстрой коэффициент
        }
    }
}
