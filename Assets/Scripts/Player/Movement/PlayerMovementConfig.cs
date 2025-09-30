using UnityEngine;

namespace Player.Movement
{
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Configs/Player/Movement/PlayerMovementConfig")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [Header("Movement")] [SerializeField] [Range(10, 100)]
        private int crouchWalkSpeed = 25;

        [SerializeField] [Range(10, 100)] private float walkSpeed = 50f;
        [SerializeField] [Range(10, 200)] private float sprintSpeed = 75f;
        [SerializeField] [Range(1, 100)] private float movementMultiplier = 10f;

        [Header("Jumping")] [SerializeField] [Range(1, 100)]
        private float jumpForce = 5f;

        [SerializeField] [Range(1, 100)] private float jumpMultiplier = 10f;
        [SerializeField] [Range(.001f, 20)] private float airDrag = 1f;
        [SerializeField] [Range(.001f, 20)] private float groundDrag = 8f;

        [Header("Crouching")] [SerializeField] [Range(0.1f, 5f)]
        private float crouchHeight = 0.5f;

        [SerializeField] [Range(0.1f, 5f)] private float crouchCameraPosition = 0.5f;
        [SerializeField] [Range(0.1f, 1f)] private float initialHeight = 1.0f;
        [SerializeField] [Range(0.1f, 10f)] private float initialCrouchCameraPosition = 0.5f;
        [SerializeField] [Range(0.1f, 10f)] private float crouchSpeed = .75f;
        [SerializeField] [Range(0.1f, 10f)] private float crouchCheckRadius = 1f;

        [SerializeField] private LayerMask excludedLayers = (1 << 6) |
                                                            (1 << 2) |
                                                            (1 << 4) |
                                                            (1 << 1) |
                                                            (1 << 5);

        [SerializeField] private Vector3 crouchCheckOffset = new(0f, 0f, 0f);
        public int CrouchWalkSpeed => crouchWalkSpeed;
        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float MovementMultiplier => movementMultiplier;
        public float JumpForce => jumpForce;
        public float JumpMultiplier => jumpMultiplier;
        public float AirDrag => airDrag;
        public float GroundDrag => groundDrag;

        public float CrouchHeight => crouchHeight;

        public float InitialHeight => initialHeight;

        public float CrouchSpeed => crouchSpeed;

        public float CrouchCheckRadius => crouchCheckRadius;
        public int ExcludedLayers => excludedLayers;

        public Vector3 CrouchCheckOffset => crouchCheckOffset;
        public float CrouchCameraPosition => crouchCameraPosition;
        public float InitialCrouchCameraPosition => initialCrouchCameraPosition;
    }
}