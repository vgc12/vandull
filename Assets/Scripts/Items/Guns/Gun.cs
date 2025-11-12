using System.Collections.Generic;
using Attributes;
using EventBus;
using General.Extensions;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UnityEngine;

namespace Items.Guns
{
    /// <summary>
    ///     Main gun component that manages all gun-related systems including firing, aiming, ammo, recoil, and trails.
    ///     Inherits from Item to integrate with the inventory system.
    /// </summary>
    public sealed class Gun : Item
    {
        #region Components

        /// <summary>
        ///     Initializer responsible for creating and wiring up all gun systems.
        /// </summary>
        [SerializeField, Required, Tooltip("Initializer responsible for creating and wiring up all gun systems")]  
        private GunInitializer initializer;

        #endregion

        #region Private Fields

        /// <summary>
        ///     Cached reference to the player's recoil transform.
        /// </summary>
        private Transform _recoilTransform;

        #endregion

        #region Reload

        /// <summary>
        ///     Initiates a reload sequence, stopping any active aim.
        /// </summary>
        /// <param name="quickReload">If true, performs a quick reload animation.</param>
        public void StartReload(bool quickReload)
        {
            AimingSystem.StopAiming();
            if (quickReload)
            {
                AmmoSystem.StartQuickReload();
            }

            AmmoSystem.StartReload();
        }

        #endregion

        public void CheckAmmo() { AmmoSystem.CheckAmmo(); }

        #region Settings

        /// <summary>
        ///     Configuration for available fire modes (semi-auto, burst, full-auto).
        /// </summary>
        [SerializeField, Required, Tooltip("Configuration for available fire modes (semi-auto, burst, full-auto)")]  
        public FireModeSettings fireModeSettings;

        /// <summary>
        ///     General firing behavior settings including fire rate and spread.
        /// </summary>
        [SerializeField, Required, Tooltip("General firing behavior settings including fire rate and spread")]  
        public FiringSettings firingSettings;

        /// <summary>
        ///     Aiming down sights configuration including FOV and speed.
        /// </summary>
        [SerializeField, Required, Tooltip("Aiming down sights configuration including FOV and speed")]  
        public AimSettings aimSettings;

        /// <summary>
        ///     Damage values and falloff configuration.
        /// </summary>
        [SerializeField, Required, Tooltip("Damage values and falloff configuration")]  
        public DamageSettings damageSettings;

        /// <summary>
        ///     Ammunition capacity, reload times, and magazine settings.
        /// </summary>
        [SerializeField, Required, Tooltip("Ammunition capacity, reload times, and magazine settings")]  
        public AmmoSettings ammoSettings;

        /// <summary>
        ///     Recoil pattern and intensity configuration.
        /// </summary>
        [SerializeField, Required, Tooltip("Recoil pattern and intensity configuration")]  
        public RecoilSettings recoilSettings;

        /// <summary>
        ///     Visual bullet trail settings including color and lifetime.
        /// </summary>
        [SerializeField, Required, Tooltip("Visual bullet trail settings including color and lifetime")]  
        public TrailSettings trailSettings;

        /// <summary>
        ///     Audio clips for firing, reloading, and other gun sounds.
        /// </summary>
        [SerializeField, Required, Tooltip("Audio clips for firing, reloading, and other gun sounds")]  
        public AudioSettings audioSettings;

        #endregion

        #region Transform References

        /// <summary>
        ///     Position where the magazine model attaches during reload animations.
        /// </summary>
        [SerializeField, Required, Tooltip("Position where the magazine model attaches during reload animations")]  
        public Transform magazinePosition;

        /// <summary>
        ///     Transform used for gun positioning when firing from the hip.
        /// </summary>
        [SerializeField, Required, Tooltip("Transform used for gun positioning when firing from the hip")]  
        public Transform hipFireTransform;

        /// <summary>
        ///     Transform used for gun positioning when aiming down sights.
        /// </summary>
        [SerializeField, Required, Tooltip("Transform used for gun positioning when aiming down sights")]  
        public Transform aimTransform;

        /// <summary>
        ///     Position where bullets spawn and muzzle flash appears.
        /// </summary>
        [SerializeField, Required, Tooltip("Position where bullets spawn and muzzle flash appears")]  
        public Transform muzzleTransform;

        #endregion

        #region Animations

        /// <summary>
        ///     Standard reload animation for this weapon.
        /// </summary>
        [SerializeField, Required, Tooltip("Standard reload animation for this weapon")]  
        public ItemAnimation reloadAnimation;

        /// <summary>
        ///     Faster reload animation (may retain ammo in magazine).
        /// </summary>
        [SerializeField, Required, Tooltip("Faster reload animation (may retain ammo in magazine)")]  
        public ItemAnimation quickReloadAnimation;

        [SerializeField, Required, Tooltip("Animation played when checking ammo")]  
        public ItemAnimation checkingAmmoAnimation;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the transform used for applying recoil to the camera/player view.
        ///     Lazily finds and caches the PlayerRecoilObject on first access.
        /// </summary>
        public Transform RecoilTransform
        {
            get
            {
                _recoilTransform ??= FindFirstObjectByType<PlayerRecoilObject>().transform;
                return _recoilTransform;
            }
        }

        /// <summary>
        ///     System responsible for handling aim down sights transitions and positioning.
        /// </summary>
        public IAimingSystem AimingSystem { get; private set; }

        /// <summary>
        ///     System responsible for ammunition tracking, reloading, and magazine management.
        /// </summary>
        public IAmmoSystem AmmoSystem { get; private set; }

        /// <summary>
        ///     System responsible for applying recoil patterns to the player's view.
        /// </summary>
        public IRecoilSystem RecoilSystem { get; private set; }

        /// <summary>
        ///     System responsible for rendering visual bullet trails.
        /// </summary>
        public ITrailSystem TrailSystem { get; private set; }

        /// <summary>
        ///     System responsible for managing fire modes and executing fire commands.
        /// </summary>
        public IFireModeSystem FireModeSystem { get; private set; }

        /// <summary>
        ///     Gets whether the player is currently aiming down sights.
        /// </summary>
        public bool IsAiming => AimingSystem.IsAiming;

        public override bool CanBeSwappedFrom => !IsReloading && !IsCheckingAmmo;

        /// <summary>
        ///     Gets whether the gun is currently in a reload animation.
        /// </summary>
        public bool IsReloading => AmmoSystem.IsReloading;

        public bool IsCheckingAmmo => AmmoSystem.IsCheckingAmmo;

        #endregion

        #region Unity Lifecycle

        /// <summary>
        ///     Initializes all gun systems and performs initial aim state setup.
        /// </summary>
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

            // Initialize aim state
            AimingSystem.StartAiming();
            AimingSystem.StopAiming();
        }

        /// <summary>
        ///     Cleans up event subscriptions when the gun is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            // Unsubscribe from fire mode events
            if (FireModeSystem?.AvailableFireModes != null)
            {
                foreach (var fireMode in FireModeSystem.AvailableFireModes)
                {
                    fireMode.OnShotFired = null;
                }
            }

            // Unsubscribe from ammo events
            if (AmmoSystem != null)
            {
                AmmoSystem.OnOutOfAmmo = null;
                AmmoSystem.OnReloadComplete = null;
            }
        }

        /// <summary>
        ///     Updates all gun systems each frame.
        /// </summary>
        protected override void OnUpdate()
        {
            RecoilSystem?.Update();
            AimingSystem?.Update();
            FireModeSystem?.Update();
            AmmoSystem?.Update();
            TrailSystem?.Update();
        }

        /// <summary>
        ///     This method is called when the gun is used (fired).
        /// </summary>
        public override void Use() { Fire(); }

        public override void StopUse() { StopFiring(); }

        #endregion

        #region Aiming

        /// <summary>
        ///     Starts the aim down sights transition if not reloading or unequipped.
        /// </summary>
        public void StartAiming()
        {
            if (IsReloading || !IsEquipped)
            {
                return;
            }

            AimingSystem.StartAiming();
        }

        /// <summary>
        ///     Stops the aim down sights transition if the gun is equipped.
        /// </summary>
        public void StopAiming()
        {
            if (!IsEquipped)
            {
                return;
            }

            AimingSystem.StopAiming();
        }

        #endregion

        #region Fire Modes

        /// <summary>
        ///     Gets a read-only list of available fire modes for this weapon.
        /// </summary>
        /// <returns>List of available fire types (semi-auto, burst, full-auto, etc.).</returns>
        public IReadOnlyList<FireType> GetAvailableFireModes() => fireModeSettings.availableFireModes;

        /// <summary>
        ///     Cycles to the next available fire mode.
        /// </summary>
        public void CycleFireMode() { FireModeSystem.CycleFireMode(); }

        #endregion

        #region Firing

        private void Fire() { FireModeSystem.CurrentFireSystem.Fire(); }

        /// <summary>
        ///     Immediately stops all firing activity.
        /// </summary>
        private void StopFiring() { FireModeSystem.CurrentFireSystem.StopFire(); }

        #endregion

        #region Item Management

        /// <summary>
        ///     Drops the gun into the world, adding physics components and ejecting the magazine.
        /// </summary>
        public void Drop()
        {
            // Add physics components
            transform.GetOrAdd<Rigidbody>();

            // Ensure there's a collider
            var colliderCount = transform.GetComponentsInChildren<Collider>();
            if (colliderCount.Length == 0)
            {
                transform.GetOrAdd<BoxCollider>();
            }

            // Detach from parent
            transform.SetParent(null);

            // Drop the magazine
            AmmoSystem.DropMagazine();
        }

        /// <summary>
        ///     Called when the gun is equipped by the player.
        ///     Resets position and stops any active states.
        /// </summary>
        public override void Equip()
        {
            gameObject.SetActive(true);
            IsEquipped = true;
            AimingSystem.ResetPosition();
            if (ItemAnimationSystem != null && holdingItemAnimation != null && Owner == OwnerStatus.Player)
            {
                ItemAnimationSystem.PlayAnimation(holdingItemAnimation);
            }

            if (AimingSystem != null)
            {
                StopAiming();
            }

            if (FireModeSystem != null)
            {
                StopFiring();
            }

            EventBus<ItemEquippedEvent>.Raise(new ItemEquippedEvent(this));
        }

        /// <summary>
        ///     Called when the gun is unequipped by the player.
        ///     Resets position and stops any active states.
        /// </summary>
        public override void UnEquip()
        {
            AimingSystem.ResetPosition();

            if (AimingSystem != null)
            {
                StopAiming();
            }

            if (FireModeSystem != null)
            {
                StopFiring();
            }

            base.UnEquip();
        }

        #endregion
    }
}