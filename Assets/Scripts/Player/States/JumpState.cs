using EventBus;
using StateMachine;

namespace Player.States
{
    public class JumpState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        private readonly PlayerMovementEnteredEvent _stateEntered;
        private readonly PlayerMovementExitedEvent _stateExited;

        public JumpState(PlayerStateMachine sm)
        {
            _sm = sm;
            _stateEntered = new PlayerMovementEnteredEvent(_sm.PlayerMovement.Rigidbody, typeof(JumpState));
            _stateExited = new PlayerMovementExitedEvent(_sm.PlayerMovement.Rigidbody, typeof(JumpState));
        }

        public override void Enter()
        {
            EventBus<PlayerMovementEnteredEvent>.Raise(_stateEntered);
            _sm.PlayerMovement.Jump();
        }


        public override void Update()
        {
            _sm.PlayerLooking.Look();
            _sm.PlayerMovement.ApplyDrag();
        }

        public override void Exit()
        {
            EventBus<PlayerMovementExitedEvent>.Raise(_stateExited);
        }
    }
}