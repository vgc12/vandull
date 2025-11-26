using UnityEngine;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "SwayConfig", menuName = "Configs/Player/Movement/SwayConfig", order = 1)]
    public sealed class SwayConfig : ScriptableObject
    {
        [Range(0.0001f, 5f)] public float horizontalSwayAmount;

        [Range(0.0001f, 5f)] public float horizontalSwaySpeed;

        [Range(0.0001f, 5f)] public float verticalSwayAmount;

        [Range(0.0001f, 5f)] public float verticalSwaySpeed;

        [Range(0.0001f, 50f)] public float swayMultiplier = 0.00001f;
        [Range(0, 50f)] public float aimMultiplier = 1f;
    }
}