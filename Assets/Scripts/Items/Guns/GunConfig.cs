using System;
using Attributes;
using Items.Guns.Aiming;
using Items.Guns.Ammo;
using Items.Guns.Firing;
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
        [Required] public TrailSettings trailSettings;
        [Required] public AudioSettings AudioSettings;
    }

    public enum GripType
    {
        ARNoGrip,
        Pistol
    }
}