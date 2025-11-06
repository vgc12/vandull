using EventBus;
using UnityEngine;

namespace Levels.Strategies
{
    public struct BombDefusedEvent : IEvent
    {
        public GameObject Bomb;
        public float TimeRemaining;
        public Vector3 BombPosition;

        public BombDefusedEvent(GameObject bomb, float timeRemaining, Vector3 bombPosition)
        {
            Bomb = bomb;
            TimeRemaining = timeRemaining;
            BombPosition = bombPosition;
        }
    }
}