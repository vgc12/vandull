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
            _sm.PlayerLooking.CameraEffects.StopBobbing();
        }


        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            
            _sm.PlayerLooking.Look();
        
            var ce = _sm.PlayerLooking.CameraEffects;
       
       
            
            ce.Sway(ce.swayConfig.IdleSway);
            
            _sm.PlayerLooking.Lean();
        }
        
        
        public override void Exit()
        {
      
        }
    }
}