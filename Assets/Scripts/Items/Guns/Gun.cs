using System.Collections.Generic;
using Attributes;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Items.Guns
{
  
    public class Gun : Item
    {
        [Header("Gun Components")]
        [SerializeField] private Vector3 muzzlePoint;
        [SerializeField] private Vector3 adsPosition;
        [SerializeField] private Vector3 hipFirePoint;
        [SerializeField, Required, ScriptableObjectDropdown] private GunConfig gunConfig;
        
        [Header("Events")]
        public UnityEvent<Vector3, float> onFired;
        public UnityEvent onReloadStarted;
        public UnityEvent onReloadCompleted;
        public UnityEvent onAmmoChanged;

        
        private FireSystem _fireSystem;
        private IAmmoSystem _ammoSystem;
        private IAimingSystem _aimingSystem;
  

        private bool _firePressed;
        private bool _aimToggled;
        private bool _reloadPressed;

        #region Unity Lifecycle

        protected override void Use(InputAction.CallbackContext context)
        {
            if(!IsEquipped) return;
            _fireSystem.Fire(context);
        }


        protected override void Initialize()
        {
            base.Initialize();
            InitializeSystems();
        }

        private void Update()
        {
     
            _aimingSystem.Update();
            if (_reloadPressed && !IsReloading)
            {
                StartReload();
            }
        }

 

        private void OnDrawGizmos()
        {
            DrawDebugGizmos();
        }

        #endregion

        #region Initialization


        private void InitializeSystems()
        {
            _ammoSystem = new AmmoSystem(gunConfig);
            _fireSystem = new FireSystem(transform, gunConfig, muzzlePoint, _ammoSystem);
            _aimingSystem = new AimingSystem(transform, gunConfig.aimSettings.adsTime);
            
            _fireSystem.OnFired += HandleFired;
            _ammoSystem.OnAmmoChanged += () => onAmmoChanged?.Invoke();
            _ammoSystem.OnReloadStarted += () => onReloadStarted?.Invoke();
            _ammoSystem.OnReloadCompleted += () => onReloadCompleted?.Invoke();
            
            InputManager = GetComponentInParent<InputManager>();
            InputManager.InputActions.Player.Aim.started += OnAim;
            InputManager.InputActions.Player.Aim.performed += OnAim;
            InputManager.InputActions.Player.Aim.canceled += OnAim;
         
            StopAiming();
        }

        #endregion
       

        #region Input Handling

  

        public void OnAim(InputAction.CallbackContext context)
        {
            if(!IsEquipped) return;
            if (context.started)
            {
                _aimToggled = !_aimToggled;
            }

            if (_aimToggled && !IsReloading)
            {
                StartAiming();
            }
            else if (!_aimToggled)
            {
                StopAiming();
            }
        }

        public void OnReload(InputAction.CallbackContext context)
        {
            if(!IsEquipped) return;
            if (context.started)
                _reloadPressed = true;
        }

        #endregion

        #region Public Interface
        
        public bool CanFire() => _fireSystem.CanFire() && !_ammoSystem.IsCurrentMagazineEmpty;
        public bool IsAiming => _aimingSystem.IsAiming;
        public bool IsReloading => _ammoSystem.IsReloading;
        
        public void StartReload()
        {
            _ammoSystem.StartReload();
            _reloadPressed = false;
        }

        public void StartAiming() => _aimingSystem.StartAiming(adsPosition, hipFirePoint);
        public void StopAiming() => _aimingSystem.StopAiming();

        #endregion

        #region Event Handlers

        private void HandleFired(Vector3 position, float damage)
        {
            onFired?.Invoke(position, damage);
        }

        #endregion

        #region Debug

        private void DrawDebugGizmos()
        {
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(muzzlePoint + transform.position, (transform.forward) * gunConfig.damageSettings.range);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + adsPosition, 0.1f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position + hipFirePoint, 0.1f);
        }

        #endregion

        #region Properties for States

        public bool FirePressed => _firePressed;
        public bool AimToggled => _aimToggled;
        public bool ReloadPressed => _reloadPressed;

        #endregion
        
        [Header("Fire Mode Controls")]
        [SerializeField] private bool allowFireModeSwitching = true;
    
        // Add this to your input handling section
        public void OnFireModeSwitch(InputAction.CallbackContext context)
        {
            if (context.started && allowFireModeSwitching)
            {
                _fireSystem?.CycleFireMode();
            }
        }
        
        public FireType GetCurrentFireMode()
        {
            return (_fireSystem as FireSystem)?.CurrentFireType ?? FireType.SemiAutomatic;
        }

        public void SetFireMode(FireType fireType)
        {
            (_fireSystem as FireSystem)?.SetFireMode(fireType);
        }

        public void CycleFireMode()
        {
            _fireSystem.CycleFireMode();
        }

        public IReadOnlyList<FireType> GetAvailableFireModes()
        {
            return gunConfig.availableFireModes;
        }
    }
    
}