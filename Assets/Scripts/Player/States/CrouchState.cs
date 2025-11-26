using EventBus;
using General.Logging;
using Reflex.Attributes;
using Reflex.Core;
using StateMachine;

namespace Player.States
{
    public sealed class CrouchState : BaseState
    {
        [Inject] private readonly ILogger _logger;

        private readonly PlayerStateMachine _sm;
        private readonly PlayerMovementEnteredEvent _stateEntered;
        private readonly PlayerMovementExitedEvent _stateExited;

        public CrouchState(PlayerStateMachine sm)
        {
            _sm = sm;
            _logger = Container.ProjectContainer.Resolve<ILogger>();
            _stateEntered = new PlayerMovementEnteredEvent(sm.PlayerMovement.Rigidbody, typeof(CrouchState));
            _stateExited = new PlayerMovementExitedEvent(sm.PlayerMovement.Rigidbody, typeof(CrouchState));
        }


        public override void Enter()
        {
            EventBus<PlayerMovementEnteredEvent>.Raise(_stateEntered);
            _sm.PlayerMovement.Crouch();
            _logger.Log("Crouching");
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
            _sm.PlayerMovement.UnCrouch();
        }
    }
}