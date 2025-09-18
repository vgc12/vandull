using StateMachine;

namespace Player.States
{
    public class CrouchWalkState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        
        public CrouchWalkState (PlayerStateMachine sm) => _sm = sm;
        
        public override void Enter()
        {
            _sm.PlayerMovement.Crouch();
        }
        
        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            _sm.PlayerLooking.Look();
            var cameraEffects = _sm.PlayerLooking.CameraBobber;
            cameraEffects.CameraBob(cameraEffects.cameraBobConfig.crouchWalkConfig);
            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.CrouchWalkSpeed);
            
        }

        public override void Exit()
        {
            _sm.PlayerMovement.UnCrouch();
        }
    }
}