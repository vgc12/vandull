using System;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using EventBus;
using Items.Guns.Firing;
using Items.Guns.Items.Guns.Builder;
using Items.Guns.Items.Guns.Dependencies;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using NPC;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public sealed class Gun : Item
    {
        [Header("Gun Components")] 
        public GunConfig gunConfig;
        
        [SerializeField, Required] private GunInitializer initializer;
        
        
        public IAimingSystem AimingSystem { get; private set; }
        public IAmmoSystem AmmoSystem { get; private set; }
        public IRecoilSystem RecoilSystem { get; private set; }
        public ITrailSystem TrailSystem { get; private set; }

        public  IFireModeSystem FireModeSystem { get; private set; }

        public Transform magazinePosition;

        public Transform hipFireTransform;

        public Transform recoilTransform;
        
        public Transform adsTransform;

        public Transform muzzleTranform;

        private void Awake()
        {
            var systems = initializer.CreateGunSystems(this);
            
            RecoilSystem = systems.RecoilSystem;
            AmmoSystem = systems.AmmoSystem;
            AimingSystem = systems.AimingSystem;
            RecoilSystem = systems.RecoilSystem;
            TrailSystem = systems.TrailSystem;
            FireModeSystem = systems.FireModeSystem;
            
            AimingSystem.StartAiming();
            AimingSystem.StopAiming();
            
        }

      

        protected override void OnUpdate()
        {
            RecoilSystem?.Update();
            AimingSystem?.Update();
            FireModeSystem?.Update();
            AmmoSystem?.Update();
            TrailSystem?.Update();
        }


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


        public IReadOnlyList<FireType> GetAvailableFireModes() => gunConfig.fireModeSettings.availableFireModes;


        public void ExecuteSingleShot()
        {
            FireModeSystem.CurrentFireSystem.ExecuteFireCommand(FireCommand.SingleShot);
        }

        public void StartAutomaticFire()
        {
            FireModeSystem.CurrentFireSystem.ExecuteFireCommand(FireCommand.StartAutomaticFire);
        }

        public void StopAutomaticFire()
        {
            FireModeSystem.CurrentFireSystem.ExecuteFireCommand(FireCommand.StopAutomaticFire);
        }

        

        public void CycleFireMode()
        {
            FireModeSystem.CycleFireMode();
        }


        public void Drop()
        {
           var rb = transform.GetOrAddComponent<Rigidbody>();
           
            transform.GetOrAddComponent<MeshCollider>();
            transform.SetParent(null);
            
            AmmoSystem.DropMagazine();
        }
    }
}


namespace Items.Guns
{
    public interface IFireModeSystem : IGunSystem
    {
        IFireSystem CurrentFireSystem { get; }
        IReadOnlyList<IFireSystem> AvailableFireModes { get; }
        
        void SetCurrentFireMode(FireType fireType);

        void CycleFireMode();
    }
}