namespace Shared
{
    public interface IKillable
    {
        bool Invulnerable { get; }
        float Health => 100f;

        bool IsDead { get; }
        void Die();
    }
}