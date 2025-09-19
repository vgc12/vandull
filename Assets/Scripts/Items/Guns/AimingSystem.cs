using System;
using UnityEngine;

namespace Items.Guns
{
    public class AimingSystem : IAimingSystem
    {
        private readonly Transform _transform;
        private readonly Transform _aimTransform;
        private readonly Transform _hipFireTransform;
        private readonly GunConfig _config;


        public event Action OnAimStarted;
        public event Action OnAimStopped;
        public bool IsAiming { get; private set; }

        public AimingSystem( Gun gun)
        {
            _transform = gun.transform;
            _config = gun.gunConfig;
            _aimTransform = gun.adsTransform;
            _hipFireTransform = gun.hipFireTransform;

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
                IsAiming ? _aimTransform.position : _hipFireTransform.position, Time.deltaTime * (1f / _config.aimSettings.adsTime));
        }
    }
}