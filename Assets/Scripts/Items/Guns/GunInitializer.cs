using System;
using System.Collections.Generic;
using System.Linq;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UnityEngine;

namespace Items.Guns
{
    public abstract class GunInitializer : ScriptableObject
    {
        public abstract GunSystems CreateGunSystems(Gun gun);

        #region Builder

        protected class Builder : IGunSystemsBuilder
        {
            private readonly Gun _gun;
            private readonly FireModeConfiguration _fireModeConfig;
            private readonly SystemFactories _factories;
            private readonly EventHandlers _eventHandlers;

            public Builder(Gun gun)
            {
                _gun = gun;
                _fireModeConfig = new FireModeConfiguration(gun);
                _factories = new SystemFactories(gun);
                _eventHandlers = new EventHandlers();
            }

            // Configuration Methods
            public IGunSystemsBuilder WithFireModes(params FireType[] fireModes)
            {
                _fireModeConfig.SetEnabledModes(fireModes);
                return this;
            }

            public IGunSystemsBuilder WithFireModes(List<FireType> fireModes)
            {
                _fireModeConfig.SetEnabledModes(fireModes);
                return this;
            }

            public IGunSystemsBuilder WithCustomFireMode<T>(FireType fireType, Func<T> customFactory)
                where T : IFireSystem
            {
                _fireModeConfig.AddCustomFactory(fireType, customFactory);
                return this;
            }

            // System Factory Methods
            public IGunSystemsBuilder WithFireModeSystem(Func<IFireModeSystem> factory)
            {
                _factories.SetFireModeSystemFactory(factory);
                return this;
            }

            public IGunSystemsBuilder WithAimingSystem(Func<IAimingSystem> factory)
            {
                _factories.SetAimingSystemFactory(factory);
                return this;
            }

            public IGunSystemsBuilder WithAmmoSystem(Func<IAmmoSystem> factory)
            {
                _factories.SetAmmoSystemFactory(factory);
                return this;
            }

            public IGunSystemsBuilder WithRecoilSystem(Func<IRecoilSystem> factory)
            {
                _factories.SetRecoilSystemFactory(factory);
                return this;
            }

            public IGunSystemsBuilder WithTrailSystem(Func<ITrailSystem> factory)
            {
                _factories.SetTrailSystemFactory(factory);
                return this;
            }

            // Event Handler Methods
            public IGunSystemsBuilder AddShotFiredHandler(Action<ShotFiredEvent> handler)
            {
                _eventHandlers.AddShotFiredHandler(handler);
                return this;
            }

            public IGunSystemsBuilder AddAmmoOutHandler(Action handler)
            {
                _eventHandlers.AddAmmoOutHandler(handler);
                return this;
            }

            // Build Method
            public GunSystems Build()
            {
                var systems = CreateSystems();
                WireUpEvents(systems);
                return systems;
            }

            private GunSystems CreateSystems()
            {
                var ammoSystem = _factories.CreateAmmoSystem();
                var trailSystem = _factories.CreateTrailSystem();
                var recoilSystem = _factories.CreateRecoilSystem();
                var aimingSystem = _factories.CreateAimingSystem();

                // Add default shot handlers before creating fire modes
                _eventHandlers.AddDefaultHandlers(_gun, recoilSystem, ammoSystem, trailSystem);

                var fireModes = _fireModeConfig.CreateFireModes(_gun, _eventHandlers.ShotFiredHandlers);
                var fireModeSystem = _factories.CreateFireModeSystem(fireModes);

                return new GunSystems
                {
                    TrailSystem = trailSystem,
                    FireModeSystem = fireModeSystem,
                    AimingSystem = aimingSystem,
                    AmmoSystem = ammoSystem,
                    RecoilSystem = recoilSystem
                };
            }

            private void WireUpEvents(GunSystems systems)
            {
                // Wire ammo events to all fire modes
                systems.AmmoSystem.OnOutOfAmmo += () =>
                {
                    foreach (var fireMode in systems.FireModeSystem.AvailableFireModes)
                        fireMode.OnOutOfAmmo();
                    
                };

                systems.AmmoSystem.OnReloadComplete += _ =>
                {
                    foreach (var fireMode in systems.FireModeSystem.AvailableFireModes)
                        fireMode.OnReloadEnded();
                };

                // Add custom ammo out handlers
                _eventHandlers.WireAmmoOutHandlers(systems.AmmoSystem);
            }
        }

        #endregion

        #region Helper Classes

        private class FireModeConfiguration
        {
            private readonly Gun _gun;
            private readonly Dictionary<FireType, Func<IFireSystem>> _customFactories = new();
            private List<FireType> _enabledModes;

            public FireModeConfiguration(Gun gun)
            {
                _gun = gun;
                _enabledModes = gun?.gunConfig?.fireModeSettings?.availableFireModes?.ToList()
                               ?? new List<FireType> { FireType.SemiAutomatic };
            }

            public void SetEnabledModes(IEnumerable<FireType> modes)
            {
                if (modes != null)
                {
                    var modesList = modes.ToList();
                    if (modesList.Count > 0)
                        _enabledModes = modesList;
                }
            }

            public void AddCustomFactory<T>(FireType fireType, Func<T> factory) where T : IFireSystem
            {
                if (factory == null) return;

                _customFactories[fireType] = () => factory();

                if (!_enabledModes.Contains(fireType))
                    _enabledModes.Add(fireType);
            }

            public List<IFireSystem> CreateFireModes(Gun gun, List<Action<ShotFiredEvent>> shotHandlers)
            {
                var fireModes = new List<IFireSystem>();

                foreach (var fireType in _enabledModes)
                {
                    var fireMode = CreateFireMode(gun, fireType);
                    AttachShotHandlers(fireMode, shotHandlers);
                    fireModes.Add(fireMode);
                }

                return fireModes;
            }

            private IFireSystem CreateFireMode(Gun gun, FireType fireType)
            {
                if (_customFactories.TryGetValue(fireType, out var factory))
                    return factory();

                return FireModeFactory.CreateFireMode(
                    fireType,
                    gun.gunConfig,
                    gun.transform,
                    gun,
                    gun.muzzleTransform
                );
            }

            private void AttachShotHandlers(IFireSystem fireMode, List<Action<ShotFiredEvent>> handlers)
            {
                foreach (var handler in handlers)
                    fireMode.OnShotFired += handler;
            }
        }

        private class SystemFactories
        {
            private readonly Gun _gun;
            private Func<IFireModeSystem> _fireModeSystemFactory;
            private Func<IAimingSystem> _aimingSystemFactory;
            private Func<IAmmoSystem> _ammoSystemFactory;
            private Func<IRecoilSystem> _recoilSystemFactory;
            private Func<ITrailSystem> _trailSystemFactory;

            public SystemFactories(Gun gun)
            {
                _gun = gun;
                SetDefaultFactories();
            }

            public void SetFireModeSystemFactory(Func<IFireModeSystem> factory)
            {
                if (factory != null) _fireModeSystemFactory = factory;
            }

            public void SetAimingSystemFactory(Func<IAimingSystem> factory)
            {
                if (factory != null) _aimingSystemFactory = factory;
            }

            public void SetAmmoSystemFactory(Func<IAmmoSystem> factory)
            {
                if (factory != null) _ammoSystemFactory = factory;
            }

            public void SetRecoilSystemFactory(Func<IRecoilSystem> factory)
            {
                if (factory != null) _recoilSystemFactory = factory;
            }

            public void SetTrailSystemFactory(Func<ITrailSystem> factory)
            {
                if (factory != null) _trailSystemFactory = factory;
            }

            public IFireModeSystem CreateFireModeSystem(List<IFireSystem> fireModes)
            {
                return _fireModeSystemFactory != null 
                    ? _fireModeSystemFactory() 
                    : new FireModeSwitcher(fireModes);
            }

            public IAimingSystem CreateAimingSystem() => _aimingSystemFactory();
            public IAmmoSystem CreateAmmoSystem() => _ammoSystemFactory();
            public IRecoilSystem CreateRecoilSystem() => _recoilSystemFactory();
            public ITrailSystem CreateTrailSystem() => _trailSystemFactory();

            private void SetDefaultFactories()
            {
                _aimingSystemFactory = () => new AimingSystem(
                    _gun.transform,
                    _gun.gunConfig,
                    _gun.hipFireTransform,
                    _gun.aimTransform
                );

                _ammoSystemFactory = () => new AmmoSystem(
                    _gun.gunConfig,
                    _gun.magazinePosition,
                    _gun
                );

                _recoilSystemFactory = () => new RecoilSystem(
                    _gun.gunConfig,
                    _gun.transform,
                    _gun.recoilTransform,
                    _gun
                );

                _trailSystemFactory = () => new TrailSystem(_gun.gunConfig.trailSettings);
            }
        }

        private class EventHandlers
        {
            public readonly List<Action<ShotFiredEvent>> ShotFiredHandlers = new();
            private readonly List<Action> _ammoOutHandlers = new();

            public void AddShotFiredHandler(Action<ShotFiredEvent> handler)
            {
                if (handler != null) ShotFiredHandlers.Add(handler);
            }

            public void AddAmmoOutHandler(Action handler)
            {
                if (handler != null) _ammoOutHandlers.Add(handler);
            }

            public void AddDefaultHandlers(Gun gun, IRecoilSystem recoil, IAmmoSystem ammo, ITrailSystem trail)
            {
                ShotFiredHandlers.Add(e => recoil.ApplyRecoil());
                ShotFiredHandlers.Add(e => ammo.ConsumeAmmo());
                ShotFiredHandlers.Add(e => gun.StartCoroutine(
                    trail.SpawnTrail(e.ShootPoint, e.EndPoint, e.Hit)));
            }

            public void WireAmmoOutHandlers(IAmmoSystem ammoSystem)
            {
                foreach (var handler in _ammoOutHandlers)
                    ammoSystem.OnOutOfAmmo += handler;
            }
        }

        private static class FireModeFactory
        {
            public static IFireSystem CreateFireMode(
                FireType fireType,
                GunConfig config,
                Transform gunTransform,
                MonoBehaviour behaviour,
                Transform muzzleTransform)
            {
                return fireType switch
                {
                    FireType.SemiAutomatic => new SemiAutoFireMode(config, gunTransform, behaviour, muzzleTransform),
                    FireType.Automatic => new AutomaticFireMode(config, gunTransform, behaviour, muzzleTransform),
                    FireType.Burst => new BurstFireMode(config, gunTransform, behaviour, muzzleTransform),
                    _ => throw new ArgumentException($"Unsupported fire type: {fireType}")
                };
            }
        }

        #endregion
    }
}