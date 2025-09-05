using StateMachines;

namespace Player.Movement.States
{
    public class IdleState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public IdleState(PlayerStateMachine sm)
        {
            _sm = sm;
        }

        
        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            
            _sm.PlayerLooking.Look();
        
            _sm.PlayerLooking.Sway(_sm.PlayerLooking.swayConfig.IdleSway);
            _sm.PlayerLooking.Lean();
        }
    }
}