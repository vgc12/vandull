using System.Collections.Generic;
using Attributes;
using EventBus;
using Items.Guns.Items.Guns.Builder;
using Items.Guns.Items.Guns.Dependencies;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using NPC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Items.Guns
{

    



namespace Items.Guns
{
    public sealed class Gun : Item
    {
        [Header("Gun Components")]
        public GunConfig gunConfig;
        
        private GunSystems _gunSystems;

  
        public IFireSystem FireSystem { get; private set; }
        public IAimingSystem AimingSystem { get; private set; }
        public IAmmoSystem AmmoSystem { get; private set; }
        public IRecoilSystem RecoilSystem { get; private set; }
        public ITrailSystem TrailSystem { get; private set; }

        public override void Use(InputAction.CallbackContext ctx)
        {
            if (!IsEquipped) return;
            FireSystem?.Fire(ctx);
        }

        public void Initialize(GunSystems systems)
        {
            _gunSystems = systems;
            InitializeSystems();
        }

        
        
        private void InitializeSystems()
        {
            TrailSystem = _gunSystems.TrailSystem;
            AmmoSystem = _gunSystems.AmmoSystem;
            AimingSystem = _gunSystems.AimingSystem;
            RecoilSystem = _gunSystems.RecoilSystem;
            FireSystem = _gunSystems.FireSystem;

           
            StartAiming();
            StopAiming();
        }

        protected override void OnUpdate()
        {
            RecoilSystem?.Update();
            AimingSystem?.Update();
            FireSystem?.Update();
            AmmoSystem?.Update();
            TrailSystem?.Update();
        }

        // Public interface remains the same
        public bool CanFire => FireSystem.CanFire && !AmmoSystem.IsCurrentMagazineEmpty;
        public bool IsAiming => AimingSystem.IsAiming;
        public bool IsReloading => AmmoSystem.IsReloading;

        public void StartReload() => AmmoSystem.StartReload();
        public void StartAiming()
        {
            if (IsReloading || !IsEquipped) return;
            AimingSystem.StartAiming();
        }
        public void StopAiming()
        {
            if (!IsEquipped) return;
            AimingSystem.StopAiming();
        }

        public void OnFireModeSwitch(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                FireSystem?.CycleFireMode();
            }
        }

        public FireType GetCurrentFireMode() => FireSystem?.CurrentFireType ?? FireType.SemiAutomatic;
        public void SetFireMode(FireType fireType) => FireSystem?.SetFireMode(fireType);
        public void CycleFireMode() => FireSystem.CycleFireMode();
        public IReadOnlyList<FireType> GetAvailableFireModes() => gunConfig.fireModeSettings.availableFireModes;

        // Debug methods remain the same
        public void OnDrawGizmos()
        {
            DrawDebugGizmos();
        }

        private void DrawDebugGizmos()
        {
            /*
            if (muzzleTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(muzzleTransform.position, muzzleTransform.transform.forward * gunConfig.damageSettings.range);
            }
            
            Gizmos.color = Color.azure; 
            Gizmos.DrawSphere(transform.TransformPoint(gunConfig.ammoSettings.magazinePosition), 0.01f);
            
            if (hipFireTransform != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(hipFireTransform.position, 0.01f);
            }
            
            if (adsTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(adsTransform.position, 0.01f);
            }
            */
        }

        public void OnShotFired(ShotFiredEvent shot)
        {
            TrailSystem?.SpawnTrail(shot.ShootPoint, shot.EndPoint, shot.Hit);
            RecoilSystem?.ApplyRecoil();
        }

        public void OnOutOfAmmo()
        {
            FireSystem?.OnOutOfAmmo();
        }
    }
}
    
}