namespace NPC
{
    public interface IKillable
    {
        bool Invulnerable { get; }
        float Health => 100f;
        void Die();
    }
}