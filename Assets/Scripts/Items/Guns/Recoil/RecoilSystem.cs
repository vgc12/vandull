using System.Collections;
using EventBus;
using UnityEngine;

namespace Items.Guns.Recoil
{
    public class RecoilSystem : IRecoilSystem
    {
        private readonly GunConfig _config;
        private readonly Transform _recoilTransform;
        private readonly Transform _gunTransform;
        private readonly MonoBehaviour _behaviour;

        private EventBinding<AimChangedEvent> _aimChangedEventBinding;

        // Camera recoil
        private Vector3 _currentRecoil;
        private Vector3 _targetRecoil;

        // Gun physical recoil
        private Vector3 _originalGunPosition;
        private Vector3 _originalGunRotation;
        private Vector3 _currentGunRecoil;
        private Vector3 _targetGunRecoil;
        private Vector3 _currentGunRotationRecoil;
        private Vector3 _targetGunRotationRecoil;

        // Progressive recoil
        private float _currentRecoilMultiplier = 1f;
        private float _lastShotTime;
        private int _consecutiveShots;

        // Coroutines
        private Coroutine _recoilCoroutine;
        
        public Vector3 CurrentRecoil => _currentRecoil * _config.recoilSettings.recoilEffectMultiplier;

        private Vector3 _basePosition;
    
        
     
        public RecoilSystem(GunConfig config, Transform gunTransform, Transform recoilTransform, MonoBehaviour behaviour)
        {
            _config = config;
            _recoilTransform = recoilTransform;
            _gunTransform = gunTransform;
            _behaviour = behaviour;

            _aimChangedEventBinding = new EventBinding<AimChangedEvent>(OnAimChanged);
            EventBus<AimChangedEvent>.Register(_aimChangedEventBinding);
            
            _originalGunPosition = _gunTransform.localPosition;
            _originalGunRotation = _gunTransform.localEulerAngles;
            
        }

        private void OnAimChanged(AimChangedEvent obj)
        {
            _basePosition = obj.GunPosition.localPosition;;
        }


        public void ApplyRecoil()
        {
            if (_config.recoilSettings.useProgressiveRecoil)
            {
                UpdateProgressiveRecoil();
            }

            // Calculate recoil values with current multiplier
            float verticalRecoil = _config.recoilSettings.verticalRecoil * _currentRecoilMultiplier;
            float horizontalRecoil =
                Random.Range(-_config.recoilSettings.horizontalRecoil, _config.recoilSettings.horizontalRecoil) *
                _currentRecoilMultiplier;

            // Apply camera recoil
            _targetRecoil += new Vector3(-verticalRecoil, horizontalRecoil, 0);

            // Apply gun physical recoil
        
            var randomPositionX = Random.Range(-_config.recoilSettings.positionRecoil.x * 0.5f, _config.recoilSettings.positionRecoil.x * 0.5f);
            var posRecoil = new Vector3(randomPositionX, _config.recoilSettings.positionRecoil.y, _config.recoilSettings.positionRecoil.z);
            _targetGunRecoil =  posRecoil * _currentRecoilMultiplier;
            
            var randomRotationY = Random.Range(-_config.recoilSettings.rotationRecoil.y * 0.5f, _config.recoilSettings.rotationRecoil.y * 0.5f);
            var rotRecoil = new Vector3(_config.recoilSettings.rotationRecoil.x, randomRotationY, _config.recoilSettings.rotationRecoil.z);
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

                if (Time.time > _lastShotTime + 2f)
                {
                    _consecutiveShots = 0;
                }
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

        private void UpdateProgressiveRecoil()
        {
            float timeSinceLastShot = Time.time - _lastShotTime;

            if (timeSinceLastShot < 0.3f) // Within burst window
            {
                _currentRecoilMultiplier = Mathf.Min(
                    _currentRecoilMultiplier * _config.recoilSettings.recoilMultiplierPerShot,
                    _config.recoilSettings.maxRecoilMultiplier
                );
            }
        }

        private void ApplyCameraRecoil()
        {
           
            _currentRecoil = Vector3.Slerp(_currentRecoil, _targetRecoil,
                Time.deltaTime * _config.recoilSettings.recoilSpeed);

           
            if (_recoilTransform)
            {
                _recoilTransform.localRotation = Quaternion.Euler(  _currentRecoil * _config.recoilSettings.recoilEffectMultiplier);

            }
            

    
            _targetRecoil = Vector3.Lerp(_targetRecoil, Vector3.zero,
                Time.deltaTime * _config.recoilSettings.returnSpeed);
        }
        
        
        private void ApplyGunRecoil()
        {

            Vector3 targetBasePosition = _basePosition;
    

            Vector3 targetPosition = targetBasePosition + _targetGunRecoil;
    
   
            _gunTransform.localPosition = Vector3.Lerp(_gunTransform.localPosition, targetPosition,
                Time.deltaTime * _config.recoilSettings.physicalRecoilSpeed);
    
    
            _currentGunRotationRecoil = Vector3.Lerp(_currentGunRotationRecoil, _targetGunRotationRecoil,
                Time.deltaTime * _config.recoilSettings.physicalRecoilSpeed);
            _gunTransform.localRotation = Quaternion.Euler(_originalGunRotation + _currentGunRotationRecoil);

    
            _targetGunRecoil = Vector3.Lerp(_targetGunRecoil, Vector3.zero,
                Time.deltaTime * _config.recoilSettings.physicalReturnSpeed);
            _targetGunRotationRecoil = Vector3.Lerp(_targetGunRotationRecoil, Vector3.zero,
                Time.deltaTime * _config.recoilSettings.physicalReturnSpeed);
        }

    }

 
}