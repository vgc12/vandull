using EventBus;
using UnityEngine;

namespace Player
{
    public class PlayerHitEvent : IEvent
    {
        public readonly Transform DamageTransform;
        public readonly Vector3 HitDirection;
        public readonly float NewHealth;

        public PlayerHitEvent(float newHealth, Vector3 hitDirection, Transform damageTransform)
        {
            NewHealth = newHealth;
            HitDirection = hitDirection;
            DamageTransform = damageTransform;
        }
    }
}