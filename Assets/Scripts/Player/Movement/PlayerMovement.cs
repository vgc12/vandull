using System;
using Attributes;
using Player.PlayerLooking;
using StateMachines;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using StateMachine = StateMachines.StateMachine;

namespace Player.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(InputManager))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Required] private Transform orientation;
        [SerializeField, Range(10, 100)] private float walkSpeed = 50f;
        [SerializeField, Range(10, 200)] private float sprintSpeed = 75f;
        
        [Header("Jumping")]
        [SerializeField, Range(1, 10)] private float jumpForce = 5f;
        [SerializeField, Range(1,20)] private float airDrag = 1f;
        [SerializeField, Range(1,20)] private float groundDrag = 8f;
        
        [Header("Ground Check")]
        [SerializeField, Range(0.1f, 5f)] private float groundCheckRadius = 0.3f;
        [SerializeField] private Vector3 groundCheckOffset = new Vector3(0, .4f, 0);
        [SerializeField] private LayerMask excludedLayers;
        
        [Header("Other")]
        [SerializeField] private bool debugMode;
        
        private StateMachine _stateMachine;

        private InputManager _inputManager;

        private Rigidbody _rigidbody;
        
        private Vector2 _moveInput;

       private bool _isGrounded;

       private bool _jumpPressed;

       private bool _sprintPressed;
       
       private readonly Collider[] _groundedColliders = new Collider[8];
        
        
   

        public class MovementStates
        {
            public IdleState IdleState { get; init; }
            public WalkState WalkState { get; init; }
            public SprintState SprintState { get; init; }
            public JumpState JumpState { get; init; }
        }
      

        #region StateMachineInitia

        

      
        private void InitializeStateMachine()
        {

            var movementStates = new MovementStates
            {
                IdleState = new IdleState(this),
                WalkState = new WalkState(this),
                SprintState = new SprintState(this),
                JumpState = new JumpState(this)
            };
            
            
            _stateMachine = new StateMachine();
            
            CreateIdleTransitions(movementStates);
            
            CreateWalkTransitions(movementStates);
            
            CreateSprintTransitions(movementStates);
            
            CreateJumpTransitions(movementStates);
            
            
            _stateMachine.SetState(movementStates.IdleState);
        }

        private void CreateIdleTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.IdleState , states.WalkState,
                new FuncPredicate(() => _moveInput != Vector2.zero && _isGrounded));
            _stateMachine.AddTransition(states.IdleState, states.WalkState,
                new FuncPredicate(() => _moveInput != Vector2.zero && _isGrounded && _sprintPressed));
            _stateMachine.AddTransition(states.IdleState, states.JumpState,
                new FuncPredicate(() => _isGrounded && _jumpPressed));
        }

        private void CreateWalkTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.WalkState, states.IdleState, 
                new FuncPredicate(() => _moveInput == Vector2.zero && _isGrounded));
            _stateMachine.AddTransition(states.WalkState, states.SprintState,
                new FuncPredicate(() => _moveInput != Vector2.zero && _isGrounded && _sprintPressed));
            _stateMachine.AddTransition(states.WalkState, states.JumpState,
                new FuncPredicate(() => _isGrounded && _jumpPressed));
        }

        private void CreateSprintTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.SprintState,states.IdleState,
                new FuncPredicate(() => _isGrounded &&  _moveInput == Vector2.zero));
            _stateMachine.AddTransition(states.SprintState, states.WalkState,
                new FuncPredicate(() => _isGrounded && !_sprintPressed && _moveInput != Vector2.zero));
            _stateMachine.AddTransition(states.SprintState, states.JumpState, new FuncPredicate(() => _isGrounded && _jumpPressed));

        }

        private void CreateJumpTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.JumpState, states.IdleState,
                new FuncPredicate(() => _isGrounded && !_jumpPressed && _moveInput == Vector2.zero));
            _stateMachine.AddTransition(states.JumpState, states.SprintState, 
                new FuncPredicate(() => _isGrounded && !_jumpPressed && _moveInput != Vector2.zero && _sprintPressed));
            _stateMachine.AddTransition(states.JumpState, states.WalkState,
                new FuncPredicate(() => _isGrounded && !_jumpPressed && _moveInput != Vector2.zero));
        }
        
        
        
        #endregion

        #region UnityFunctions

        private void OnDrawGizmos()
        {
            if (!debugMode) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + groundCheckOffset, groundCheckRadius);
        }


        private void Awake()
        {
            InitializeStateMachine();

           
        }
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
            _inputManager = GetComponent<InputManager>();
            
            _inputManager.InputActions.Player.Move.performed += OnMove;
            _inputManager.InputActions.Player.Move.canceled += OnMove;
            
            _inputManager.InputActions.Player.Jump.performed += OnJump;
            _inputManager.InputActions.Player.Jump.canceled += OnJump;
            
            _inputManager.InputActions.Player.Sprint.performed += OnSprint;
            _inputManager.InputActions.Player.Sprint.canceled += OnSprint;
            
            
        }
        
        private void Update()
        {
            _stateMachine.Update();
          
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
        #endregion

        #region ControlFunctions

        

    
        private void OnSprint(InputAction.CallbackContext obj)
        {
            _sprintPressed = obj.performed;
        }

        private void OnJump(InputAction.CallbackContext obj)
        {
            _jumpPressed = obj.performed;
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            
            _moveInput = context.ReadValue<Vector2>();
     
        }

        #endregion

        #region MovementFunctions

        

        public void Walk()
        {
            var forwardMovement = orientation.forward * (_moveInput.y * walkSpeed);
            var rightMovement = orientation.right * (_moveInput.x * walkSpeed);
            
            ApplyMovement(forwardMovement, rightMovement);
        }

        public void Sprint()
        {
            var forwardMovement = orientation.forward * (_moveInput.y * sprintSpeed);
            var rightMovement = orientation.right * (_moveInput.x * sprintSpeed);
  
            ApplyMovement(forwardMovement, rightMovement);
        }
        
        
        public void ApplyMovement(Vector3 forwardMovement, Vector3 rightMovement)
        {
            _rigidbody.AddForce(forwardMovement, ForceMode.Force);
            _rigidbody.AddForce(rightMovement, ForceMode.Force);
        }
        
        public void ApplyDrag()
        {
            _rigidbody.linearDamping = _isGrounded ? groundDrag : airDrag;
        }
        
        public void CheckForGround() =>
            _isGrounded =
                Physics.OverlapSphereNonAlloc(transform.position + groundCheckOffset, groundCheckRadius, _groundedColliders, ~excludedLayers) >
                0;

        public void Jump()
        {
            _rigidbody.AddForce((transform.up + _rigidbody.linearVelocity.normalized ) * jumpForce, ForceMode.Impulse);
        }
        #endregion
    }
}
namespace System.Runtime.CompilerServices
{
}