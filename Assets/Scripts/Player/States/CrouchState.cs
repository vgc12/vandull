using General;
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
            VandullLogger.Log("Crouching");
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