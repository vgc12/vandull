using System;
using UnityEngine;

namespace Items.Guns
{
    [Serializable]
    public class FiringSettings 
    {
        public float fireRate = 0.2f; 
        public int burstCount = 3;
        public float burstDelay = 0.1f; 

  
    }
}