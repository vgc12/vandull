using System;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;

namespace Items.Guns
{
    public interface IGunSystemsBuilder
    {
        IGunSystemsBuilder WithAimingSystem(Func<IAimingSystem> aimingSystemFactory = null);
        IGunSystemsBuilder WithAmmoSystem(Func<IAmmoSystem> ammoSystemFactory = null);
        IGunSystemsBuilder WithRecoilSystem(Func<IRecoilSystem> recoilSystemFactory = null);
        IGunSystemsBuilder WithTrailSystem(Func<ITrailSystem> trailSystemFactory = null);
        IGunSystemsBuilder AddShotFiredHandler(Action<ShotFiredEvent> handler);
        IGunSystemsBuilder AddAmmoOutHandler(Action onOutOfAmmo);
        GunSystems Build();
    }
}