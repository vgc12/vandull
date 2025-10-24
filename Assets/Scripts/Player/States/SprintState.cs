using StateMachine;

namespace Player.States
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

            _sm.PlayerLooking.Look();
            var weaponEffects = _sm.PlayerLooking.weaponBobber;
            weaponEffects.Bob(_sm.PlayerLooking.weaponBobConfig.sprintConfig,
                _sm.PlayerMovement.Rigidbody.linearVelocity);
            var ce = _sm.PlayerLooking.Bobber;
            ce.Bob(_sm.PlayerLooking.cameraBobConfig.sprintConfig, _sm.PlayerMovement.Rigidbody.linearVelocity);

            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.SprintSpeed);
        }

        public override void Exit()
        {
        }
    }
}