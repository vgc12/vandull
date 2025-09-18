using System;
using System.Collections;
using System.Linq;
using Attributes;
using General;
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

        [SerializeField, Required] private Transform playerModel;
        [SerializeField, Required] private Transform crouchPositionTransform;
        [SerializeField, Required] private Transform headCheckTransform;

        [ScriptableObjectDropdown] public PlayerMovementConfig config;


        private Rigidbody _rigidbody;


        private GroundChecker _groundChecker;

        private InputManager _inputManager;

        private Coroutine _crouchCoroutine;


        public bool ObjectAbove { get; private set; }

        public bool SprintPressed { get; private set; }

        public bool JumpPressed { get; private set; }

        public Vector2 MoveInput { get; private set; }

        public bool CrouchPressed { get; private set; }

        public Transform PlayerModel => playerModel;

        private readonly RaycastHit[] _crouchCheckHits = new RaycastHit[1];

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
            
            playerModel.localScale = new Vector3(1, config.InitialHeight, 1);
            crouchPositionTransform.localPosition = new Vector3(crouchPositionTransform.localPosition.x,
                config.InitialCrouchCameraPosition,
                crouchPositionTransform.localPosition.z);
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
            Gizmos.color = Color.green;
            Gizmos.DrawLine(headCheckTransform.position,
                headCheckTransform.position + Vector3.up * (config.InitialCrouchCameraPosition - crouchPositionTransform.localPosition.y));
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
            _rigidbody.AddForce(Vector3.up * (config.JumpForce * config.JumpMultiplier), ForceMode.Impulse);
            _rigidbody.AddForce(_rigidbody.linearVelocity * (config.JumpForce / 4f * config.JumpMultiplier),
                ForceMode.Impulse);
        }

        #endregion


        public void Crouch()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);

            _crouchCoroutine = StartCoroutine(SetPlayerHeight(config.CrouchHeight, config.CrouchCameraPosition));
        }

        public void UnCrouch()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine =
                StartCoroutine(SetPlayerHeight(config.InitialHeight, config.InitialCrouchCameraPosition));
        }


        private IEnumerator SetPlayerHeight(float height, float position)
        {
            float t = 0;
            var currentHeight = PlayerModel.localScale.y;
            var crouchPosition = crouchPositionTransform.localPosition.y;

            while (t < 1)
            {
                var size = Physics.SphereCastNonAlloc(config.CrouchCheckOffset + headCheckTransform.position,
                    config.CrouchCheckRadius, Vector3.up, _crouchCheckHits,
                    config.InitialCrouchCameraPosition - crouchPositionTransform.localPosition.y,
                    ~config.ExcludedLayers);
                /*
                var size = Physics.OverlapSphereNonAlloc(config.CrouchCheckOffset + headCheckTransform.position,
                    config.CrouchCheckRadius, _crouchCheckCollider, ~config.ExcludedLayers);
*/

                if (size > 0)
                {
                    VandullLogger.Log(_crouchCheckHits[0].collider.name);
                    ObjectAbove = true;
                    yield return null;
                    continue;
                }

                ObjectAbove = false;

                t += Time.deltaTime / config.CrouchSpeed;

                var newHeight = Mathf.SmoothStep(currentHeight, height, t);
                var newPosition = Mathf.SmoothStep(crouchPosition, position, t);
                PlayerModel.localScale = new Vector3(1, newHeight, 1);
                crouchPositionTransform.localPosition = new Vector3(crouchPositionTransform.localPosition.x,
                    newPosition,
                    crouchPositionTransform.localPosition.z);
                yield return null;
            }

            _crouchCoroutine = null;
        }
    }
}