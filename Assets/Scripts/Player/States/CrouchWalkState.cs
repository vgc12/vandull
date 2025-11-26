using EventBus;
using StateMachine;

namespace Player.States
{
    public sealed class CrouchWalkState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        private readonly PlayerMovementEnteredEvent _stateEntered;
        private readonly PlayerMovementExitedEvent _stateExited;

        public CrouchWalkState(PlayerStateMachine sm)
        {
            _sm = sm;
            _stateEntered = new PlayerMovementEnteredEvent(_sm.PlayerMovement.Rigidbody, typeof(CrouchWalkState));
            _stateExited = new PlayerMovementExitedEvent(_sm.PlayerMovement.Rigidbody, typeof(CrouchWalkState));
        }

        public override void Enter()
        {
            EventBus<PlayerMovementEnteredEvent>.Raise(_stateEntered);
            _sm.PlayerMovement.Crouch();
        }

        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();
            _sm.PlayerLooking.Look();
            /*
            var weaponEffects = _sm.PlayerLooking.weaponBobber;
            var config = _sm.PlayerMovement.IsAiming
                ? _sm.PlayerLooking.weaponBobConfig.aimCrouchWalkConfig
                : _sm.PlayerLooking.weaponBobConfig.crouchWalkConfig;
            weaponEffects.Bob(config,
                _sm.PlayerMovement.Rigidbody.linearVelocity);
                */
            _sm.PlayerLooking.Lean();
        }

        public override void FixedUpdate()
        {
            _sm.PlayerMovement.Move(_sm.PlayerMovement.config.CrouchWalkSpeed);
        }

        public override void Exit()
        {
            EventBus<PlayerMovementExitedEvent>.Raise(_stateExited);
            _sm.PlayerMovement.UnCrouch();
        }
    }
}