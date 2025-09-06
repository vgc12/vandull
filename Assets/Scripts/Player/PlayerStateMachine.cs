using Player.Looking;
using Player.Movement;
using Player.Movement.States;
using StateMachines;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(GroundChecker), typeof(PlayerMovement), typeof(PlayerLooking))]
    public class PlayerStateMachine : MonoBehaviour
    {
        private StateMachine _stateMachine;

      
        public PlayerMovement PlayerMovement { get; private set; }

        public PlayerLooking PlayerLooking { get; private set; }


        private GroundChecker _groundChecker;

        private class MovementStates
        {
            public IdleState IdleState { get; init; }
            public WalkState WalkState { get; init; }
            public SprintState SprintState { get; init; }
            public JumpState JumpState { get; init; }
            
            public static MovementStates Create(PlayerStateMachine sm) => new()
            {
                IdleState = new IdleState(sm),
                WalkState = new WalkState(sm),
                SprintState = new SprintState(sm),
                JumpState = new JumpState(sm)
            };
        }


        private void InitializeStateMachine()
        {
            var movementStates = MovementStates.Create(this);

            _stateMachine = new StateMachine();

            CreateIdleTransitions(movementStates);

            CreateWalkTransitions(movementStates);

            CreateSprintTransitions(movementStates);

            CreateJumpTransitions(movementStates);


            _stateMachine.SetState(movementStates.IdleState);
        }

        private void CreateIdleTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.IdleState, states.WalkState,
                new FuncPredicate(() => PlayerMovement.MoveInput != Vector2.zero && _groundChecker.IsGrounded));
            _stateMachine.AddTransition(states.IdleState, states.WalkState,
                new FuncPredicate(() =>
                    PlayerMovement.MoveInput != Vector2.zero && _groundChecker.IsGrounded && PlayerMovement.SprintPressed));
            _stateMachine.AddTransition(states.IdleState, states.JumpState,
                new FuncPredicate(() => _groundChecker.IsGrounded && PlayerMovement.JumpPressed));
        }

        private void CreateWalkTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.WalkState, states.IdleState,
                new FuncPredicate(() => PlayerMovement.MoveInput == Vector2.zero && _groundChecker.IsGrounded));
            _stateMachine.AddTransition(states.WalkState, states.SprintState,
                new FuncPredicate(() => PlayerMovement.MoveInput != Vector2.zero && _groundChecker.IsGrounded && PlayerMovement.SprintPressed));
            _stateMachine.AddTransition(states.WalkState, states.JumpState,
                new FuncPredicate(() => _groundChecker.IsGrounded && PlayerMovement.JumpPressed));
        }

        private void CreateSprintTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.SprintState, states.IdleState,
                new FuncPredicate(() => _groundChecker.IsGrounded && PlayerMovement.MoveInput == Vector2.zero));
            _stateMachine.AddTransition(states.SprintState, states.WalkState,
                new FuncPredicate(() => _groundChecker.IsGrounded && !PlayerMovement.SprintPressed && PlayerMovement.MoveInput != Vector2.zero));
            _stateMachine.AddTransition(states.SprintState, states.JumpState,
                new FuncPredicate(() => _groundChecker.IsGrounded && PlayerMovement.JumpPressed));
        }

        private void CreateJumpTransitions(MovementStates states)
        {
            _stateMachine.AddTransition(states.JumpState, states.IdleState,
                new FuncPredicate(() => _groundChecker.IsGrounded && !PlayerMovement.JumpPressed && PlayerMovement.MoveInput == Vector2.zero));
            _stateMachine.AddTransition(states.JumpState, states.SprintState,
                new FuncPredicate(() =>
                    _groundChecker.IsGrounded && !PlayerMovement.JumpPressed && PlayerMovement.MoveInput != Vector2.zero && PlayerMovement.SprintPressed));
            _stateMachine.AddTransition(states.JumpState, states.WalkState,
                new FuncPredicate(() => _groundChecker.IsGrounded && !PlayerMovement.JumpPressed && PlayerMovement.MoveInput != Vector2.zero));
        }


        private void Awake()
        {
            _groundChecker = GetComponent<GroundChecker>();
            PlayerMovement = GetComponent<PlayerMovement>();
            PlayerLooking = GetComponent<PlayerLooking>();
            
          
            InitializeStateMachine();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
        }
    }
}