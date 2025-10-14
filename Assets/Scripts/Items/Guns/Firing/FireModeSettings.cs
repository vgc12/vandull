using System;
using System.Collections.Generic;
using General;
using UnityEngine;

namespace Items.Guns.Firing
{
    [CreateAssetMenu(fileName = "Fire Mode Settings", menuName = "Guns/Fire Mode Settings", order = 0)]
    public class FireModeSettings : ScriptableObject, ICloneable
    {
        [Header("Fire Mode Settings")] public List<FireType> availableFireModes = new() { FireType.SemiAutomatic };

        public FireType defaultFireType = FireType.SemiAutomatic;

        public object Clone()
        {
            var config = CreateInstance<FireModeSettings>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}