using EventBus;
using UnityEngine;

namespace Npcs
{
    public readonly struct ThreatEvent : IEvent
    {
        public Enemy Enemy { get; init; }
        public Vector3 ThreatSourcePosition { get; init; }

        public ThreatEvent(Enemy enemy, Vector3 threatSourcePosition)
        {
            Enemy = enemy;
            ThreatSourcePosition = threatSourcePosition;
        }
    }
}