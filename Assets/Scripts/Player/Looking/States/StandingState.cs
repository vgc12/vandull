using StateMachines;

namespace Player.PlayerLooking.States
{
    public class StandingState : BaseState
    {
        private readonly PlayerLooking _playerLooking;
        public StandingState(PlayerLooking playerLooking) 
        {
            _playerLooking = playerLooking;
        }

        
        public override void Update()
        {
            _playerLooking.Look();
        }
    }
    
    public class LeaningState : BaseState
    {
        private readonly PlayerLooking _playerLooking;
        public LeaningState(PlayerLooking playerLooking) 
        {
            _playerLooking = playerLooking;
        }

        
        public override void Update()
        {
            _playerLooking.Look();
        }
    }
}