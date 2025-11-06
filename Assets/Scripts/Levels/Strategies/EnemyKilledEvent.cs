using EventBus;
using Npcs;
using UnityEngine;

namespace Levels.Strategies
{
    public struct EnemyKilledEvent : IEvent
    {
        public Enemy Enemy;
        public Vector3 Position;

        public EnemyKilledEvent(Enemy enemy, Vector3 position)
        {
            Enemy = enemy;
            Position = position;
        }
    }
}