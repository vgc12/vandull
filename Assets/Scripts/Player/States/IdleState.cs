using StateMachine;

namespace Player.States
{
    public class IdleState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public IdleState(PlayerStateMachine sm)
        {
            _sm = sm;
        }

        public override void Enter()
        {
            _sm.PlayerLooking.CameraBobber.StopBobbing();
        }


        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            
            _sm.PlayerLooking.Look();
        
            var ce = _sm.PlayerLooking.objectSwayer;
       
       
            
            ce.Sway(ce.swayConfig);
            
            _sm.PlayerLooking.Lean();
        }
        
        
        public override void Exit()
        {
      
        }
    }
}