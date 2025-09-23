using System;
using General;
using Player.Looking;
using Player.Movement;
using Player.States;
using StateMachine;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(GroundChecker), typeof(PlayerMovement), typeof(PlayerLooking))]
    public class PlayerStateMachine : MonoBehaviour
    {
        private StateMachine.StateMachine _stateMachine;


        public PlayerMovement PlayerMovement { get; private set; }

        public PlayerLooking PlayerLooking { get; private set; }


        private GroundChecker _groundChecker;

        private class MovementStates
        {
            public IdleState IdleState { get; private init; }
            public WalkState WalkState { get; private init; }
            public SprintState SprintState { get; private init; }
            public JumpState JumpState { get; private init; }
            public CrouchState CrouchState { get; private init; }
            public IState CrouchWalkState { get; private init; }

            public static MovementStates Create(PlayerStateMachine sm) => new()
            {
                IdleState = new IdleState(sm),
                WalkState = new WalkState(sm),
                SprintState = new SprintState(sm),
                JumpState = new JumpState(sm),
                CrouchState = new CrouchState(sm),
                CrouchWalkState = new CrouchWalkState(sm)
            };
        }


        private void InitializeStateMachine()
        {
            var movementStates = MovementStates.Create(this);

            _stateMachine = new StateMachine.StateMachine();

            CreateAnyTransitions(movementStates);
            _stateMachine.SetState(movementStates.IdleState);
        }

        private bool IsGroundedAndNotCrouching =>
            _groundChecker.IsGrounded && !PlayerMovement.CrouchPressed ;

        private bool IsGroundedAndCrouching =>
            _groundChecker.IsGrounded && (PlayerMovement.CrouchPressed || !IsAtNormalHeight);

        private bool IsMoving => PlayerMovement.MoveInput != Vector2.zero;

        private bool IsAtNormalHeight =>
            Mathf.Approximately(PlayerMovement.config.InitialHeight,
                PlayerMovement.PlayerModel.localScale.y);

        private void CreateAnyTransitions(MovementStates states)
        {
            _stateMachine.AddAnyTransition(states.JumpState,
                new FuncPredicate(() =>
                    IsGroundedAndNotCrouching && !PlayerMovement.ObjectAbove && PlayerMovement.JumpPressed &&
                    IsAtNormalHeight));
            _stateMachine.AddAnyTransition(states.SprintState,
                new FuncPredicate(() =>
                    IsGroundedAndNotCrouching && !PlayerMovement.ObjectAbove && IsMoving &&
                    PlayerMovement.SprintPressed));
            _stateMachine.AddAnyTransition(states.WalkState,
                new FuncPredicate(() =>
                    IsGroundedAndNotCrouching && !PlayerMovement.ObjectAbove && IsMoving &&
                    !PlayerMovement.SprintPressed));
            _stateMachine.AddAnyTransition(states.IdleState,
                new FuncPredicate(() => IsGroundedAndNotCrouching && !PlayerMovement.ObjectAbove && !IsMoving));
            _stateMachine.AddAnyTransition(states.CrouchState,
                new FuncPredicate(() => IsGroundedAndCrouching && !IsMoving));
            _stateMachine.AddAnyTransition(states.CrouchWalkState,
                new FuncPredicate(() => IsGroundedAndCrouching && IsMoving));
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