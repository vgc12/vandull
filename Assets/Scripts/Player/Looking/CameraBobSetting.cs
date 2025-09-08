using UnityEngine;

namespace Player.Looking
{
    [System.Serializable]
    public class CameraBobSetting
    {
        [Range(0.0001f, 5f)]
        public float frequency = 1f;
        [Range(0.0001f, 5f)]
        public float maxSpeed = 5f;
        [Range(0.0001f, 5f)]
        public float horizontalAmplitude;
        [Range(0.0001f, 5f)]
        public float verticalAmplitude;
        [Range(0.0001f, 50f)]
        public float speedCurve = 0.00001f;
    }
}