using EventBus;
using StateMachine;

namespace Player.States
{
    public class IdleState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        private readonly PlayerMovementEnteredEvent _stateEntered;
        private readonly PlayerMovementExitedEvent _stateExited;


        public IdleState(PlayerStateMachine sm)
        {
            _sm = sm;
            _stateEntered = new PlayerMovementEnteredEvent(_sm.PlayerMovement.Rigidbody, typeof(IdleState));
            _stateExited = new PlayerMovementExitedEvent(_sm.PlayerMovement.Rigidbody, typeof(IdleState));
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


        public override void Exit()
        {
            EventBus<PlayerMovementExitedEvent>.Raise(_stateExited);
        }
    }
}