using StateMachines;

namespace Player.Movement.States
{
    public class WalkState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public WalkState(PlayerStateMachine sm)
        {
            _sm = sm;
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            _sm.PlayerLooking.Look();
            _sm.PlayerLooking.CameraBob(_sm.PlayerLooking.cameraBobConfig.walkBob);
            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.WalkSpeed);
            
        }
    }
}