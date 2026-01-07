using UnityEngine;

namespace FreedomGrind.Combat
{
    /// <summary>Единый пакет данных об уроне.</summary>
    [System.Serializable]
    public struct DamageInfo
    {
        public float amount;
        public DamageType type;

        public GameObject instigator; // кто начал (юнит)
        public GameObject source;     // чем ударили (хитбокс/снаряд)
        public Vector2 hitPoint;      // точка попадания (опц.)
    }
}
