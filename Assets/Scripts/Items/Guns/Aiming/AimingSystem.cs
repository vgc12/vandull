using EventBus;
using UnityEngine;

namespace Items.Guns.Aiming
{
    public class AimingSystem : IAimingSystem
    {
        private readonly Gun _gun;

        public AimingSystem(Gun gun)
        {
            _gun = gun;

            HipFirePoint = gun.hipFireTransform;
            AimFirePoint = gun.aimTransform;
        }


        public Transform HipFirePoint { get; }

        public Transform AimFirePoint { get; }

        public bool IsAiming { get; private set; }

        public void StartAiming()
        {
            IsAiming = true;
            EventBus<AimChangedEvent>.Raise(new AimChangedEvent(true, AimFirePoint));
        }

        public void StopAiming()
        {
            IsAiming = false;
            EventBus<AimChangedEvent>.Raise(new AimChangedEvent(false, HipFirePoint));
        }

        public void ResetPosition()
        {
            _gun.transform.position = HipFirePoint.position;
        }

        public void Update()
        {
            _gun.transform.position = Vector3.Lerp(_gun.transform.position,
                IsAiming ? AimFirePoint.position : HipFirePoint.position,
                Time.deltaTime * (1f / _gun.aimSettings.adsTime));
        }
    }
}