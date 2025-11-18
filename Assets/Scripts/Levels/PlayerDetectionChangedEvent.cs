using EventBus;
using Npcs;

namespace Levels
{
    public readonly struct PlayerDetectionChangedEvent : IEvent
    {
        public readonly Enemy Enemy;
        public bool Detected => Enemy.PlayerDetected;

        public PlayerDetectionChangedEvent(Enemy enemy)
        {
            Enemy = enemy;
        }
    }
}