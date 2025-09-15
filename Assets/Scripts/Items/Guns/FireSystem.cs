using System;
using System.Collections;
using System.Collections.Generic;
using General;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Items.Guns
{
    public class FireSystem : IFireSystem
    {
        public event Action<Vector3, float> OnFired;
        public event Action OnFireModeChanged; 

        private readonly GunConfig _config;
        private readonly Vector3 _muzzlePoint;
        private readonly Transform _transform;
        private readonly IAmmoSystem _ammoSystem;
        private readonly MonoBehaviour _behaviour;
        private readonly RaycastHit[] _hitResults = new RaycastHit[10];

       
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

        private ITrailSystem _trailSystem;
        
        private float _lastFireTime;
        
        private IRecoilSystem _recoilSystem;
        private bool _fireButtonHeld;

        public FireSystem(Transform transform, GunConfig config,  MonoBehaviour behaviour, IRecoilSystem recoilSystem,
            IAmmoSystem ammoSystem, ITrailSystem trailSystem)
        {
            _trailSystem = trailSystem;
            _recoilSystem = recoilSystem;
            _transform = transform;
            _config = config;
            _muzzlePoint = config.aimSettings.muzzlePoint;
            _ammoSystem = ammoSystem;
            _behaviour = behaviour;

            _availableFireModes = new List<FireType>(config.fireModeSettings.availableFireModes);
            _currentFireType = _availableFireModes.Count > 0 ? _availableFireModes[0] : FireType.SemiAutomatic;
            _currentFireModeIndex = 0;


            _burstCount = config.firingSettings.burstCount;
            _burstDelay = config.firingSettings.burstDelay;
            
            OnFired += (position, damage) =>
            {
                _lastFireTime = Time.time;
            };
        }

        public bool CanFire => Time.time > _lastFireTime + _config.firingSettings.fireRate &&  !_ammoSystem.IsReloading && !_ammoSystem.IsCurrentMagazineEmpty;

        

        public void Fire(InputAction.CallbackContext context)
        {
        
            if (context.started)
            {
                if (_currentFireType == FireType.SemiAutomatic)
                    FireSingle();
            
                else if (_currentFireType == FireType.Burst)
                    StartBurstFire();
            }
            else if (context.performed && _currentFireType == FireType.Automatic )
            {
                
                StartAutomaticFire();
            }
            else if( context.canceled)
            {
              
                 StopAutomaticFire();
            }
            
            VandullLogger.Log(context.started  + " " + context.performed + " " + context.canceled);
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
            if (!CanFire) return;

            PerformShot();
        }

        private void StartAutomaticFire()
        {
            if (_autoFireCoroutine != null) return;
            if (!CanFire) return;

            _autoFireCoroutine = _behaviour.StartCoroutine(AutomaticFireRoutine());
        }

        private void StopAutomaticFire()
        {
            if (_autoFireCoroutine == null) return;
            _behaviour.StopCoroutine(_autoFireCoroutine);
            _autoFireCoroutine = null;
        }

        private void StartBurstFire()
        {
            if (_burstFireCoroutine != null) return;
            if (!CanFire) return;

            _burstFireCoroutine = _behaviour.StartCoroutine(BurstFireRoutine());
        }

        private void StopAllFiring()
        {
            StopAutomaticFire();

            if (_burstFireCoroutine != null)
            {
               _behaviour.StopCoroutine(_burstFireCoroutine);
                _burstFireCoroutine = null;
            }
        }

    

        private IEnumerator AutomaticFireRoutine()
        {
            while (CanFire)
            {
             
                    PerformShot();
                yield return new WaitForSeconds(_config.firingSettings.fireRate);

              

                yield return null;
            }

            _autoFireCoroutine = null;
  
        }

        public void Update()
        {
    
            if (_currentFireType == FireType.Automatic && CanFire && _fireButtonHeld) 
                PerformShot();
        }
        
        private IEnumerator BurstFireRoutine()
        {
           

            for (int i = 0; i < _burstCount && !_ammoSystem.IsCurrentMagazineEmpty; i++)
            {
                PerformShot();

                if (i < _burstCount - 1) // Don't wait after the last shot
                {
                    yield return new WaitForSeconds(_burstDelay);
                }
            }

            yield return new WaitForSeconds(_config.firingSettings.fireRate - _burstDelay);

        
            _burstFireCoroutine = null;
        }

        private void PerformShot()
        {
            _ammoSystem.ConsumeAmmo();
            PerformRaycast();
            _recoilSystem.ApplyRecoil();
            
            OnFired?.Invoke(_muzzlePoint + _transform.position, _config.damageSettings.damage);
        }

        private void PerformRaycast()
        {
            var startPoint = _transform.TransformPoint(_muzzlePoint);
            var hitCount = Physics.RaycastNonAlloc(
                _transform.TransformPoint(_muzzlePoint),
                _transform.forward,
                _hitResults,
                _config.damageSettings.range);

            if (hitCount <= 0)
            {
                FireTrail(startPoint,
                     _transform.forward * _config.damageSettings.range,
                    new RaycastHit());
            

                return;
            }


            ProcessHits(hitCount, startPoint);
        }

        private void FireTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
        {
            _behaviour.StartCoroutine(_trailSystem.SpawnTrail(startPoint, endPoint, hit));
        }
        
        private void ProcessHits(int hitCount, Vector3 startPoint)
        {
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitResults[i];
                FireTrail(startPoint, hit.point, hit);
               
                if (hit.collider == null) continue;

                Debug.DrawLine(_transform.TransformPoint(_muzzlePoint), hit.point, Color.yellow, 20f);

                VandullLogger.Log($"Hit {hit.collider.name} at distance {hit.distance}");
            }
        }
       
    }
}