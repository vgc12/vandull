using EventBus;
using StateMachine;

namespace Player.States
{
    public class SprintState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        private readonly PlayerMovementEnteredEvent _stateEntered;
        private readonly PlayerMovementExitedEvent _stateExited;

        public SprintState(PlayerStateMachine pm)
        {
            _sm = pm;
            _stateEntered = new PlayerMovementEnteredEvent(_sm.PlayerMovement.Rigidbody, typeof(SprintState));
            _stateExited = new PlayerMovementExitedEvent(_sm.PlayerMovement.Rigidbody, typeof(SprintState));
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
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.SprintSpeed);
        }

        public override void Exit()
        {
            EventBus<PlayerMovementExitedEvent>.Raise(_stateExited);
        }
    }
}