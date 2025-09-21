using System;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Ammo Settings", menuName = "Guns/Ammo Settings", order = 1)]
    public class AmmoSettings : ScriptableObject
    {
        [Range(1, 150)]
        public int magazineSize;
        [Range(1, 20)]
        public int magazineCount = 4;
        [Min(0.1f)]
        public float reloadTime = 2f;
        
        public Vector3 magazinePosition;
        public Vector3 magazineRotation;
        
        public GameObject magazinePrefab;
    }
}