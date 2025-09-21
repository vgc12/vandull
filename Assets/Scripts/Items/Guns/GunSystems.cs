using Items.Guns.Recoil;
using Items.Guns.Trail;

namespace Items.Guns.Items.Guns.Builder
{
    public struct GunSystems
    {
        public IFireSystem FireSystem;
        public IAimingSystem AimingSystem;
        public IAmmoSystem AmmoSystem;
        public IRecoilSystem RecoilSystem;
        public ITrailSystem TrailSystem;
    }
}