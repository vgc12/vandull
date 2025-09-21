using EventBus;
using UnityEngine;

namespace Items.Guns
{
    public class AimChangedEvent : IEvent
    {
        public bool IsAiming { get; }
        public Transform GunPosition { get; }

        public AimChangedEvent(bool isAiming, Transform gunPosition)
        {
            IsAiming = isAiming;
            GunPosition = gunPosition;
        }
    }
}