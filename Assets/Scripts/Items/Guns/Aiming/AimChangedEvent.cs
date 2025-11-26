using EventBus;
using UnityEngine;

namespace Items.Guns.Aiming
{
    public sealed class AimChangedEvent : IEvent
    {
        public AimChangedEvent(bool isAiming, Transform gunPosition)
        {
            IsAiming = isAiming;
            GunPosition = gunPosition;
        }

        public bool IsAiming { get; }
        public Transform GunPosition { get; }
    }
}