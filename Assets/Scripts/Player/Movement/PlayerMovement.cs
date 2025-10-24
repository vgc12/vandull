using System.Collections;
using Attributes;
using Player.Input;
using Reflex.Attributes;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Player.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(GroundChecker))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Transforms")] [SerializeField] [Required]
        private Transform orientation;

        [SerializeField] [Required] private Transform playerModel;
        [SerializeField] [Required] private Transform crouchPositionTransform;
        [SerializeField] [Required] private Transform headCheckTransform;
        [SerializeField] private float maxSlopeAngle;

        [ScriptableObjectDropdown] public PlayerMovementConfig config;

        private readonly RaycastHit[] _crouchCheckHits = new RaycastHit[1];

        [Inject] private readonly ILogger _logger;

        private Coroutine _crouchCoroutine;

        private GroundChecker _groundChecker;

        private RaycastHit _slopeHit;
        


        public Rigidbody Rigidbody { get; private set; }

        public bool ObjectAbove { get; private set; }

        public bool SprintPressed { get; private set; }

        public bool JumpPressed { get; private set; }

        public Vector2 MoveInput { get; private set; }

        public bool CrouchPressed { get; private set; }

        public Transform PlayerModel => playerModel;
        
        public bool IsAiming { get; set; }


        public void Crouch()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine = StartCoroutine(SetPlayerHeight(config.CrouchHeight, config.CrouchCameraPosition));
        }

        public void UnCrouch()
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _crouchCoroutine = StartCoroutine(SetPlayerHeight(config.InitialHeight, config.InitialCrouchCameraPosition));
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

                if (size > 0)
                {
                    _logger.Log(_crouchCheckHits[0].collider.name);
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

        #region UnityFunctions

        [Inject] private readonly IPlayerInput _input;

        private void Start()
        {
            _groundChecker = GetComponent<GroundChecker>();

            Rigidbody = GetComponent<Rigidbody>();

            _input.Crouch += OnCrouchInput;

            _input.Jump += OnJumpInput;

            _input.Sprint += OnSprintInput;

            _input.Move += OnMoveInput;
            
            _input.Aim += OnAimInput;

            playerModel.localScale = new Vector3(1, config.InitialHeight, 1);
            crouchPositionTransform.localPosition = new Vector3(crouchPositionTransform.localPosition.x,
                config.InitialCrouchCameraPosition,
                crouchPositionTransform.localPosition.z);
        }

        private void OnAimInput(bool value)
        {
            IsAiming = value;
        }

        private void OnMoveInput(Vector2 value)
        {
            MoveInput = value;
        }


        private void OnCrouchInput(bool value)
        {
            CrouchPressed = value;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(config.CrouchCheckOffset + headCheckTransform.position, config.CrouchCheckRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(headCheckTransform.position,
                headCheckTransform.position + Vector3.up *
                (config.InitialCrouchCameraPosition - crouchPositionTransform.localPosition.y));
            //Gizmos.DrawRay(playerModel.transform.position, Vector3.down * (playerModel.localScale.y * 0.5f + 0.3f));
            Gizmos.DrawRay(transform.position, _slopeDir * 20f);
        }

        #endregion

        #region ControlFunctions

        private void OnSprintInput(bool value)
        {
            SprintPressed = value;
        }


        private void OnJumpInput(bool value)
        {
            JumpPressed = value;
        }

        #endregion


        #region MovementFunctions

        private Vector3 _slopeDir;

        public void Move(float speed)
        {
            var moveDirection = orientation.forward * MoveInput.y + orientation.right * MoveInput.x;
            moveDirection.Normalize();


            if (OnSlope())
            {
                var slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, _slopeHit.normal).normalized;
                _slopeDir = slopeMoveDirection;

                ApplyMovement(slopeMoveDirection * (speed * config.SlopeMultiplier));


                // if (_rigidbody.linearVelocity.y > 0) _rigidbody.AddForce(Vector3.down * 5f, ForceMode.Force);
                return;
            }

            ApplyMovement(moveDirection * (speed * config.MovementMultiplier));
        }

        private bool OnSlope()
        {
            if (Physics.Raycast(playerModel.position, Vector3.down, out _slopeHit,
                    playerModel.localScale.y * 0.5f + 0.3f, ~LayerMask.GetMask("Player")))
            {
                var angle = Vector3.Angle(Vector3.up, _slopeHit.normal);

                return angle < maxSlopeAngle && angle != 0;
            }

            return false;
        }

        public void ApplyMovement(Vector3 movement)
        {
            Rigidbody.AddForce(movement, ForceMode.Force);
        }

        public void ApplyDrag()
        {
            Rigidbody.linearDamping = _groundChecker.IsGrounded ? config.GroundDrag : config.AirDrag;
        }


        public void Jump()
        {
            Rigidbody.AddForce(Vector3.up * (config.JumpForce * config.JumpMultiplier), ForceMode.Impulse);
            Rigidbody.AddForce(Rigidbody.linearVelocity / 3 * (config.JumpForce * config.JumpMultiplier),
                ForceMode.Impulse);
        }

        #endregion
    }
}