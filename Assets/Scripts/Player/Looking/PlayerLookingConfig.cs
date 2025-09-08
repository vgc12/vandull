using UnityEngine;

namespace Player.Looking
{
    [CreateAssetMenu(fileName = "PlayerLookingConfig", menuName = "Configs/Player/Movement/PlayerLookingConfig", order = 1)]
    public class PlayerLookingConfig : ScriptableObject
    {
        public float Sensitivity => sensitivity;
        public float LeanAngle => leanAngle;
        public float LeanSpeed => leanSpeed;

        [SerializeField] private float sensitivity = 50f;
        [SerializeField] private float leanSpeed = 10f;
        [SerializeField] private float leanAngle = 15f;
    }
}