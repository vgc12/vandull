namespace Shared
{
    public interface IKillable
    {
        bool Invulnerable { get; }
        float Health { get; set; }

        bool IsDead { get; }
        void Die();
    }
}