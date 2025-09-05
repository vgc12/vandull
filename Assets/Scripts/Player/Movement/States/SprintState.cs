using StateMachines;

namespace Player.Movement.States
{
    public class SprintState : BaseState
    {
        private readonly PlayerStateMachine _sm;

        public SprintState(PlayerStateMachine pm)
        {
            _sm = pm;
        }
        
        public override void Enter()
        {
          
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            
            _sm.PlayerLooking.Look();
            _sm.PlayerLooking.CameraBob(_sm.PlayerLooking.cameraBobConfig.sprintBob);
            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.SprintSpeed);
        }
    }
}