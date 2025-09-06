using UnityEngine;

namespace Player.Looking
{
    [System.Serializable]
    public class Sway
    {
        [Range(0.0001f, 5f)]
        public float horizontalSwayAmount;
        [Range(0.0001f, 5f)]
        public float horizontalSwaySpeed;
        [Range(0.0001f, 5f)]
        public float verticalSwayAmount;
        [Range(0.0001f, 5f)]
        public float verticalSwaySpeed;
        [Range(0.0001f, 50f)]
        public float swayMultiplier = 0.00001f;
    }
}