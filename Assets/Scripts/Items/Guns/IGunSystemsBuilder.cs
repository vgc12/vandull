using System;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;
using UI;

namespace Items.Guns
{
    public interface IGunSystemsBuilder : IBuilder<GunSystems>
    {
        IGunSystemsBuilder WithAimingSystem(Func<IAimingSystem> aimingSystemFactory = null);
        IGunSystemsBuilder WithAmmoSystem(Func<IAmmoSystem> ammoSystemFactory = null);
        IGunSystemsBuilder WithRecoilSystem(Func<IRecoilSystem> recoilSystemFactory = null);
        IGunSystemsBuilder WithTrailSystem(Func<ITrailSystem> trailSystemFactory = null);
        IGunSystemsBuilder AddShotFiredHandler(Action<ShotFiredEvent> handler);
        IGunSystemsBuilder AddAmmoOutHandler(Action onOutOfAmmo);
        IGunSystemsBuilder WithAnimationSystem(Func<IItemAnimationSystem> func);
        IGunSystemsBuilder WithOwnerStatus(OwnerStatus player);
    }
}