using EventBus;
using Items.Guns.Aiming;
using UnityEngine;

namespace Items.Guns.Recoil
{
    public class EnemyRecoilSystem : IRecoilSystem
    {
        private readonly EventBinding<AimChangedEvent> _aimChangedEventBinding;
        private readonly MonoBehaviour _behaviour;
        private readonly GunConfig _config;
   
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
   

        public EnemyRecoilSystem(GunConfig config, Transform recoilTransform,
            MonoBehaviour behaviour)
        {
            _config = config;
            _recoilTransform = recoilTransform;
            _behaviour = behaviour;

            _aimChangedEventBinding = new EventBinding<AimChangedEvent>(OnAimChanged);
            EventBus<AimChangedEvent>.Register(_aimChangedEventBinding);
            
        }

        public Vector3 CurrentRecoil => _currentRecoil * _config.recoilSettings.recoilEffectMultiplier;


        public void ApplyRecoil()
        {
            if (_config.recoilSettings.useProgressiveRecoil) UpdateProgressiveRecoil();

            // Calculate recoil values with current multiplier
            var verticalRecoil = _config.recoilSettings.verticalRecoil * _currentRecoilMultiplier;
            var horizontalRecoil =
                Random.Range(-_config.recoilSettings.horizontalRecoil, _config.recoilSettings.horizontalRecoil) *
                _currentRecoilMultiplier;

            // Apply camera recoil
            _targetRecoil += new Vector3(-verticalRecoil, horizontalRecoil, 0);

            // Apply gun physical recoil

            var randomPositionX = Random.Range(-_config.recoilSettings.positionRecoil.x * 0.5f,
                _config.recoilSettings.positionRecoil.x * 0.5f);
            var posRecoil = new Vector3(randomPositionX, _config.recoilSettings.positionRecoil.y,
                _config.recoilSettings.positionRecoil.z);
            _targetGunRecoil = posRecoil * _currentRecoilMultiplier;

            var randomRotationY = Random.Range(-_config.recoilSettings.rotationRecoil.y * 0.5f,
                _config.recoilSettings.rotationRecoil.y * 0.5f);
            var rotRecoil = new Vector3(_config.recoilSettings.rotationRecoil.x, randomRotationY,
                _config.recoilSettings.rotationRecoil.z);
            _targetGunRotationRecoil = rotRecoil * _currentRecoilMultiplier;


            _lastShotTime = Time.time;
            _consecutiveShots++;
        }

        public void Update()
        {
            // Update progressive recoil decay
            if (_config.recoilSettings.useProgressiveRecoil && Time.time > _lastShotTime + 0.5f)
            {
                _currentRecoilMultiplier = Mathf.Lerp(_currentRecoilMultiplier, 1f,
                    Time.deltaTime * _config.recoilSettings.recoilDecayRate);

                if (Time.time > _lastShotTime + 2f) _consecutiveShots = 0;
            }
            
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
                    _currentRecoilMultiplier * _config.recoilSettings.recoilMultiplierPerShot,
                    _config.recoilSettings.maxRecoilMultiplier
                );
        }

        private void ApplyGunRecoil()
        {
            var targetBasePosition = _basePosition;


            var targetPosition = targetBasePosition + _targetGunRecoil;


            _recoilTransform.position = Vector3.Lerp(_recoilTransform.position, targetPosition,
                Time.deltaTime * _config.recoilSettings.physicalRecoilSpeed);


            _currentGunRotationRecoil = Vector3.Lerp(_currentGunRotationRecoil, _targetGunRotationRecoil,
                Time.deltaTime * _config.recoilSettings.physicalRecoilSpeed);
          //  _gunTransform.localRotation = Quaternion.Euler(_originalGunRotation + _currentGunRotationRecoil);


            _targetGunRecoil = Vector3.Lerp(_targetGunRecoil, Vector3.zero,
                Time.deltaTime * _config.recoilSettings.physicalReturnSpeed);
            _targetGunRotationRecoil = Vector3.Lerp(_targetGunRotationRecoil, Vector3.zero,
                Time.deltaTime * _config.recoilSettings.physicalReturnSpeed);
        }
    }
}