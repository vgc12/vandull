using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Guns
{
    [Serializable]
    public class FireModeSettings
    {
        [Header("Fire Mode Settings")]
        public List<FireType> availableFireModes = new List<FireType> { FireType.SemiAutomatic };
        public FireType defaultFireType = FireType.SemiAutomatic;
    }
}