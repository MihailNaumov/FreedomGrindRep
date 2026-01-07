namespace FreedomGrind.Combat
{
    /// <summary>
    /// Любая цель, которая может получать урон, реализует это.
    /// Обычно это HealthModule.
    /// </summary>
    public interface IDamageable
    {
        bool IsAlive { get; }
        bool TryApplyDamage(DamageInfo info);
    }
}
