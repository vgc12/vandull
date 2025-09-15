using System;
using UnityEngine;

namespace Items.Guns
{
    public class AimingSystem : IAimingSystem
    {
        private readonly Transform _transform;
        private readonly GunConfig _config;


        public event Action OnAimStarted;
        public event Action OnAimStopped;
        public bool IsAiming { get; private set; }

        public AimingSystem( GunConfig config, Transform transform)
        {
            _transform = transform;
            _config = config;
            
          
  
        }

        public void StartAiming()
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
            _transform.localPosition = Vector3.Lerp(_transform.localPosition,
                IsAiming ? _config.aimSettings.adsPosition : _config.aimSettings.hipFirePoint, Time.deltaTime * (1f / _config.aimSettings.adsTime));
        }
    }
}