using System.Collections.Generic;
using UnityEngine;

namespace Items.Guns
{
    [CreateAssetMenu(fileName = "Gun Config", menuName = "Items/GunConfig")]
    public class GunConfig : ScriptableObject
    {
        [Header("Fire Mode Settings")]
        public List<FireType> availableFireModes = new List<FireType> { FireType.SemiAutomatic };
        public FireType defaultFireType = FireType.SemiAutomatic;
    
        public AimSettings aimSettings;
        public FiringSettings firingSettings;
        public DamageSettings damageSettings;
        public AmmoSettings ammoSettings;
       
        public float adsTime = 0.3f;
    }

    [System.Serializable]
    public class AmmoSettings 
    {
        public int magazineSize;
        public int magazineCount;
        public float reloadTime = 2f;
    }
    
    [System.Serializable]
    public class DamageSettings
    {
        public float damage = 100f;
        public float range = 100f;
    }
    
    [System.Serializable]
    public class FiringSettings 
    {
        public float fireRate = 0.2f; 
        public int burstCount = 3;
        public float burstDelay = 0.1f; 
    
    }
    
}