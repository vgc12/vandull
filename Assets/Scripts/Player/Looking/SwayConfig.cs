using UnityEngine;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "SwayConfig", menuName = "Configs/Player/Movement/SwayConfig", order = 1)]
    public class SwayConfig : ScriptableObject
    {
    
        public Sway IdleSway => idleSway;
        
        [SerializeField] private Sway idleSway;
  
        
    }

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
    

    namespace Player.Looking
    {
        [CreateAssetMenu(fileName = "CameraBobConfig", menuName = "Configs/Player/Movement/CameraBobConfig", order = 1)]
        public class CameraBobConfig : ScriptableObject
        {
    
            public CameraBobSetting walkBob;
            public CameraBobSetting sprintBob;

        }

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
}