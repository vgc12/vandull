using System;
using UnityEngine;

namespace Items.Guns
{
    public class AimingSystem : IAimingSystem
    {
        private readonly Transform _transform;
        private readonly GunConfig _config;


        public Transform HipFirePoint { get; }

        public Transform AimFirePoint { get; }

        public event Action OnAimStarted;
        public event Action OnAimStopped;
        public bool IsAiming { get; private set; }

        public AimingSystem( Gun gun)
        {
            _transform = gun.transform;
            _config = gun.gunConfig;
            AimFirePoint = gun.adsTransform;
            HipFirePoint = gun.hipFireTransform;

        }

        public void StartAiming( )
        {
            IsAiming = true;
            OnAimStarted?.Invoke();
        }

        public void StopAiming()
        {
            IsAiming = false;
            OnAimStopped?.Invoke();
        }

        public void Update()
        {
            _transform.position = Vector3.Lerp(_transform.position,
                IsAiming ? AimFirePoint.position : HipFirePoint.position, Time.deltaTime * (1f / _config.aimSettings.adsTime));
        }
    }
}