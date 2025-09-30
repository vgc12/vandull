using General;
using Player.Looking;
using Player.Movement;
using Player.States;
using StateMachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Player
{
    [RequireComponent(typeof(GroundChecker), typeof(PlayerMovement), typeof(PlayerLooking))]
    public class PlayerStateMachine : MonoBehaviour, IKillable, IDamageable
    {
        [SerializeField] private bool invulnerable;


        private GroundChecker _groundChecker;
        private StateMachine.StateMachine _stateMachine;


        public PlayerMovement PlayerMovement { get; private set; }

        public PlayerLooking PlayerLooking { get; private set; }

        private bool IsGroundedAndNotCrouching =>
            _groundChecker.IsGrounded && !PlayerMovement.CrouchPressed;

        private bool IsGroundedAndCrouching =>
            _groundChecker.IsGrounded && (PlayerMovement.CrouchPressed || !IsAtNormalHeight);

        private bool IsMoving => PlayerMovement.MoveInput != Vector2.zero;

        private bool IsAtNormalHeight =>
            Mathf.Approximately(PlayerMovement.config.InitialHeight,
                PlayerMovement.PlayerModel.localScale.y);

        private void Awake()
        {
            _groundChecker = GetComponent<GroundChecker>();
            PlayerMovement = GetComponent<PlayerMovement>();
            PlayerLooking = GetComponent<PlayerLooking>();


            Health = 100;


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

        public void TakeDamage(float amount, Vector3 direction)
        {
            if (Invulnerable) return;

            VandullLogger.Log($"Player took {amount} damage");
            Health -= amount;
            if (Health <= 0) Die();
        }

        public bool Invulnerable => invulnerable;
        public float Health { get; set; }

        public void Die()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        private void InitializeStateMachine()
        {
            var movementStates = Factory.Create(this);

            _stateMachine = new StateMachine.StateMachine();

            CreateAnyTransitions(movementStates);
            _stateMachine.SetState(movementStates.IdleState);
        }

        private void CreateAnyTransitions(Factory states)
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


        private class Factory
        {
            public IdleState IdleState { get; private init; }
            public WalkState WalkState { get; private init; }
            public SprintState SprintState { get; private init; }
            public JumpState JumpState { get; private init; }
            public CrouchState CrouchState { get; private init; }
            public IState CrouchWalkState { get; private init; }

            public static Factory Create(PlayerStateMachine sm)
            {
                return new Factory
                {
                    IdleState = new IdleState(sm),
                    WalkState = new WalkState(sm),
                    SprintState = new SprintState(sm),
                    JumpState = new JumpState(sm),
                    CrouchState = new CrouchState(sm),
                    CrouchWalkState = new CrouchWalkState(sm)
                };
            }
        }
    }
}