using StateMachine;

namespace Player.States
{
    public class CrouchState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        public CrouchState(PlayerStateMachine sm) => _sm = sm;
        
        
        public override void Enter()
        {
            _sm.PlayerMovement.Crouch();
        }

        public override void Update()
        {
            _sm.PlayerLooking.Lean();
        }

        public override void Exit()
        {
            _sm.PlayerMovement.UnCrouch();
        }
    }
    
    
}