using EventBus;
using General;
using Levels.Strategies;
using Player.Looking;
using Player.Movement;
using Player.States;
using Reflex.Attributes;
using Shared;
using StateMachine;
using UnityEngine;
using ILogger = General.Logging.ILogger;

namespace Player
{
    [RequireComponent(typeof(GroundChecker), typeof(PlayerMovement), typeof(PlayerLooking))]
    public class PlayerStateMachine : MonoBehaviour, IKillable, IDamageable
    {
        [SerializeField] private bool invulnerable;

        [SerializeField] private float health;

        [SerializeField] private float maxHealth = 100f;

        [Inject] private readonly ILogger _logger;


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


            health = 100f;


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
            if (Invulnerable || IsDead) return;

            _logger.Log($"Player took {amount} damage");
            health -= amount;
            EventBus<PlayerHitEvent>.Raise(new PlayerHitEvent(health));
            if (Health <= 0) Die();
        }

        public bool Invulnerable => invulnerable;

        public float Health
        {
            get => health;
            set => health = Mathf.Clamp(value, 0, maxHealth);
        }

        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }

        public void Die()
        {
            IsDead = true;
            EventBus<PlayerKilledEvent>.Raise(new PlayerKilledEvent(gameObject, transform.position, ""));
        }


        private void InitializeStateMachine()
        {
            var movementStates = new Factory(this).Create();

            _stateMachine = new StateMachine.StateMachine();

            CreateAnyTransitions(movementStates);
            _stateMachine.SetState(movementStates.IdleState);
        }

        private void CreateAnyTransitions(PlayerStates states)
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


        private class PlayerStates
        {
            public IdleState IdleState { get; init; }
            public WalkState WalkState { get; init; }
            public SprintState SprintState { get; init; }
            public JumpState JumpState { get; init; }
            public CrouchState CrouchState { get; init; }
            public IState CrouchWalkState { get; init; }
        }

        private class Factory : IFactory<PlayerStates>
        {
            private readonly PlayerStateMachine _sm;

            public Factory(PlayerStateMachine sm)
            {
                _sm = sm;
            }

            public PlayerStates Create()
            {
                return new PlayerStates
                {
                    IdleState = new IdleState(_sm),
                    WalkState = new WalkState(_sm),
                    SprintState = new SprintState(_sm),
                    JumpState = new JumpState(_sm),
                    CrouchState = new CrouchState(_sm),
                    CrouchWalkState = new CrouchWalkState(_sm)
                };
            }
        }
    }
}