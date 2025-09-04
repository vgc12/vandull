using StateMachines;

namespace Player.Movement
{
    public class SprintState : BaseState
    {
        private readonly PlayerMovement _playerMovement;

        public SprintState(PlayerMovement pm)
        {
            _playerMovement = pm;
        }
        
        public override void Enter()
        {
          
        }

        public override void Update()
        {
            _playerMovement.ApplyDrag();
        }

        public override void FixedUpdate()
        {
            _playerMovement.Sprint();
        }
    }
}