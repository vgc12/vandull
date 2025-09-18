using UnityEngine;

namespace Items.Guns
{

    [CreateAssetMenu(fileName = "Aim Settings", menuName = "Guns/Aim Settings", order = 3)]
    public class AimSettings : ScriptableObject
    {
        public float adsTime = 0.3f;
        public Vector3 muzzlePoint;
        public Vector3 adsPosition;
        public Vector3 hipFirePoint;
    }
}