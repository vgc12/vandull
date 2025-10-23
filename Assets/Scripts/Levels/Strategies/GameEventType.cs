using EventBus;
using Npcs;
using UnityEngine;

namespace Levels.Strategies
{
    public struct LevelWonEvent : IEvent
    {
    }

    public struct LevelLostEvent : IEvent
    {
    }

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

    public struct HostageRescuedEvent : IEvent
    {
        public GameObject Hostage;
        public Vector3 RescuePosition;

        public HostageRescuedEvent(GameObject hostage, Vector3 rescuePosition)
        {
            Hostage = hostage;
            RescuePosition = rescuePosition;
        }
    }

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

    public struct ObjectiveCompletedEvent : IEvent
    {
        public string ObjectiveId;
        public string ObjectiveName;

        public ObjectiveCompletedEvent(string objectiveId, string objectiveName)
        {
            ObjectiveId = objectiveId;
            ObjectiveName = objectiveName;
        }
    }

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