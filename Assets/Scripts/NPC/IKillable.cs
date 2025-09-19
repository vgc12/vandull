namespace NPC
{
    public interface IKillable
    {
        float Health { get; }
        void Die();
    }
}