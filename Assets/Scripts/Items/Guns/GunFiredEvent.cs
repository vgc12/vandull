using EventBus;
using UnityEngine;

namespace Items.Guns
{
    internal class GunFiredEvent : IEvent
    {
        public Vector3 Position { get; }
        public float Damage { get; }

        public GunFiredEvent(Vector3 position, float damage)
        {
            Position = position;
            Damage = damage;
        }
    }
}