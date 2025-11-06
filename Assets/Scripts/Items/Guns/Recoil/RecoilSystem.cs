using EventBus;
using Items.Guns.Aiming;
using UnityEngine;

namespace Items.Guns.Recoil
{
    public class RecoilSystem : IRecoilSystem
    {
        private readonly EventBinding<AimChangedEvent> _aimChangedEventBinding;
        private readonly MonoBehaviour _behaviour;
        private readonly Gun _gun;
        private readonly Transform _gunTransform;
        private readonly Vector3 _originalGunRotation;
        private readonly Transform _recoilTransform;

        private Vector3 _basePosition;
        private int _consecutiveShots;
        private Vector3 _currentGunRecoil;
        private Vector3 _currentGunRotationRecoil;

        // Camera recoil
        private Vector3 _currentRecoil;

        // Progressive recoil
        private float _currentRecoilMultiplier = 1f;
        private float _lastShotTime;

        // Gun physical recoil
        private Vector3 _originalGunPosition;

        // Coroutines
        private Coroutine _recoilCoroutine;
        private Vector3 _targetGunRecoil;
        private Vector3 _targetGunRotationRecoil;
        private Vector3 _targetRecoil;


        public RecoilSystem(Gun gun)
        {
            _gun = gun;
            _recoilTransform = gun.RecoilTransform;
            _gunTransform = gun.transform;
            _behaviour = gun;

            _aimChangedEventBinding = new EventBinding<AimChangedEvent>(OnAimChanged);
            EventBus<AimChangedEvent>.Register(_aimChangedEventBinding);

            _originalGunPosition = _gunTransform.localPosition;
            _originalGunRotation = _gunTransform.localEulerAngles;
        }

        public Vector3 CurrentRecoil => _currentRecoil * _gun.recoilSettings.recoilEffectMultiplier;


        public void ApplyRecoil()
        {
            if (_gun.recoilSettings.useProgressiveRecoil) UpdateProgressiveRecoil();

            // Calculate recoil values with current multiplier
            var verticalRecoil = _gun.recoilSettings.verticalRecoil * _currentRecoilMultiplier;
            var horizontalRecoil =
                Random.Range(-_gun.recoilSettings.horizontalRecoil, _gun.recoilSettings.horizontalRecoil) *
                _currentRecoilMultiplier;

            // Apply camera recoil
            _targetRecoil += new Vector3(-verticalRecoil, horizontalRecoil, 0);

            // Apply gun physical recoil

            var randomPositionX = Random.Range(-_gun.recoilSettings.positionRecoil.x * 0.5f,
                _gun.recoilSettings.positionRecoil.x * 0.5f);
            var posRecoil = new Vector3(randomPositionX, _gun.recoilSettings.positionRecoil.y,
                _gun.recoilSettings.positionRecoil.z);
            _targetGunRecoil = posRecoil * _currentRecoilMultiplier;

            var randomRotationY = Random.Range(-_gun.recoilSettings.rotationRecoil.y * 0.5f,
                _gun.recoilSettings.rotationRecoil.y * 0.5f);
            var rotRecoil = new Vector3(_gun.recoilSettings.rotationRecoil.x, randomRotationY,
                _gun.recoilSettings.rotationRecoil.z);
            _targetGunRotationRecoil = rotRecoil * _currentRecoilMultiplier;


            _lastShotTime = Time.time;
            _consecutiveShots++;
        }

        public void Update()
        {
            // Update progressive recoil decay
            if (_gun.recoilSettings.useProgressiveRecoil && Time.time > _lastShotTime + 0.5f)
            {
                _currentRecoilMultiplier = Mathf.Lerp(_currentRecoilMultiplier, 1f,
                    Time.deltaTime * _gun.recoilSettings.recoilDecayRate);

                if (Time.time > _lastShotTime + 2f) _consecutiveShots = 0;
            }

            // Apply camera recoil
            ApplyCameraRecoil();

            // Apply gun physical recoil
            ApplyGunRecoil();
        }

        public void Reset()
        {
            _currentRecoil = Vector3.zero;
            _targetRecoil = Vector3.zero;
            _currentGunRecoil = Vector3.zero;
            _targetGunRecoil = Vector3.zero;
            _currentGunRotationRecoil = Vector3.zero;
            _targetGunRotationRecoil = Vector3.zero;
            _currentRecoilMultiplier = 1f;
            _consecutiveShots = 0;

            if (_recoilCoroutine != null)
            {
                _behaviour.StopCoroutine(_recoilCoroutine);
                _recoilCoroutine = null;
            }
        }

        private void OnAimChanged(AimChangedEvent obj)
        {
            _basePosition = obj.GunPosition.localPosition;
        }

        private void UpdateProgressiveRecoil()
        {
            var timeSinceLastShot = Time.time - _lastShotTime;

            if (timeSinceLastShot < 0.3f) // Within burst window
                _currentRecoilMultiplier = Mathf.Min(
                    _currentRecoilMultiplier * _gun.recoilSettings.recoilMultiplierPerShot,
                    _gun.recoilSettings.maxRecoilMultiplier
                );
        }

        private void ApplyCameraRecoil()
        {
            _currentRecoil = Vector3.Slerp(_currentRecoil, _targetRecoil,
                Time.deltaTime * _gun.recoilSettings.recoilSpeed);


            if (_recoilTransform)
                _recoilTransform.localRotation =
                    Quaternion.Euler(_currentRecoil * _gun.recoilSettings.recoilEffectMultiplier);


            _targetRecoil = Vector3.Lerp(_targetRecoil, Vector3.zero,
                Time.deltaTime * _gun.recoilSettings.returnSpeed);
        }


        private void ApplyGunRecoil()
        {
            var targetBasePosition = _basePosition;


            var targetPosition = targetBasePosition + _targetGunRecoil;


            _gunTransform.localPosition = Vector3.Lerp(_gunTransform.localPosition, targetPosition,
                Time.deltaTime * _gun.recoilSettings.physicalRecoilSpeed);


            _currentGunRotationRecoil = Vector3.Lerp(_currentGunRotationRecoil, _targetGunRotationRecoil,
                Time.deltaTime * _gun.recoilSettings.physicalRecoilSpeed);
            _gunTransform.localRotation = Quaternion.Euler(_originalGunRotation + _currentGunRotationRecoil);


            _targetGunRecoil = Vector3.Lerp(_targetGunRecoil, Vector3.zero,
                Time.deltaTime * _gun.recoilSettings.physicalReturnSpeed);
            _targetGunRotationRecoil = Vector3.Lerp(_targetGunRotationRecoil, Vector3.zero,
                Time.deltaTime * _gun.recoilSettings.physicalReturnSpeed);
        }
    }
}