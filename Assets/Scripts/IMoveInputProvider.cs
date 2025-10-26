using UnityEngine;

public interface IMoveInputProvider // Это интерфейс чтобы разные типы движения врагов (по прямой, через навмеш и т.д.) могли подключаться к MoveModule одинаково.
{
    Vector2 GetInput();
}