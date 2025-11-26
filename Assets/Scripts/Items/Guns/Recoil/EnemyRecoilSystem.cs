using UnityEngine;

namespace Items.Guns.Recoil
{
    public sealed class NullRecoilSystem : IRecoilSystem
    {
        public Vector3 CurrentRecoil { get; private set; }

        public void ApplyRecoil()
        {
            // noop
        }

        public void Update()
        {
            // noop
        }

        public void Reset()
        {
            CurrentRecoil = Vector3.zero;
        }
    }

    public class EnemyRecoilSystem : IRecoilSystem
    {
        private readonly Vector3 _basePosition;
        private readonly Quaternion _baseRotation;

        private readonly Gun _gun;
        private readonly Transform _recoilTransform;


        private float _currentSpreadMultiplier = 1f;

        private float _lastShotTime;
        private Vector3 _targetSpreadOffset;

        public EnemyRecoilSystem(Gun gun)
        {
            _gun = gun;
            _recoilTransform = gun.RecoilTransform;


            _basePosition = _recoilTransform.localPosition;
            _baseRotation = _recoilTransform.localRotation;
        }

        public Vector3 CurrentRecoil { get; private set; }

        public void ApplyRecoil()
        {
            // noop
        }

        public void Update()
        {
            // Update progressive spread decay
            if (_gun.recoilSettings.useProgressiveRecoil && Time.time > _lastShotTime + 0.5f)
                _currentSpreadMultiplier = Mathf.Lerp(_currentSpreadMultiplier, 1f,
                    Time.deltaTime * _gun.recoilSettings.recoilDecayRate);
        }

        public void Reset()
        {
            CurrentRecoil = Vector3.zero;
            _targetSpreadOffset = Vector3.zero;
            _currentSpreadMultiplier = 1f;


            _recoilTransform.localPosition = _basePosition;
            _recoilTransform.localRotation = _baseRotation;
        }
    }
}