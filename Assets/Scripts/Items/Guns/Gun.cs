using System.Collections.Generic;
using Attributes;
using General;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Items.Guns
{
  
    [CreateAssetMenu( fileName = "New Gun", menuName = "Items/Gun")]
    public class Gun : Item
    {
        [Header("Gun Components")]
      
        private Transform _recoilTransform;
        [SerializeField] private GunConfig gunConfig;
        [SerializeField] private TrailConfig trailConfig;
        
        [Header("Events")]
        public UnityEvent<Vector3, float> onFired;
        public UnityEvent onReloadStarted;
        public UnityEvent onReloadCompleted;
        public UnityEvent onAmmoChanged;

        [Header("Systems")]
        private FireSystem _fireSystem;
        private IAmmoSystem _ammoSystem;
        private IAimingSystem _aimingSystem;
        private TrailSystem _trailSystem;
        private RecoilSystem _recoilSystem;

        

        private bool _firePressed;
        private bool _aimToggled;


  
      

        #region Unity Lifecycle

        protected override void Use(InputAction.CallbackContext context)
        {
            if(!IsEquipped ) return;
            
            _fireSystem.Fire(context);
    
        }


        public override void Spawn(MonoBehaviour monoBehaviour)
        {
            base.Spawn(monoBehaviour);
            InitializeSystems();
        }

         protected override void OnUpdate()
        {
            _recoilSystem.Update();
            _aimingSystem.Update();
            _fireSystem.Update();
    
        }


        public void OnDrawGizmos()
        {
            DrawDebugGizmos();
        }

        #endregion

        #region Initialization


        private void InitializeSystems()
        {
            _recoilTransform = GameObject.FindWithTag("RecoilTransform").transform;
            
            _trailSystem = new TrailSystem(trailConfig);
            _ammoSystem = new AmmoSystem(gunConfig, MonoBehaviour);
            _aimingSystem = new AimingSystem( gunConfig, ItemInstance.transform);
            _recoilSystem = new RecoilSystem(gunConfig, _aimingSystem, _recoilTransform,  ItemInstance.transform, MonoBehaviour);
            _fireSystem = new FireSystem(ItemInstance.transform, gunConfig,MonoBehaviour,_recoilSystem, _ammoSystem, _trailSystem);
          
          
            _fireSystem.OnFired += HandleFired;
            _ammoSystem.OnAmmoChanged += () => onAmmoChanged?.Invoke();
            _ammoSystem.OnReloadStarted += () => onReloadStarted?.Invoke();
            _ammoSystem.OnReloadCompleted += () => onReloadCompleted?.Invoke();
   
            
            InputManager = ItemInstance.GetComponentInParent<InputManager>();
            InputManager.InputActions.Player.Aim.started += OnAim;
            InputManager.InputActions.Player.Aim.performed += OnAim;
            InputManager.InputActions.Player.Aim.canceled += OnAim;
            InputManager.InputActions.Player.Reload.started += OnReload;

            InputManager.InputActions.Player.SwitchFireMode.started += OnFireModeSwitch;
         
            StartAiming();
            StopAiming();
            
            ItemInstance.transform.localPosition = gunConfig.aimSettings.hipFirePoint;
            
           
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
                StartReload();
        }

        #endregion

        #region Public Interface
        
        public bool CanFire=> _fireSystem.CanFire && !_ammoSystem.IsCurrentMagazineEmpty;
        public bool IsAiming => _aimingSystem.IsAiming;
        public bool IsReloading => _ammoSystem.IsReloading;
        
        public void StartReload()
        {
            _ammoSystem.StartReload();
         
        }

        public void StartAiming() => _aimingSystem.StartAiming();
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
            var muzzleWorldPosition = ItemInstance.transform.TransformPoint( gunConfig.aimSettings.muzzlePoint);
            Gizmos.DrawRay(muzzleWorldPosition, ItemInstance.transform.forward * gunConfig.damageSettings.range);
           // Gizmos.DrawSphere( muzzleWorldPosition, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(ItemInstance.transform.parent.TransformPoint(gunConfig.aimSettings.adsPosition), 0.1f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(ItemInstance.transform.parent.TransformPoint( gunConfig.aimSettings.hipFirePoint), 0.1f);
        }

        #endregion

        


        public void OnFireModeSwitch(InputAction.CallbackContext context)
        {
            if (context.started )
            {
                _fireSystem?.CycleFireMode();
            }
        }
        
        public FireType GetCurrentFireMode()
        {
            return _fireSystem?.CurrentFireType ?? FireType.SemiAutomatic;
        }

        public void SetFireMode(FireType fireType)
        {
            _fireSystem?.SetFireMode(fireType);
        }

        public void CycleFireMode()
        {
            _fireSystem.CycleFireMode();
        }

        public IReadOnlyList<FireType> GetAvailableFireModes()
        {
            return gunConfig.fireModeSettings.availableFireModes;
        }
    }
    
}