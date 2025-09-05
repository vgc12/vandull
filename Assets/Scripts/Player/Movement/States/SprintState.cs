using StateMachines;

namespace Player.Movement
{
    public class SprintState : BaseState
    {
        private readonly PlayerStateMachine _sm;

        public SprintState(PlayerStateMachine pm)
        {
            _sm = pm;
        }
        
        public override void Enter()
        {
          
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Sprint();
        }
    }
}