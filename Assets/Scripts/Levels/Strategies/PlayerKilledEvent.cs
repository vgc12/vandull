using EventBus;
using UnityEngine;

namespace Levels.Strategies
{
    public struct PlayerKilledEvent : IEvent
    {
        public GameObject Player;
        public Vector3 DeathPosition;
        public string CauseOfDeath;

        public PlayerKilledEvent(GameObject player, Vector3 deathPosition, string causeOfDeath)
        {
            Player = player;
            DeathPosition = deathPosition;
            CauseOfDeath = causeOfDeath;
        }
    }
}