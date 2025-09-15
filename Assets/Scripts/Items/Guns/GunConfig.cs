using System;
using Items.Guns.Recoil;
using UnityEngine;

namespace Items.Guns
{

    [Serializable]
    public class GunConfig
    {

        public FireModeSettings fireModeSettings;
        public AimSettings aimSettings;
        public FiringSettings firingSettings;
        public DamageSettings damageSettings;
        public AmmoSettings ammoSettings;
        public RecoilSettings recoilSettings;
    
    }
}