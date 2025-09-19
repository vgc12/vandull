using UnityEngine;

namespace Items.Guns
{
    public class AimSettings : ScriptableObject
    {
        public float adsTime = 0.3f;
        public Vector3 muzzlePoint;
        public Vector3 adsPosition;
        public Vector3 hipFirePoint;
    }
}