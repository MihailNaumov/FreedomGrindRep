using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary> Интерфейс “откуда брать направление”</summary>
    public interface IAimProvider2D
    {
        bool TryGetAimDirection(Vector3 originWorld, out Vector2 dir);
    }
}