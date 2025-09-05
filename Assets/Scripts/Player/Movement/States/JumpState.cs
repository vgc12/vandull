using StateMachines;

namespace Player.Movement.States
{
    public class JumpState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public JumpState(PlayerStateMachine sm)
        {
            _sm = sm;
        }

        public override void Enter()
        {
            
            _sm.PlayerMovement.Jump();
        }
        
        
        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
        }
    }
}