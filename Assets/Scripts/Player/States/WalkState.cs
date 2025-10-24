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
            var weaponEffects = _sm.PlayerLooking.weaponBobber;
            weaponEffects.Bob(_sm.PlayerLooking.weaponBobConfig.walkConfig,
                _sm.PlayerMovement.Rigidbody.linearVelocity);
            var cameraEffects = _sm.PlayerLooking.Bobber;
            cameraEffects.Bob(_sm.PlayerLooking.cameraBobConfig.walkConfig,
                _sm.PlayerMovement.Rigidbody.linearVelocity);
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