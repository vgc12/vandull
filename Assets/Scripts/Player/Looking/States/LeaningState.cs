using StateMachines;

namespace Player.PlayerLooking.States
{
    public class LeaningState : BaseState
    {
        private readonly PlayerLooking _playerLooking;
        public LeaningState(PlayerLooking playerLooking) 
        {
            _playerLooking = playerLooking;
        }

        public override void Enter()
        {
          
        }

        public override void Update()
        {
            _playerLooking.Lean();
            _playerLooking.Look();
         
        }

        public override void Exit()
        {
            _playerLooking.StopLean();
        }
    }
}