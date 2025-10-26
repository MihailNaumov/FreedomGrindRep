using UnityEngine;

public class EnemyStateController : MonoBehaviour
{
    public enum MovementMode
    {
        Direct,
        NavMesh
    }

    [Header("Текущий режим поведения врага")]
    [SerializeField] private MovementMode currentMode;

    [Header("Провайдеры движения")]
    [SerializeField] private EnemyNavMeshInput navInput;
    [SerializeField] private EnemyDirectInput directInput;
    public IMoveInputProvider ActiveProvider //общая точка доступа, чтобы получить текущий активный провайдер движения
    {
        get
        {
            IMoveInputProvider selected = currentMode switch
            {
                MovementMode.Direct => directInput,
                MovementMode.NavMesh => navInput,
                _ => null
            };

            // 💥 Фильтр: если компонент выключен — вернём null
            if (selected is MonoBehaviour mb && !mb.enabled)
                return null;

            return selected;
        }
    }
    /// <summary>
    /// Применяет текущий выбранный режим движения.
    /// </summary>
    public void ApplyCurrentMode()
    {
        // Включаем нужный режим, выключаем ненужный
        switch (currentMode)
        {
            case MovementMode.Direct:
                if (directInput != null) directInput.enabled = true;
                if (navInput != null) navInput.enabled = false;
                break;
            case MovementMode.NavMesh:
                if (directInput != null) directInput.enabled = false;
                if (navInput != null) navInput.enabled = true;
                break;
        }
    }

    /// <summary>
    /// Устанавливает режим движения и сразу применяет.
    /// Удобно вызывать из других скриптов.
    /// </summary>
    public void SetMovementMode(MovementMode newMode)
    {
        currentMode = newMode;
        ApplyCurrentMode();
    }

    /// <summary>
    /// Автоматически находит компоненты, если не заданы вручную.
    /// </summary>
    private void Awake()
    {
        ApplyCurrentMode(); // применяем поведение при запуске
    }

    /// <summary>
    /// Срабатывает при изменении значений в инспекторе в редакторе.
    /// Позволяет тестить переключение прямо в Editor.
    /// </summary>
    private void OnValidate()
    {
        ApplyCurrentMode();
    }
}