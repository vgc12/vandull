using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using EventBus;
using General;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using NPC;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Items.Guns
{
    public class FireSystem : IFireSystem
    {
      
        public event Action OnFireModeChanged; 

        private readonly GunConfig _config;
        private readonly Transform _muzzleTransform;

        private readonly Transform _transform;
        private readonly MonoBehaviour _behaviour;
        private readonly RaycastHit[] _hitResults = new RaycastHit[10];

       
        private Coroutine _cooldownCoroutine;
        private Coroutine _autoFireCoroutine;
        private Coroutine _burstFireCoroutine;

  
        private FireType _currentFireType;
        private readonly List<FireType> _availableFireModes;
        private int _currentFireModeIndex;
   
        // Burst fire properties
        private readonly int _burstCount;
        private readonly float _burstDelay;

        public FireType CurrentFireType => _currentFireType;
        public IReadOnlyList<FireType> AvailableFireModes => _availableFireModes;


        
        private float _lastFireTime;
        
       
        private bool _fireButtonHeld;
        
        public Vector3 StartPoint { get; private set; }
        
        public event Action<ShotFiredEvent> OnShotFired;
        
        
        
        private bool _isOutOfAmmo;
        
        public FireSystem(
            GunConfig config,
            Transform gunTransform,
            MonoBehaviour behaviour,
            List<Action<ShotFiredEvent>> onShotFired = null
           )
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            var go = new GameObject("MuzzlePoint");
            _muzzleTransform = go.transform;
            _muzzleTransform.SetParent(gunTransform, false);
            _muzzleTransform.localPosition = _config.firingSettings.muzzlePoint;
            
            _transform = gunTransform ?? throw new ArgumentNullException(nameof(gunTransform));
   
            _behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));
          

            _availableFireModes = new List<FireType>(_config.fireModeSettings.availableFireModes);
            _currentFireType = _availableFireModes.Count > 0 ? _availableFireModes[0] : FireType.SemiAutomatic;
            _currentFireModeIndex = 0;
            _burstCount = _config.firingSettings.burstCount;
            _burstDelay = _config.firingSettings.burstDelay;
            if (onShotFired == null) return;
            foreach (var action in onShotFired)
            {
                OnShotFired += action;
            }
        }
        
        

        public bool CanFire => Time.time > _lastFireTime + _config.firingSettings.fireRate &&  !_isOutOfAmmo ;

        
        public void OnOutOfAmmo()
        {
            _isOutOfAmmo = true;
        }
        
        public void OnReloadEnded()
        {
            _isOutOfAmmo = false;
        }
        
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
            
//            VandullLogger.Log(context.started  + " " + context.performed + " " + context.canceled);
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

            _burstFireCoroutine = _behaviour.StartCoroutine(FireBurst());
        }

        private void StopAllFiring()
        {
            StopAutomaticFire();

            if (_burstFireCoroutine == null) return;
            _behaviour.StopCoroutine(_burstFireCoroutine);
            _burstFireCoroutine = null;
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
        
        private IEnumerator FireBurst()
        {
            
            for (int i = 0; i < _burstCount && !_isOutOfAmmo; i++)
            {
                PerformShot();

                if (i < _burstCount - 1)
                {
                    yield return new WaitForSeconds(_burstDelay);
                }
            }

            yield return new WaitForSeconds(_config.firingSettings.fireRate - _burstDelay);

        
            _burstFireCoroutine = null;
        }

        private void PerformShot()
        {
            
            PerformRaycast();
            
            
        
        }

        private void PerformRaycast()
        {
            var startPoint = _muzzleTransform.position;
            var endPoint = startPoint + (_muzzleTransform.forward * _config.damageSettings.range);
            
            
            var hitCount = Physics.RaycastNonAlloc(
                startPoint,
               _muzzleTransform.forward ,
                _hitResults,
                _config.damageSettings.range,~LayerMask.GetMask("Ignore Raycast","Player"));
  
            if (hitCount <= 0)
            {
               
                OnShotFired?.Invoke( new ShotFiredEvent(startPoint, endPoint, new RaycastHit() ));

                return;
            }

           

            ProcessHits(hitCount, startPoint);
        }

        
        
        private void ProcessHits(int hitCount, Vector3 startPoint)
        {
            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hitResults[i];
             
               
                if (hit.collider == null) continue;
                OnShotFired?.Invoke( new ShotFiredEvent(startPoint, hit.point, hit ));
                
                VandullLogger.Log("Hit: " + hit.collider.name);

                if (!hit.collider.TryGetComponent<BodyPart>(out var bodyPart)) continue;
                
                if (!hit.collider.transform.root.TryGetComponent<IDamageable>(out var damageable)) continue;
                
                damageable.TakeDamage(_config.damageSettings.damage * bodyPart.damageMultiplier);
                EventBus<GunFiredEvent>.Raise(new GunFiredEvent(_transform.position, _config.damageSettings.damage));

            }
        }
       
    }
}

[RequireComponent(typeof(Collider))]
public class BodyPart : MonoBehaviour
{
    public float damageMultiplier = 1f;
}