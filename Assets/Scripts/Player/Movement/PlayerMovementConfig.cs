using UnityEngine;

namespace Player.Movement
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Configs/Player/Movement/PlayerMovementConfig")]
    public class PlayerMovementConfig : ScriptableObject
    {
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float MovementMultiplier => movementMultiplier;
        public float JumpForce => jumpForce;
        public float JumpMultiplier => jumpMultiplier;
        public float AirDrag => airDrag;
        public float GroundDrag => groundDrag;

        [Header("Movement")] 
        [SerializeField, Range(10, 100)] private float walkSpeed = 50f;
        [SerializeField, Range(10, 200)] private float sprintSpeed = 75f;
        [SerializeField, Range(1, 100)] private float movementMultiplier = 10f;

        [Header("Jumping")] 
        [SerializeField, Range(1, 10)] private float jumpForce = 5f;
        [SerializeField, Range(1, 100)] private float jumpMultiplier = 10f;
        [SerializeField, Range(1, 20)] private float airDrag = 1f;
        [SerializeField, Range(1, 20)] private float groundDrag = 8f;
    }
}