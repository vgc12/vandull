
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

            _inputManager.InputActions.Player.Move.performed += OnMovement;
            _inputManager.InputActions.Player.Move.canceled += OnMovement;

            _inputManager.InputActions.Player.Jump.performed += OnJumping;
            _inputManager.InputActions.Player.Jump.canceled += OnJumping;

            _inputManager.InputActions.Player.Sprint.performed += OnSprinting;
            _inputManager.InputActions.Player.Sprint.canceled += OnSprinting;
        }

        private void OnDestroy()
        {
            _inputManager.InputActions.Player.Move.performed -= OnMovement;
            _inputManager.InputActions.Player.Move.canceled -= OnMovement;
            
            _inputManager.InputActions.Player.Jump.performed -= OnJumping;
            _inputManager.InputActions.Player.Jump.canceled -= OnJumping;
            
            _inputManager.InputActions.Player.Sprint.performed -= OnSprinting;
            _inputManager.InputActions.Player.Sprint.canceled -= OnSprinting;
            
        }

        #endregion

        #region ControlFunctions

        private void OnSprinting(InputAction.CallbackContext obj)
        {
            SprintPressed = obj.performed;
        }

        private void OnJumping(InputAction.CallbackContext obj)
        {
            JumpPressed = obj.performed;
        }

        private void OnMovement(InputAction.CallbackContext context)
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
            
            _rigidbody.AddForce((transform.up + _rigidbody.linearVelocity.normalized) * 
                                ((_rigidbody.linearVelocity != Vector3.zero ? config.JumpForce / 2 : config.JumpForce)
                                 * config.JumpMultiplier), ForceMode.Impulse);
        }

        #endregion
    }
}