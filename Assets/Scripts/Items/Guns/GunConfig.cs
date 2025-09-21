using System;
using Attributes;
using Items.Guns.Trail;
using UnityEngine;

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
}