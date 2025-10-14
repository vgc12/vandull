using System;
using General;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Damage Settings", menuName = "Guns/Damage Settings", order = 2)]
    public class DamageSettings : ScriptableObject, ICloneable
    {
        public int damage = 100;
        public float range = 100f;

        public object Clone()
        {
            var config = CreateInstance<DamageSettings>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}