using UnityEngine;

namespace Items.Guns
{
    public class AimingSystem : IAimingSystem
    {
        private readonly Transform _transform;
        private  Vector3 _adsPosition;
        private  Vector3 _hipPosition;
        private readonly float _aimTime;
        

        public bool IsAiming { get; private set; }

        public AimingSystem(Transform transform, float aimTime)
        {
            _transform = transform;
          
            _aimTime = aimTime;
        }

        public void StartAiming(Vector3 adsPosition, Vector3 hipPosition)
        {
            _adsPosition = adsPosition;
            _hipPosition = hipPosition;
            IsAiming = true;
        }

        public void StopAiming()
        {
            IsAiming = false;
        }

        public void Update()
        {
            _transform.localPosition = Vector3.Lerp(_transform.localPosition,
                IsAiming ? _adsPosition : _hipPosition, Time.deltaTime * (1f / _aimTime));
        }
    }
}