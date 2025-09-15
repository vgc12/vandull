using System;
using UnityEngine;

namespace Items.Guns
{
    [Serializable]
    public class AmmoSettings 
    {
        [Range(1, 150)]
        public int magazineSize;
        [Range(1, 20)]
        public int magazineCount = 4;
        [Min(0.1f)]
        public float reloadTime = 2f;
    }
}