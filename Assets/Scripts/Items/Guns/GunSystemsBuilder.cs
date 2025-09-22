using System;
using System.Collections.Generic;
using System.Linq;
using Items.Guns.Firing;
using Items.Guns.Items.Guns.Dependencies;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UnityEngine;

namespace Items.Guns
{
    public abstract class FireModeFactory
    {
        public static IFireSystem CreateFireMode(
            FireType fireType, 
            GunConfig config, 
            Transform gunTransform, 
            MonoBehaviour behaviour,
            List<Action<ShotFiredEvent>> shotHandlers = null)
        {
            IFireSystem fireSystem = fireType switch
            {
                FireType.SemiAutomatic => new SemiAutoFireMode(config, gunTransform, behaviour),
                FireType.Automatic => new AutomaticFireMode(config, gunTransform, behaviour),
                FireType.Burst => new BurstFireMode(config, gunTransform, behaviour),
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
  public class GunSystemsBuilder : IGunSystemsBuilder
    {
        private readonly GunDependencyContainer _container;
        private readonly List<Action<ShotFiredEvent>> _onShotFired = new List<Action<ShotFiredEvent>>();
        private readonly List<Action> _onAmmoOut = new List<Action>();

        // Fire mode configuration
        private List<FireType> _enabledFireModes;
        private readonly Dictionary<FireType, Func<IFireSystem>> _customFireModeFactories;

        // Other system factories
        private Func<IFireModeSystem> _fireModeSystemFactory;
        private Func<IAimingSystem> _aimingSystemFactory;
        private Func<IAmmoSystem> _ammoSystemFactory;
        private Func<IRecoilSystem> _recoilSystemFactory;
        private Func<ITrailSystem> _trailSystemFactory;

        public GunSystemsBuilder(GunDependencyContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _customFireModeFactories = new Dictionary<FireType, Func<IFireSystem>>();
            
            SetDefaultConfiguration();
            SetDefaultFactories();
        }

        private void SetDefaultConfiguration()
        {
           
            _enabledFireModes = _container.Config?.fireModeSettings?.availableFireModes?.ToList() 
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
                _container.GunTransform,
                _container.Config,
                _container.HipFireTransform,
                _container.AdsTransform
            );

            _ammoSystemFactory = () => new AmmoSystem(
                _container.GunTransform,
                _container.Config,
                _container.Behaviour
            );

            _recoilSystemFactory = () => new RecoilSystem(
                _container.Config,
                _container.GunTransform,
                _container.RecoilTransform,
                _container.Behaviour
            );

            _trailSystemFactory = () => new TrailSystem(_container.Config.trailConfig);
        }

        private List<IFireSystem> CreateFireModes()
        {
            var fireModes = new List<IFireSystem>();

            foreach (var fireType in _enabledFireModes)
            {
                IFireSystem fireMode;

                // Check if there's a custom factory for this fire type
                if (_customFireModeFactories.ContainsKey(fireType))
                {
                    fireMode = _customFireModeFactories[fireType]();
                }
                else
                {
                    // Use default factory
                    fireMode = FireModeFactory.CreateFireMode(
                        fireType, 
                        _container.Config, 
                        _container.GunTransform, 
                        _container.Behaviour,
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
            if (fireModes != null && fireModes.Count > 0)
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

        public GunSystems Build()
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

            var systems = new GunSystems
            {
                FireModeSystem = fireModeSystem,
                AimingSystem = aimingSystem,
                AmmoSystem = ammoSystem,
                RecoilSystem = recoilSystem,
                TrailSystem = trailSystem
            };

            return systems;
        }

        private void AddDefaultShotHandlers(IRecoilSystem recoilSystem, IAmmoSystem ammoSystem,
            ITrailSystem trailSystem)
        {
            _onShotFired.Add(e => recoilSystem.ApplyRecoil());
            _onShotFired.Add(e => ammoSystem.ConsumeAmmo());
            _onShotFired.Add(e => _container.Behaviour.StartCoroutine(
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
}