using UnityEngine;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "SwayConfig", menuName = "ScriptableObjects/SwayConfig", order = 1)]
    public class SwayConfig : ScriptableObject
    {
        [Range(0,10)] public float verticalSwayAmount = 0.5f;
        [Range(0,10)] public float verticalSwaySpeed = 1f;
        [Range(0,10)] public float horizontalSwayAmount = 0.5f;
        [Range(0,10)] public float horizontalSwaySpeed = 1f;
        
        [Range(0,20)] public float walkSwayMultiplier = 1f;
        [Range(0,20)] public float sprintSwayMultiplier = 2f;
        [Range(0,20)] public float aimSwayMultiplier = 0.5f;
        
    }
}