
using System;
using Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Player.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(InputManager), typeof(GroundChecker))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Transforms")]
        [SerializeField, Required] private Transform orientation;
        [Required, ScriptableObjectDropdown] public PlayerMovementConfig config;
        
        [Header("Other")] 
        [SerializeField] private bool debugMode;

        private Rigidbody _rigidbody;


        private GroundChecker _groundChecker;

        private InputManager _inputManager;

        public bool SprintPressed { get; private set; }

        public bool JumpPressed { get; private set; }

        public Vector2 MoveInput { get; private set; }


        #region UnityFunctions

        private void Start()
        {
            _groundChecker = GetComponent<GroundChecker>();

            _rigidbody = GetComponent<Rigidbody>();

            _inputManager = GetComponent<InputManager>();

            _inputManager.InputActions.Player.Move.performed += OnMove;
            _inputManager.InputActions.Player.Move.canceled += OnMove;

            _inputManager.InputActions.Player.Jump.performed += OnJump;
            _inputManager.InputActions.Player.Jump.canceled += OnJump;

            _inputManager.InputActions.Player.Sprint.performed += OnSprint;
            _inputManager.InputActions.Player.Sprint.canceled += OnSprint;
        }

        #endregion

        #region ControlFunctions

        private void OnSprint(InputAction.CallbackContext obj)
        {
            SprintPressed = obj.performed;
        }

        private void OnJump(InputAction.CallbackContext obj)
        {
            JumpPressed = obj.performed;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        #endregion


        #region MovementFunctions


        
        public void Move(float speed)
        {
            var forwardMovement = orientation.forward * (MoveInput.y * speed * config.MovementMultiplier);
            var rightMovement = orientation.right * (MoveInput.x * speed * config.MovementMultiplier);

            ApplyMovement(forwardMovement, rightMovement);
        }
        


        public void ApplyMovement(Vector3 forwardMovement, Vector3 rightMovement)
        {
            _rigidbody.AddForce(forwardMovement, ForceMode.Force);
            _rigidbody.AddForce(rightMovement, ForceMode.Force);
        }

        public void ApplyDrag()
        {
            _rigidbody.linearDamping = _groundChecker.IsGrounded ? config.GroundDrag : config.AirDrag;
        }


        public void Jump()
        {
            _rigidbody.AddForce((transform.up + _rigidbody.linearVelocity.normalized) * config.JumpForce, ForceMode.Impulse);
        }

        #endregion
    }
}