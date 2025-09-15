using UnityEngine;

namespace Items.Guns
{
    [System.Serializable]
    public class AimSettings
    {
        public float adsTime = 0.3f;
        public Vector3 muzzlePoint;
        public Vector3 adsPosition;
        public Vector3 hipFirePoint;
    }
}