using System;
using General;
using UnityEngine;

namespace Items.Guns
{
   [CreateAssetMenu (fileName = "Damage Settings", menuName = "Guns/Damage Settings", order = 2)]
    public class DamageSettings : ScriptableObject, ICloneable
    {
        public float damage = 100f;
        public float range = 100f;
        public object Clone()
        {
            DamageSettings config = CreateInstance<DamageSettings>();
            Utilities.CopyValues(this, config);
            return config;
        }
    }
}