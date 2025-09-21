using System;
using System.Collections.Generic;
using Items.Guns.Items.Guns.Dependencies;
using Items.Guns.Recoil;
using Items.Guns.Trail;

namespace Items.Guns.Items.Guns.Builder
{
    public class GunSystemsBuilder : IGunSystemsBuilder
    {
        private readonly GunDependencyContainer _container;
        private readonly List<Action<ShotFiredEvent>> _onShotFired = new List<Action<ShotFiredEvent>>();
        private readonly List<Action> _onAmmoOut = new List<Action>();

   
        private Func<List<Action<ShotFiredEvent>>, IFireSystem> _fireSystemFactory;
        private Func<IAimingSystem> _aimingSystemFactory;
        private Func<IAmmoSystem> _ammoSystemFactory;
        private Func<IRecoilSystem> _recoilSystemFactory;
        private Func<ITrailSystem> _trailSystemFactory;

        public GunSystemsBuilder(GunDependencyContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            
            SetDefaultFactories();
        }

        private void SetDefaultFactories()
        {
            _fireSystemFactory = (shotHandlers) => new FireSystem(
                _container.Config,
                _container.GunTransform,
                _container.Behaviour,
                shotHandlers
            );

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

        public IGunSystemsBuilder WithFireSystem(Func<List<Action<ShotFiredEvent>>, IFireSystem> fireSystemFactory = null)
        {
            if (fireSystemFactory != null)
                _fireSystemFactory = fireSystemFactory;
            return this;
        }

        public IGunSystemsBuilder WithAimingSystem(Func<IAimingSystem> aimingSystemFactory = null)
        {
            if (aimingSystemFactory != null)
                _aimingSystemFactory = aimingSystemFactory;
            return this;
        }

        public IGunSystemsBuilder WithAmmoSystem(Func<IAmmoSystem> ammoSystemFactory = null)
        {
            if (ammoSystemFactory != null)
                _ammoSystemFactory = ammoSystemFactory;
            return this;
        }

        public IGunSystemsBuilder WithRecoilSystem(Func<IRecoilSystem> recoilSystemFactory = null)
        {
            if (recoilSystemFactory != null)
                _recoilSystemFactory = recoilSystemFactory;
            return this;
        }

        public IGunSystemsBuilder WithTrailSystem(Func<ITrailSystem> trailSystemFactory = null)
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
            
            AddDefaultShotHandlers(recoilSystem, ammoSystem, trailSystem);
            
            // Create fire system with all shot handlers
            var fireSystem = _fireSystemFactory(_onShotFired);
            
            // Wire up ammo out events
            WireAmmoOutEvents(ammoSystem, fireSystem);
            
            var aimingSystem = _aimingSystemFactory();

            var systems = new GunSystems
            {
                FireSystem = fireSystem,
                AimingSystem = aimingSystem,
                AmmoSystem = ammoSystem,
                RecoilSystem = recoilSystem,
                TrailSystem = trailSystem
            };

            return systems;
        }

        private void AddDefaultShotHandlers(IRecoilSystem recoilSystem, IAmmoSystem ammoSystem, ITrailSystem trailSystem)
        {
            _onShotFired.Add(e => recoilSystem.ApplyRecoil());
            _onShotFired.Add(e => ammoSystem.ConsumeAmmo());
            _onShotFired.Add(e => _container.Behaviour.StartCoroutine(
                trailSystem.SpawnTrail(e.ShootPoint, e.EndPoint, e.Hit)));
        }

        private void WireAmmoOutEvents(IAmmoSystem ammoSystem, IFireSystem fireSystem)
        {
           
            ammoSystem.OnOutOfAmmo += () => (fireSystem as FireSystem)?.OnOutOfAmmo();
            
      
            foreach (var handler in _onAmmoOut)
            {
                ammoSystem.OnOutOfAmmo += handler;
            }
        }

    
    }
}