using EventBus;
using UnityEngine;

namespace Levels.Strategies
{
    public struct LevelWonEvent : IEvent
    {
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
}