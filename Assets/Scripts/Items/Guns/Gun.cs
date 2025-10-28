using System.Collections.Generic;
using Attributes;
using General.Extensions;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UnityEngine;

namespace Items.Guns
{
    public sealed class Gun : Item
    {
        [Required] public FireModeSettings fireModeSettings;
        [Required] public FiringSettings firingSettings;
        [Required] public AimSettings aimSettings;
        [Required] public DamageSettings damageSettings;
        [Required] public AmmoSettings ammoSettings;
        [Required] public RecoilSettings recoilSettings;
        [Required] public TrailSettings trailSettings;
        [Required] public AudioSettings audioSettings;


        [SerializeField] [Required] private GunInitializer initializer;

        [Required] public Transform magazinePosition;

        [Required] public Transform hipFireTransform;

        [Required] public Transform aimTransform;

        [Required] public Transform muzzleTransform;
        [Required] public ItemAnimation reloadAnimation;
        [Required] public ItemAnimation quickReloadAnimation;

        private Transform _recoilTransform;


        public Transform RecoilTransform
        {
            get
            {
                _recoilTransform ??= FindFirstObjectByType<PlayerRecoilObject>().transform;
                return _recoilTransform;
            }
        }


        public IAimingSystem AimingSystem { get; private set; }
        public IAmmoSystem AmmoSystem { get; private set; }
        public IRecoilSystem RecoilSystem { get; private set; }
        public ITrailSystem TrailSystem { get; private set; }
        public IFireModeSystem FireModeSystem { get; private set; }

        

        public bool IsAiming => AimingSystem.IsAiming;
        public bool IsReloading => AmmoSystem.IsReloading;

        private void Awake()
        {
            var systems = initializer.CreateGunSystems(this);

            RecoilSystem = systems.RecoilSystem;
            AmmoSystem = systems.AmmoSystem;
            AimingSystem = systems.AimingSystem;
            RecoilSystem = systems.RecoilSystem;
            TrailSystem = systems.TrailSystem;
            FireModeSystem = systems.FireModeSystem;
            ItemAnimationSystem = systems.ItemAnimationSystem;

            AimingSystem.StartAiming();
            AimingSystem.StopAiming();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var fireMode in FireModeSystem?.AvailableFireModes) fireMode.OnShotFired = null;
            AmmoSystem.OnOutOfAmmo = null;
            AmmoSystem.OnReloadComplete = null;
        }


        protected override void OnUpdate()
        {
            RecoilSystem?.Update();
            AimingSystem?.Update();
            FireModeSystem?.Update();
            AmmoSystem?.Update();
            TrailSystem?.Update();
        }

        public void StartReload(bool quickReload)
        {
            AimingSystem.StopAiming();
            if (quickReload) AmmoSystem.StartQuickReload();
            AmmoSystem.StartReload();
        }

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


        public IReadOnlyList<FireType> GetAvailableFireModes()
        {
            return fireModeSettings.availableFireModes;
        }


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
            transform.GetOrAdd<Rigidbody>();

            var colliderCount = transform.GetComponentsInChildren<Collider>();
            if (colliderCount.Length == 0)
                transform.GetOrAdd<BoxCollider>();

            transform.SetParent(null);

            AmmoSystem.DropMagazine();
        }

        public void StopFiring()
        {
            FireModeSystem.CurrentFireSystem.StopFire();
        }

        public override void Equip()
        {
            base.Equip();
            AimingSystem.ResetPosition();
            if (AimingSystem != null) StopAiming();

            if (FireModeSystem != null) StopFiring();
        }

        public override void UnEquip()
        {
            AimingSystem.ResetPosition();
            if (AimingSystem != null) StopAiming();

            if (FireModeSystem != null) StopFiring();


            base.UnEquip();
        }
    }
}