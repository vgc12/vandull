using System;
using Attributes;
using Player;
using Player.Looking;
using Player.Movement;
using Player.Movement.States;
using Player.PlayerLooking;
using StateMachines;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using StateMachine = StateMachines.StateMachine;

public class PlayerStateMachine : MonoBehaviour
{
      
    private StateMachine _stateMachine;

    private InputManager _inputManager;
    public PlayerMovement PlayerMovement { get; private set; }
    public PlayerLooking PlayerLooking { get; private set; }
    
    private GroundChecker _groundChecker;
    
        public class MovementStates
        {
            
            public IdleState IdleState { get; init; }
            public WalkState WalkState { get; init; }
            public SprintState SprintState { get; init; }
            public JumpState JumpState { get; init; }
        }


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
                new FuncPredicate(() => PlayerMovement.MoveInput != Vector2.zero && _groundChecker.IsGrounded));
            _stateMachine.AddTransition(states.IdleState, states.WalkState,
                new FuncPredicate(() => PlayerMovement.MoveInput != Vector2.zero && _groundChecker.IsGrounded && _sprintPressed));
            _stateMachine.AddTransition(states.IdleState, states.JumpState,
                new FuncPredicate(() => _groundChecker.IsGrounded && PlayerMovement._jumpPressed));
        }

        private void CreateWalkTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.WalkState, states.IdleState, 
                new FuncPredicate(() => _moveInput == Vector2.zero && _groundChecker.IsGrounded));
            _stateMachine.AddTransition(states.WalkState, states.SprintState,
                new FuncPredicate(() => _moveInput != Vector2.zero && _groundChecker.IsGrounded && _sprintPressed));
            _stateMachine.AddTransition(states.WalkState, states.JumpState,
                new FuncPredicate(() => _groundChecker.IsGrounded && _jumpPressed));
        }

        private void CreateSprintTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.SprintState,states.IdleState,
                new FuncPredicate(() => _groundChecker.IsGrounded &&  _moveInput == Vector2.zero));
            _stateMachine.AddTransition(states.SprintState, states.WalkState,
                new FuncPredicate(() => _groundChecker.IsGrounded && !_sprintPressed && _moveInput != Vector2.zero));
            _stateMachine.AddTransition(states.SprintState, states.JumpState, new FuncPredicate(() => _groundChecker.IsGrounded && _jumpPressed));

        }

        private void CreateJumpTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.JumpState, states.IdleState,
                new FuncPredicate(() => _groundChecker.IsGrounded && !_jumpPressed && _moveInput == Vector2.zero));
            _stateMachine.AddTransition(states.JumpState, states.SprintState, 
                new FuncPredicate(() => _groundChecker.IsGrounded && !_jumpPressed && _moveInput != Vector2.zero && _sprintPressed));
            _stateMachine.AddTransition(states.JumpState, states.WalkState,
                new FuncPredicate(() => _groundChecker.IsGrounded && !_jumpPressed && _moveInput != Vector2.zero));
        }
        
        
        
        #endregion
}

namespace Player.Movement
{
    [RequireComponent(typeof(Rigidbody), typeof(InputManager), typeof(GroundChecker))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Required] private Transform orientation;
        [SerializeField, Range(10, 100)] private float walkSpeed = 50f;
        [SerializeField, Range(10, 200)] private float sprintSpeed = 75f;
        [SerializeField, Range(1,100)] private float movementMultiplier = 10f;
        
        [Header("Jumping")]
        [SerializeField, Range(1, 10)] private float jumpForce = 5f;
        [SerializeField, Range(1,20)] private float airDrag = 1f;
        [SerializeField, Range(1,20)] private float groundDrag = 8f;
        
        [Header("Other")]
        [SerializeField] private bool debugMode;
        


        private Rigidbody _rigidbody;

        public Vector2 MoveInput;

        public bool _jumpPressed;

       private bool _sprintPressed;
       private GroundChecker _groundChecker;


 

      

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
            
            MoveInput = context.ReadValue<Vector2>();
     
        }

        #endregion

        #region MovementFunctions

        

        public void Walk()
        {
            var forwardMovement = orientation.forward * (MoveInput.y * walkSpeed * movementMultiplier);
            var rightMovement = orientation.right * (MoveInput.x * walkSpeed * movementMultiplier);
            
            ApplyMovement(forwardMovement, rightMovement);
        }

        public void Sprint()
        {
            var forwardMovement = orientation.forward * (MoveInput.y * sprintSpeed * movementMultiplier);
            var rightMovement = orientation.right * (MoveInput.x * sprintSpeed * movementMultiplier);
  
            ApplyMovement(forwardMovement, rightMovement);
        }
        
        
        public void ApplyMovement(Vector3 forwardMovement, Vector3 rightMovement)
        {
            _rigidbody.AddForce(forwardMovement, ForceMode.Force);
            _rigidbody.AddForce(rightMovement, ForceMode.Force);
        }
        
        public void ApplyDrag()
        {
            _rigidbody.linearDamping = _groundChecker.IsGrounded ? groundDrag : airDrag;
        }
        
     

        public void Jump()
        {
            _rigidbody.AddForce((transform.up + _rigidbody.linearVelocity.normalized ) * jumpForce, ForceMode.Impulse);
        }
        #endregion
    }
}
