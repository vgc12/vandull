using System;
using System.Collections;
using System.Collections.Generic;
using General;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public class FireSystem : IFireSystem
    {
        public event Action<Vector3, float> OnFired;
        public event Action OnFireModeChanged; // New event for UI updates
    
        private readonly GunConfig _config;
        private readonly Vector3 _muzzlePoint;
        private readonly Transform _transform;
        private readonly IAmmoSystem _ammoSystem;
        private readonly RaycastHit[] _hitResults = new RaycastHit[10];
    
        private bool _canFire = true;
        private Coroutine _cooldownCoroutine;
        private Coroutine _autoFireCoroutine;
        private Coroutine _burstFireCoroutine;
    
        // Fire mode properties
        private FireType _currentFireType;
        private readonly List<FireType> _availableFireModes;
        private int _currentFireModeIndex;
    
        // Burst fire properties
        private readonly int _burstCount;
        private readonly float _burstDelay;

        public FireType CurrentFireType => _currentFireType;
        public IReadOnlyList<FireType> AvailableFireModes => _availableFireModes;

        public FireSystem(Transform transform, GunConfig config, Vector3 muzzlePoint, IAmmoSystem ammoSystem)
        {
            _transform = transform;
            _config = config;
            _muzzlePoint = muzzlePoint;
            _ammoSystem = ammoSystem;
        

            _availableFireModes = new List<FireType>(config.availableFireModes);
            _currentFireType = _availableFireModes.Count > 0 ? _availableFireModes[0] : FireType.SemiAutomatic;
            _currentFireModeIndex = 0;
        
     
            _burstCount = config.firingSettings.burstCount;
            _burstDelay = config.firingSettings.burstDelay;
        }

        public bool CanFire() => _canFire && !_ammoSystem.IsReloading;


        public void Fire(InputAction.CallbackContext context)
        {
            if (!CanFire()) return;
            if (context.started)
            {
                if (_currentFireType == FireType.SemiAutomatic)
                    FireSingle();
                else if (_currentFireType == FireType.Automatic)
                    StartAutomaticFire();
                else if (_currentFireType == FireType.Burst)
                    StartBurstFire();
            }
            else if (context.canceled)
            {
                if (_currentFireType == FireType.Automatic)
                    StopAutomaticFire();
            }
        }

        public void StopFire()
        {
            StopAutomaticFire();
          
        }


        public void SetFireMode(FireType fireType)
        {
            if (!_availableFireModes.Contains(fireType)) return;
        
            StopAllFiring();
            _currentFireType = fireType;
            _currentFireModeIndex = _availableFireModes.IndexOf(fireType);
            OnFireModeChanged?.Invoke();
        }

        public void CycleFireMode()
        {
            if (_availableFireModes.Count <= 1) return;
        
            StopAllFiring();
            _currentFireModeIndex = (_currentFireModeIndex + 1) % _availableFireModes.Count;
            _currentFireType = _availableFireModes[_currentFireModeIndex];
            OnFireModeChanged?.Invoke();
        }

        private void FireSingle()
        {
            if (!CanFire()) return;
        
            PerformShot();
            StartCooldown();
        }

        private void StartAutomaticFire()
        {
            if (_autoFireCoroutine != null) return;
            if (!CanFire()) return;
        
            _autoFireCoroutine = CoroutineRunner.StartCoroutine(AutomaticFireRoutine());
        }

        private void StopAutomaticFire()
        {
            if (_autoFireCoroutine != null)
            {
                CoroutineRunner.StopCoroutine(_autoFireCoroutine);
                _autoFireCoroutine = null;
            }
        }

        private void StartBurstFire()
        {
            if (_burstFireCoroutine != null) return;
            if (!CanFire()) return;
        
            _burstFireCoroutine = CoroutineRunner.StartCoroutine(BurstFireRoutine());
        }

        private void StopAllFiring()
        {
            StopAutomaticFire();
        
            if (_burstFireCoroutine != null)
            {
                CoroutineRunner.StopCoroutine(_burstFireCoroutine);
                _burstFireCoroutine = null;
            }
        }

        private IEnumerator AutomaticFireRoutine()
        {
            while (CanFire())
            {
                PerformShot();
                yield return new WaitForSeconds(_config.firingSettings.fireRate);
            }
            _autoFireCoroutine = null;
        }

        private IEnumerator BurstFireRoutine()
        {
            _canFire = false;
        
            for (int i = 0; i < _burstCount && !_ammoSystem.IsCurrentMagazineEmpty; i++)
            {
                PerformShot();
            
                if (i < _burstCount - 1) // Don't wait after the last shot
                {
                    yield return new WaitForSeconds(_burstDelay);
                }
            }
            
            yield return new WaitForSeconds(_config.firingSettings.fireRate - _burstDelay);
        
            _canFire = true;
            _burstFireCoroutine = null;
        }

        private void PerformShot()
        {
            _ammoSystem.ConsumeAmmo();
            PerformRaycast();
            OnFired?.Invoke(_muzzlePoint + _transform.position , _config.damageSettings.damage);
        }

        private void PerformRaycast()
        {
            
            
            int hitCount = Physics.RaycastNonAlloc(
                _muzzlePoint + _transform.position , 
                 _transform.forward , 
                _hitResults, 
                _config.damageSettings.range);

            if (hitCount <= 0) return;
            ProcessHits(hitCount);
        }

        private void ProcessHits(int hitCount)
        {
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitResults[i];
                if (hit.collider == null) continue;

                VandullLogger.Log($"Hit {hit.collider.name} at distance {hit.distance}");
            }
        }

        private void StartCooldown()
        {
            if (_cooldownCoroutine != null) return;
            _cooldownCoroutine = CoroutineRunner.StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            _canFire = false;
            yield return new WaitForSeconds(_config.firingSettings.fireRate);
            _canFire = true;
            _cooldownCoroutine = null;
        }
    }
}
