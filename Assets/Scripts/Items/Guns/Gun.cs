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
        [SerializeField, ScriptableObjectDropdown] private TrailConfig trailConfig;
        [SerializeField, Required] private GameObject magazinePrefab;
        
        [Header("Events")]
        public UnityEvent<Vector3, float> onFired;
        public UnityEvent onReloadStarted;
        public UnityEvent onReloadCompleted;
        public UnityEvent onAmmoChanged;

        private Transform _muzzleTransform;
        

        private bool _firePressed;
        private bool _aimToggled;
      

        #region Unity Lifecycle

        protected override void Use(InputAction.CallbackContext context)
        {
            if(!IsEquipped ) return;
            
            FireSystem.Fire(context);
            
        }


        public override void Spawn(MonoBehaviour monoBehaviour)
        {
            base.Spawn(monoBehaviour);
            InitializeSystems();
        }

         protected override void OnUpdate()
        {
            RecoilSystem.Update();
            AimingSystem.Update();
            FireSystem.Update();
 
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
            
            _muzzleTransform = new GameObject("Muzzle").transform;
            _muzzleTransform.SetParent(ItemInstance.transform);
            _muzzleTransform.localPosition = gunConfig.aimSettings.muzzlePoint;
            _muzzleTransform.localRotation = Quaternion.identity;
            
            
            TrailSystem = new TrailSystem(trailConfig);
            AmmoSystem = new AmmoSystem(gunConfig, MonoBehaviour, ItemInstance.transform, magazinePrefab);
            AimingSystem = new AimingSystem( gunConfig, ItemInstance.transform);
            RecoilSystem = new RecoilSystem(gunConfig, AimingSystem, _recoilTransform,  ItemInstance.transform, MonoBehaviour);
            FireSystem = new FireSystem(ItemInstance.transform, gunConfig, _muzzleTransform, MonoBehaviour,RecoilSystem, AmmoSystem, TrailSystem);
          
          
            FireSystem.OnFired += HandleFired;
            AmmoSystem.OnAmmoChanged += () => onAmmoChanged?.Invoke();
            AmmoSystem.OnReloadStarted += () => onReloadStarted?.Invoke();
            AmmoSystem.OnReloadCompleted += () => onReloadCompleted?.Invoke();
   
            
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
        
        public bool CanFire=> FireSystem.CanFire && !AmmoSystem.IsCurrentMagazineEmpty;
        public bool IsAiming => AimingSystem.IsAiming;
        public bool IsReloading => AmmoSystem.IsReloading;
        public RecoilSystem RecoilSystem { get; private set; }

        public AmmoSystem AmmoSystem { get; private set; }

        [field: Header("Systems")]
        public FireSystem FireSystem { get; private set; }

        public AimingSystem AimingSystem { get; private set; }

        public TrailSystem TrailSystem { get; private set; }


        public void StartReload()
        {
            AmmoSystem.StartReload();
         
        }

        public void StartAiming() => AimingSystem.StartAiming();
        public void StopAiming() => AimingSystem.StopAiming();

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
            Gizmos.DrawRay(_muzzleTransform.position, _muzzleTransform.transform.forward * gunConfig.damageSettings.range);
            
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(ItemInstance.transform.parent.TransformPoint(gunConfig.aimSettings.adsPosition), 0.01f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(ItemInstance.transform.parent.TransformPoint( gunConfig.aimSettings.hipFirePoint), 0.01f);
            
            Gizmos.color = Color.azure; 
            Gizmos.DrawSphere(ItemInstance.transform.TransformPoint( gunConfig.ammoSettings.magazinePosition), 0.01f);
            
        }

        #endregion

        


        public void OnFireModeSwitch(InputAction.CallbackContext context)
        {
            if (context.started )
            {
                FireSystem?.CycleFireMode();
            }
        }
        
        public FireType GetCurrentFireMode()
        {
            return FireSystem?.CurrentFireType ?? FireType.SemiAutomatic;
        }

        public void SetFireMode(FireType fireType)
        {
            FireSystem?.SetFireMode(fireType);
        }

        public void CycleFireMode()
        {
            FireSystem.CycleFireMode();
        }

        public IReadOnlyList<FireType> GetAvailableFireModes()
        {
            return gunConfig.fireModeSettings.availableFireModes;
        }
    }
    
}