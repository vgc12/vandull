using System;
using General;
using UnityEngine;

namespace Items.Guns.Firing
{
    [CreateAssetMenu(fileName = "Firing Settings", menuName = "Guns/Firing Settings", order = 0)]
    public sealed class FiringSettings : ScriptableObject, ICloneable
    {
        public float fireRate = 0.2f;
        public int burstCount = 3;
        public float burstDelay = 0.1f;
        public Vector3 muzzlePoint;


        public object Clone()
        {
            var config = CreateInstance<FiringSettings>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}