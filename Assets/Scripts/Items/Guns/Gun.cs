using System;
using System.Collections.Generic;
using Attributes;
using EventBus;
using General;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public sealed class Gun : Item
    {
        [Header("Gun Components")]
      
      
        public GunConfig gunConfig;
        
        [SerializeField, ScriptableObjectDropdown] public TrailConfig trailConfig;
        
        [SerializeField, Required] public GameObject magazinePrefab;
        
        
        public Transform hipFireTransform;
        public Transform adsTransform;
        public Transform recoilTransform;
        public Transform muzzleTransform;
        
        private EventBinding<GunHandlerInitializedEvent> _gunHandlerInitializedEventBinding;
        
        #region Unity Lifecycle

        protected override void Use(InputAction.CallbackContext context)
        {
            if(!IsEquipped ) return;
            
            FireSystem.Fire(context);
            
        }


        public override void Awake()
        {
            base.Awake();    
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
            
            muzzleTransform = new GameObject("Muzzle").transform;
            muzzleTransform.SetParent(transform);
            muzzleTransform.localPosition = gunConfig.firingSettings.muzzlePoint;
            muzzleTransform.localRotation = Quaternion.identity;
            
            
            TrailSystem = new TrailSystem(trailConfig);
            AmmoSystem = new AmmoSystem(this);
            AimingSystem = new AimingSystem( this);
            RecoilSystem = new RecoilSystem(this);
            FireSystem = new FireSystem(this);


            

            StartAiming();
            StopAiming();

            

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

        public void StartAiming()
        {
            if(IsReloading || !IsEquipped) return;
            AimingSystem.StartAiming();
        }

        public void StopAiming()
        {
            if(!IsEquipped) return;
            AimingSystem.StopAiming();
        }

        #endregion


        #region Debug

        private void DrawDebugGizmos()
        {
            
            Gizmos.color = Color.red;
            Gizmos.DrawRay(muzzleTransform.position, muzzleTransform.transform.forward * gunConfig.damageSettings.range);
            
            Gizmos.color = Color.azure; 
            Gizmos.DrawSphere(transform.TransformPoint( gunConfig.ammoSettings.magazinePosition), 0.01f);
            
            if( hipFireTransform == null || adsTransform == null) return;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(adsTransform.position, 0.01f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(hipFireTransform.position, 0.01f);
            

            
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