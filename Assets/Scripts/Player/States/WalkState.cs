using StateMachine;

namespace Player.States
{
    public class WalkState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public WalkState(PlayerStateMachine sm)
        {
            _sm = sm;
        }
        public override void Enter()
        {
      
        }
        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            _sm.PlayerLooking.Look();
            var cameraEffects = _sm.PlayerLooking.CameraEffects;
            cameraEffects.CameraBob(cameraEffects.cameraBobConfig.walkConfig);
            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.WalkSpeed);
            
        }


        public override void Exit()
        {
            
        }
    }
}