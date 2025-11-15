using System;
using System.Collections.Generic;
using System.Linq;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using Player;
using UnityEngine;

namespace Items.Guns
{
    public abstract class GunInitializer : ScriptableObject
    {
        public abstract GunSystems CreateGunSystems(Gun gun);

        #region Builder

        protected class Builder : IGunSystemsBuilder
        {
            private readonly EventHandlers _eventHandlers;
            private readonly SystemFactories _factories;
            private readonly FireModeConfiguration _fireModeConfig;
            private readonly Gun _gun;

            public Builder(Gun gun)
            {
                _gun = gun;
                _fireModeConfig = new FireModeConfiguration(gun);
                _factories = new SystemFactories(gun);
                _eventHandlers = new EventHandlers();
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

            public IGunSystemsBuilder WithAnimationSystem(Func<IItemAnimationSystem> factory)
            {
                _factories.SetAnimationSystemFactory(factory);
                return this;
            }

            public IGunSystemsBuilder WithOwnerStatus(OwnerStatus owner)
            {
                _factories.SetOwnerStatus(owner);
                return this;
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

            private GunSystems CreateSystems()
            {
                _gun.Owner = _factories.GetOwner();
                var ammoSystem = _factories.CreateAmmoSystem();
                var trailSystem = _factories.CreateTrailSystem();
                var recoilSystem = _factories.CreateRecoilSystem();
                var aimingSystem = _factories.CreateAimingSystem();
                var animationSystem = _factories.CreateAnimationSystem();

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
                    RecoilSystem = recoilSystem,
                    ItemAnimationSystem = animationSystem
                };
            }

            private void WireUpEvents(GunSystems systems)
            {
                // Add custom ammo out handlers
                _eventHandlers.WireAmmoOutHandlers(systems.AmmoSystem);
            }
        }

        #endregion

        #region Helper Classes

        private class FireModeConfiguration
        {
            private readonly Dictionary<FireType, Func<IFireSystem>> _customFactories = new();

            private List<FireType> _enabledModes;

            public FireModeConfiguration(Gun gun)
            {
                _enabledModes = gun?.fireModeSettings?.availableFireModes?.ToList()
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
                    fireType, gun
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
            private Func<IAimingSystem> _aimingSystemFactory;
            private Func<IAmmoSystem> _ammoSystemFactory;
            private Func<IItemAnimationSystem> _animationSystemFactory;
            private Func<IFireModeSystem> _fireModeSystemFactory;
            private OwnerStatus _owner;
            private Func<IRecoilSystem> _recoilSystemFactory;
            private Func<ITrailSystem> _trailSystemFactory;

            public SystemFactories(Gun gun)
            {
                _gun = gun;
                SetDefaultFactories();
            }

            public void SetAnimationSystemFactory(Func<IItemAnimationSystem> factory)
            {
                if (factory != null) _animationSystemFactory = factory;
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

            public void SetOwnerStatus(OwnerStatus owner)
            {
                _owner = owner;
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

            public IAimingSystem CreateAimingSystem()
            {
                return _aimingSystemFactory();
            }

            public IAmmoSystem CreateAmmoSystem()
            {
                return _ammoSystemFactory();
            }

            public IRecoilSystem CreateRecoilSystem()
            {
                return _recoilSystemFactory();
            }

            public ITrailSystem CreateTrailSystem()
            {
                return _trailSystemFactory();
            }

            public IItemAnimationSystem CreateAnimationSystem()
            {
                return _animationSystemFactory != null
                    ? _animationSystemFactory()
                    : new PlayerItemAnimationSystem();
            }

            private void SetDefaultFactories()
            {
                _aimingSystemFactory = () => new AimingSystem(
                    _gun
                );

                _ammoSystemFactory = () => new AmmoSystem(
                    _gun,
                    _gun.GetComponentInParent<RigHandler>()
                );

                _recoilSystemFactory = () => new RecoilSystem(
                    _gun
                );

                _trailSystemFactory = () => new TrailSystem(_gun.trailSettings);

                _animationSystemFactory = () => new PlayerItemAnimationSystem();
                _owner = OwnerStatus.Player;
            }

            public OwnerStatus GetOwner()
            {
                return _owner;
            }
        }

        private class EventHandlers
        {
            private readonly List<Action> _ammoOutHandlers = new();

            public readonly List<Action<ShotFiredEvent>> ShotFiredHandlers = new();

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
                Gun gun)
            {
                return fireType switch
                {
                    FireType.SemiAutomatic => new SemiAutoFireMode(gun),
                    FireType.Automatic => new AutomaticFireMode(gun),
                    FireType.Burst => new BurstFireMode(gun),
                    _ => throw new ArgumentException($"Unsupported fire type: {fireType}")
                };
            }
        }

        #endregion
    }
}