using System.Collections.Generic;
using Attributes;
using EventBus;
using Items.Guns.Firing;
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

        
        public IAimingSystem AimingSystem { get; private set; }
        public IAmmoSystem AmmoSystem { get; private set; }
        public IRecoilSystem RecoilSystem { get; private set; }
        public ITrailSystem TrailSystem { get; private set; }
        
        public IFireModeSystem FireModeSystem { get; private set; }

      

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
            FireModeSystem = _gunSystems.FireModeSystem;

           
            StartAiming();
            StopAiming();
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
        

        public void OnShotFired(ShotFiredEvent shot)
        {
            TrailSystem?.SpawnTrail(shot.ShootPoint, shot.EndPoint, shot.Hit);
            RecoilSystem?.ApplyRecoil();
        }

        public void OnOutOfAmmo()
        {
            FireModeSystem.CurrentFireSystem?.OnOutOfAmmo();
        }

        public void CycleFireMode()
        {
            FireModeSystem.CycleFireMode();
        }
    }
}
    
}

namespace Items.Guns
{
    public interface IFireModeSystem : IGunSystem
    {

        IFireSystem CurrentFireSystem { get; }
        IReadOnlyList<IFireSystem> AvailableFireModes { get; }

        void CycleFireMode();
     
    }
}