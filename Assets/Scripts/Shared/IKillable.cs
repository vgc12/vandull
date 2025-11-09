namespace Shared
{
    public interface IKillable
    {
        bool Invulnerable { get; }
        float Health { get; set; }

        float MaxHealth { get; }

        bool IsDead { get; }
        void Die();
    }
}