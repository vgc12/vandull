using System.Collections.Generic;
using Items.Guns.Recoil;
using Items.Guns.Trail;

namespace Items.Guns
{
    public struct GunSystems
    {
        public IFireModeSystem FireModeSystem;
        public IAimingSystem AimingSystem;
        public IAmmoSystem AmmoSystem;
        public IRecoilSystem RecoilSystem;
        public ITrailSystem TrailSystem;
    }
}