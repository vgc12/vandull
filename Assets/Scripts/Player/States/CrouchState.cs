using General.Logging;
using Reflex.Attributes;
using Reflex.Core;
using StateMachine;

namespace Player.States
{
    public class CrouchState : BaseState
    {
        [Inject] private readonly ILogger _logger;

        private readonly PlayerStateMachine _sm;

        public CrouchState(PlayerStateMachine sm)
        {
            _sm = sm;
            _logger = Container.ProjectContainer.Resolve<ILogger>();
        }


        public override void Enter()
        {
            _sm.PlayerMovement.Crouch();
            _logger.Log("Crouching");
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();

            _sm.PlayerLooking.Look();

            var os = _sm.PlayerLooking.objectSwayer;


            os.Sway(_sm.PlayerLooking.swayConfig);

            _sm.PlayerLooking.Lean();
        }

        public override void Exit()
        {
            _sm.PlayerMovement.UnCrouch();
        }
    }
}