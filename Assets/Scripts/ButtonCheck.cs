using UnityEngine;

public class ButtonCheck : MonoBehaviour
{
    private MoveModule moveModule;
    private Vector2 currentDirection = Vector2.zero;

    private void Awake()
    {
        moveModule = GetComponent<MoveModule>();

        if (moveModule == null)
        {
            Debug.LogWarning($"[ButtonCheck] На объекте {gameObject.name} не найден компонент MoveModule! Движение невозможно.");
        }
    }

    private void Update()
    {
        if (!moveModule.enabled) { Debug.LogWarning($"[ButtonCheck] MoveModule выключен! Движение невозможно."); return; }

        Vector2 newDirection = Vector2.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) newDirection.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) newDirection.y -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) newDirection.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) newDirection.x += 1;

        if (newDirection != currentDirection)
        {
            currentDirection = newDirection;
            moveModule?.SetDirection(currentDirection);
        }
    }
}