using System;
using EventBus;
using UnityEngine;

namespace Items.Guns.Aiming
{
    public class AimingSystem : IAimingSystem
    {
        private readonly GunConfig _config;
        private readonly Transform _transform;

        public AimingSystem(
            Transform gunTransform,
            GunConfig config,
            Transform hipFirePoint,
            Transform aimFirePoint)
        {
            _transform = gunTransform ?? throw new ArgumentNullException(nameof(gunTransform));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            HipFirePoint = hipFirePoint ?? throw new ArgumentNullException(nameof(hipFirePoint));
            AimFirePoint = aimFirePoint ?? throw new ArgumentNullException(nameof(aimFirePoint));
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
            _transform.position = HipFirePoint.position;
        }

        public void Update()
        {
            _transform.position = Vector3.Lerp(_transform.position,
                IsAiming ? AimFirePoint.position : HipFirePoint.position,
                Time.deltaTime * (1f / _config.aimSettings.adsTime));
        }
    }
}