using EventBus;
using UnityEngine;

namespace Items.Guns
{
    internal sealed class GunFiredEvent : IEvent
    {
        public GunFiredEvent(Vector3 position, float damage)
        {
            Position = position;
            Damage = damage;
        }

        public Vector3 Position { get; }
        public float Damage { get; }
    }
}