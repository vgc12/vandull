using StateMachines;

namespace Player.PlayerLooking.States
{
    public class IdleState : BaseState
    {
        private readonly Looking.PlayerLooking _playerLooking;
        public IdleState(Looking.PlayerLooking playerLooking) 
        {
            _playerLooking = playerLooking;
        }

        
        public override void Update()
        {
            _playerLooking.Look();
            _playerLooking.Sway();
        }
    }
    
    
    
    public class WalkingState : BaseState
    {
        private readonly Looking.PlayerLooking _playerLooking;
        public WalkingState(Looking.PlayerLooking playerLooking) 
        {
            _playerLooking = playerLooking;
        }

        
        public override void Update()
        {
            _playerLooking.Look();
            _playerLooking.Sway(_playerLooking.swayConfig.walkSwayMultiplier);
        }
    }
}