using EventBus;
using StateMachine;

namespace Player.States
{
    public sealed class WalkState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        private readonly PlayerMovementEnteredEvent _stateEntered;
        private readonly PlayerMovementExitedEvent _stateExited;

        public WalkState(PlayerStateMachine sm)
        {
            _sm = sm;
            _stateEntered = new PlayerMovementEnteredEvent(_sm.PlayerMovement.Rigidbody, typeof(WalkState));
            _stateExited = new PlayerMovementExitedEvent(_sm.PlayerMovement.Rigidbody, typeof(WalkState));
        }

        public override void Enter()
        {
            EventBus<PlayerMovementEnteredEvent>.Raise(_stateEntered);
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            _sm.PlayerLooking.Look();
            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.WalkSpeed);
        }


        public override void Exit()
        {
            EventBus<PlayerMovementExitedEvent>.Raise(_stateExited);
        }
    }
}