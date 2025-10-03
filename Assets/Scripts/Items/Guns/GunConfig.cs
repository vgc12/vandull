using System;
using Attributes;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
using Items.Guns.Recoil;
using Items.Guns.Trail;

namespace Items.Guns
{
    [Serializable]
    public class GunConfig
    {
        [Required] public FireModeSettings fireModeSettings;
        [Required] public FiringSettings firingSettings;
        [Required] public AimSettings aimSettings;
        [Required] public DamageSettings damageSettings;
        [Required] public AmmoSettings ammoSettings;
        [Required] public RecoilSettings recoilSettings;
        [Required] public TrailConfig trailConfig;
  
    }

    public enum GripType
    {
        ARNoGrip,
        Pistol,
    }
    
    public class GunSystems
    {
        public IRecoilSystem RecoilSystem { get; init; }
        public ITrailSystem TrailSystem { get; init; }
        public IAimingSystem AimingSystem { get; init; }

        public IFireModeSystem FireModeSystem { get; init; }
        public IAmmoSystem AmmoSystem { get; init; }
    }
}