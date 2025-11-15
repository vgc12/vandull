using EventBus;
using UnityEngine;

namespace Player
{
    public class PlayerHitEvent : IEvent
    {
        public readonly Vector3 DamageLocation;
        public readonly Vector3 HitDirection;
        public readonly float NewHealth;

        public PlayerHitEvent(float newHealth, Vector3 hitDirection, Vector3 damageLocation)
        {
            NewHealth = newHealth;
            HitDirection = hitDirection;
            DamageLocation = damageLocation;
        }
    }
}