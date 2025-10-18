using EventBus;

namespace Player
{
    public class PlayerHitEvent : IEvent
    {
        public readonly float NewHealth;

        public PlayerHitEvent(float newHealth)
        {
            NewHealth = newHealth;
        }
    }
}