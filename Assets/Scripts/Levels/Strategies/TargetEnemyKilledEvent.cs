using EventBus;
using UnityEngine;

namespace Levels.Strategies
{
    public struct TargetEnemyKilledEvent : IEvent
    {
        public AssassinationTarget Enemy;
        public Vector3 Position;

        public TargetEnemyKilledEvent(AssassinationTarget enemy, Vector3 position)
        {
            Enemy = enemy;
            Position = position;
        }
    }
}