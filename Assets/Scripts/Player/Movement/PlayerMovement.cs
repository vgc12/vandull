using System;
using System.Collections;
using System.Linq;
using Attributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Player.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(InputManager), typeof(GroundChecker))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Transforms")] [SerializeField, Required]
        private Transform orientation;

        [SerializeField, Required] private Transform crouchTransform;
        [SerializeField, Required] private Transform headCheckTransform;

        [Required, ScriptableObjectDropdown] public PlayerMovementConfig config;



        private Rigidbody _rigidbody;


        private GroundChecker _groundChecker;

        private InputManager _inputManager;

        private Coroutine _crouchCoroutine;

        public bool SprintPressed { get; private set; }

        public bool JumpPressed { get; private set; }

        public Vector2 MoveInput { get; private set; }

        public bool CrouchPressed { get; private set; }

        public Transform CrouchTransform => crouchTransform;

        private readonly Collider[] _crouchCheckCollider = new Collider[1];

        #region UnityFunctions

        private void Start()
        {
            _groundChecker = GetComponent<GroundChecker>();

            _rigidbody = GetComponent<Rigidbody>();

            _inputManager = GetComponent<InputManager>();

            _inputManager.InputActions.Player.Move.performed += OnMoveInput;
            _inputManager.InputActions.Player.Move.canceled += OnMoveInput;

            _inputManager.InputActions.Player.Jump.performed += OnJumpInput;
            _inputManager.InputActions.Player.Jump.canceled += OnJumpInput;

            _inputManager.InputActions.Player.Sprint.performed += OnSprintInput;
            _inputManager.InputActions.Player.Sprint.canceled += OnSprintInput;

            _inputManager.InputActions.Player.Crouch.performed += OnCrouchInput;
            _inputManager.InputActions.Player.Crouch.canceled += OnCrouchInput;
        }


        private void OnDestroy()
        {
            _inputManager.InputActions.Player.Move.performed -= OnMoveInput;
            _inputManager.InputActions.Player.Move.canceled -= OnMoveInput;

            _inputManager.InputActions.Player.Jump.performed -= OnJumpInput;
            _inputManager.InputActions.Player.Jump.canceled -= OnJumpInput;

            _inputManager.InputActions.Player.Sprint.performed -= OnSprintInput;
            _inputManager.InputActions.Player.Sprint.canceled -= OnSprintInput;

            _inputManager.InputActions.Player.Crouch.performed -= OnCrouchInput;
            _inputManager.InputActions.Player.Crouch.canceled -= OnCrouchInput;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(config.CrouchCheckOffset + headCheckTransform.position, config.CrouchCheckRadius);
        }

        #endregion

        #region ControlFunctions

        private void OnSprintInput(InputAction.CallbackContext obj)
        {
            SprintPressed = obj.performed;
        }

        private void OnCrouchInput(InputAction.CallbackContext obj)
        {
            CrouchPressed = obj.performed;
        }

        private void OnJumpInput(InputAction.CallbackContext obj)
        {
            JumpPressed = obj.performed;
            
        }

        private void OnMoveInput(InputAction.CallbackContext context)
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
            _rigidbody.AddForce(Vector3.up  * (config.JumpForce * config.JumpMultiplier), ForceMode.Impulse);
            _rigidbody.AddForce(_rigidbody.linearVelocity * (config.JumpForce/4f * config.JumpMultiplier), ForceMode.Impulse);
        }

        #endregion


        public void Crouch()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);

            _crouchCoroutine = StartCoroutine(SetPlayerHeight(config.CrouchHeight));
            
        }

        public void UnCrouch()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine = StartCoroutine(SetPlayerHeight(config.InitialHeight));
        }

        private IEnumerator SetPlayerHeight(float height)
        {
            float t = 0;
            var currentHeight = CrouchTransform.localScale.y;

            while (t < 1)
            {
                var size = Physics.OverlapSphereNonAlloc(config.CrouchCheckOffset + headCheckTransform.position, config.CrouchCheckRadius, _crouchCheckCollider, ~config.ExcludedLayers);

                
                if (size > 0)
                {
                    yield return null;
                    continue;
                }

                t += Time.deltaTime / config.CrouchSpeed;
                
                var newHeight = Mathf.SmoothStep(currentHeight, height, t);
               CrouchTransform.localScale = new Vector3(1, newHeight, 1);
                yield return null;
            }

            _crouchCoroutine = null;
        }
    }
}