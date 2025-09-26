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
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Items.Guns
{
    public sealed class Gun : Item
    {
        [Header("Gun Components")] public GunConfig gunConfig;

        public IAimingSystem AimingSystem { get; private set; }
        public IAmmoSystem AmmoSystem { get; private set; }
        public IRecoilSystem RecoilSystem { get; private set; }
        public ITrailSystem TrailSystem { get; private set; }

        public  IFireModeSystem FireModeSystem { get; private set; }


        private void Initialize()
        {
            if (HipFireTransform)
            {
                transform.position = HipFireTransform.position;
            }
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

        public abstract class FireModeFactory
        {
            public static IFireSystem CreateFireMode(
                FireType fireType,
                GunConfig config,
                Transform gunTransform,
                MonoBehaviour behaviour,
                Transform muzzleTransform,
                List<Action<ShotFiredEvent>> shotHandlers = null)
            {
                IFireSystem fireSystem = fireType switch
                {
                    FireType.SemiAutomatic => new SemiAutoFireMode(config, gunTransform, behaviour, muzzleTransform ),
                    FireType.Automatic => new AutomaticFireMode(config, gunTransform, behaviour, muzzleTransform),
                    FireType.Burst => new BurstFireMode(config, gunTransform, behaviour, muzzleTransform),
                    _ => throw new ArgumentException($"Unsupported fire type: {fireType}")
                };

                if (shotHandlers == null) return fireSystem;
                foreach (var handler in shotHandlers)
                {
                    fireSystem.OnShotFired += handler;
                }

                return fireSystem;
            }
        }

        public class Initializer : IGunSystemsBuilder
        {
            private readonly List<Action<ShotFiredEvent>> _onShotFired = new List<Action<ShotFiredEvent>>();
            private readonly List<Action> _onAmmoOut = new();

            // Fire mode configuration
            private List<FireType> _enabledFireModes = new();
            private readonly Dictionary<FireType, Func<IFireSystem>> _customFireModeFactories;

            // Other system factories
            private Func<IFireModeSystem> _fireModeSystemFactory;
            private Func<IAimingSystem> _aimingSystemFactory;
            private Func<IAmmoSystem> _ammoSystemFactory;
            private Func<IRecoilSystem> _recoilSystemFactory;
            private Func<ITrailSystem> _trailSystemFactory;
            private Gun _gun;
            public Initializer(Gun gun)
            {
                _gun = gun;
            
                _customFireModeFactories = new Dictionary<FireType, Func<IFireSystem>>();

                SetDefaultConfiguration();
                SetDefaultFactories();
            }

            private void SetDefaultConfiguration()
            {
                _enabledFireModes = _gun?.gunConfig?.fireModeSettings?.availableFireModes?.ToList()
                                    ?? new List<FireType> { FireType.SemiAutomatic };
            }

            private void SetDefaultFactories()
            {
                // Default fire mode system factory
                _fireModeSystemFactory = () =>
                {
                    var fireModes = CreateFireModes();
                    return new FireModeSwitcher(fireModes);
                };

                _aimingSystemFactory = () => new AimingSystem(
                    
                    _gun.transform,
                    _gun.gunConfig,
                    _gun.HipFireTransform,
                    _gun.AdsTransform
                );

                _ammoSystemFactory = () => new AmmoSystem(
                   _gun.transform,
                   _gun.gunConfig,
                   _gun.MagazinePosition,
                   _gun
                );

                _recoilSystemFactory = () => new RecoilSystem(
                    _gun.gunConfig,
                    _gun.transform,
                    _gun.RecoilTransform,
                    _gun
                );

                _trailSystemFactory = () => new TrailSystem(_gun.gunConfig.trailConfig);
            }

            private List<IFireSystem> CreateFireModes()
            {
                var fireModes = new List<IFireSystem>();

                foreach (var fireType in _enabledFireModes)
                {
                    IFireSystem fireMode;

                    // Check if there's a custom factory for this fire type
                    if (_customFireModeFactories.TryGetValue(fireType, out var factory))
                    {
                        fireMode = factory();
                    }
                    else
                    {
                        // Use default factory
                        fireMode = FireModeFactory.CreateFireMode(
                            fireType,
                            _gun.gunConfig,
                            _gun.transform,
                            _gun,
                            _gun.MuzzleTranform,
                            _onShotFired
                        );
                    }

                    fireModes.Add(fireMode);
                }

                return fireModes;
            }

            // Builder methods for fire modes
            public IGunSystemsBuilder WithFireModes(params FireType[] fireModes)
            {
                if (fireModes != null && fireModes.Length > 0)
                {
                    _enabledFireModes = fireModes.ToList();
                }

                return this;
            }

            public IGunSystemsBuilder WithFireModes(List<FireType> fireModes)
            {
                if (fireModes is { Count: > 0 })
                {
                    _enabledFireModes = fireModes;
                }

                return this;
            }

            public IGunSystemsBuilder WithCustomFireMode<T>(FireType fireType, Func<T> customFactory)
                where T : IFireSystem
            {
                if (customFactory != null)
                {
                    _customFireModeFactories[fireType] = () => customFactory();

                    // Add to enabled fire modes if not already present
                    if (!_enabledFireModes.Contains(fireType))
                    {
                        _enabledFireModes.Add(fireType);
                    }
                }

                return this;
            }

            public IGunSystemsBuilder WithFireModeSystem(Func<IFireModeSystem> fireModeSystemFactory)
            {
                if (fireModeSystemFactory != null)
                    _fireModeSystemFactory = fireModeSystemFactory;
                return this;
            }


            public IGunSystemsBuilder WithAimingSystem(Func<IAimingSystem> aimingSystemFactory)
            {
                if (aimingSystemFactory != null)
                    _aimingSystemFactory = aimingSystemFactory;
                return this;
            }

            public IGunSystemsBuilder WithAmmoSystem(Func<IAmmoSystem> ammoSystemFactory)
            {
                if (ammoSystemFactory != null)
                    _ammoSystemFactory = ammoSystemFactory;
                return this;
            }

            public IGunSystemsBuilder WithRecoilSystem(Func<IRecoilSystem> recoilSystemFactory)
            {
                if (recoilSystemFactory != null)
                    _recoilSystemFactory = recoilSystemFactory;
                return this;
            }

            public IGunSystemsBuilder WithTrailSystem(Func<ITrailSystem> trailSystemFactory)
            {
                if (trailSystemFactory != null)
                    _trailSystemFactory = trailSystemFactory;
                return this;
            }

            public IGunSystemsBuilder AddShotFiredHandler(Action<ShotFiredEvent> handler)
            {
                if (handler != null)
                    _onShotFired.Add(handler);
                return this;
            }

            public IGunSystemsBuilder AddAmmoOutHandler(Action onOutOfAmmo)
            {
                if (onOutOfAmmo != null)
                    _onAmmoOut.Add(onOutOfAmmo);
                return this;
            }

            public void Initialize()
            {
                // Create systems using factories
                var ammoSystem = _ammoSystemFactory();
                var trailSystem = _trailSystemFactory();
                var recoilSystem = _recoilSystemFactory();

                // Add default shot handlers
                AddDefaultShotHandlers(recoilSystem, ammoSystem, trailSystem);

                // Create fire mode system
                var fireModeSystem = _fireModeSystemFactory();

                // Wire up ammo out events
                WireAmmoOutEvents(ammoSystem, fireModeSystem);

                var aimingSystem = _aimingSystemFactory();

                

               _gun.FireModeSystem = fireModeSystem;
               _gun.AimingSystem = aimingSystem;
                _gun.AmmoSystem = ammoSystem;
               _gun.RecoilSystem = recoilSystem;
                _gun.TrailSystem = trailSystem;
         
                _gun.Initialize();
                
            }

            private void AddDefaultShotHandlers(IRecoilSystem recoilSystem, IAmmoSystem ammoSystem,
                ITrailSystem trailSystem)
            {
                _onShotFired.Add(e => recoilSystem.ApplyRecoil());
                _onShotFired.Add(e => ammoSystem.ConsumeAmmo());
                _onShotFired.Add(e => _gun.StartCoroutine(
                    trailSystem.SpawnTrail(e.ShootPoint, e.EndPoint, e.Hit)));
            }

            private void WireAmmoOutEvents(IAmmoSystem ammoSystem, IFireModeSystem fireModeSystem)
            {
                // Wire ammo events to all fire modes
                ammoSystem.OnOutOfAmmo += () =>
                {
                    foreach (var fireMode in fireModeSystem.AvailableFireModes)
                    {
                        fireMode.OnOutOfAmmo();
                    }
                };

                ammoSystem.OnReloadComplete += (r) =>
                {
                    foreach (var firemode in fireModeSystem.AvailableFireModes)
                    {
                        firemode.OnReloadEnded();
                    }
                };

                // Add custom ammo out handlers
                foreach (var handler in _onAmmoOut)
                {
                    ammoSystem.OnOutOfAmmo += handler;
                }
            }
        }

        public Transform MagazinePosition;

        public Transform HipFireTransform;

        public Transform RecoilTransform;
        
        public Transform AdsTransform;

        public Transform MuzzleTranform;
        
        
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