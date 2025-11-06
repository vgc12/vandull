using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Recoil;
using Items.Guns.Trail;

namespace Items.Guns
{
    public class GunSystems
    {
        public IRecoilSystem RecoilSystem { get; init; }
        public ITrailSystem TrailSystem { get; init; }
        public IAimingSystem AimingSystem { get; init; }

        public IFireModeSystem FireModeSystem { get; init; }
        public IAmmoSystem AmmoSystem { get; init; }

        public IItemAnimationSystem ItemAnimationSystem { get; init; }
    }
}