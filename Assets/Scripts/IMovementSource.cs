using System;

public interface IMovementSource
{
    /// <summary>
    /// Фактическая мировая скорость объекта (после всех сил, разгона и т.д.)
    /// </summary>
    event Action<UnityEngine.Vector2> OnVelocityChanged;
}