using System;
using Attributes;

namespace Items.Guns
{
    [Serializable]
    public class GunConfig
    {
        [ScriptableObjectDropdown] public FireModeSettings fireModeSettings;
        [ScriptableObjectDropdown] public AimSettings aimSettings;
        [ScriptableObjectDropdown] public FiringSettings firingSettings;
        [ScriptableObjectDropdown] public DamageSettings damageSettings;
        [ScriptableObjectDropdown] public AmmoSettings ammoSettings;
        [ScriptableObjectDropdown] public RecoilSettings recoilSettings;
    }
}