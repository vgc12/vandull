using StateMachine;

namespace Player.States
{
    public class IdleState : BaseState
    {
        private readonly PlayerStateMachine _sm;
        

        public IdleState(PlayerStateMachine sm)
        {
            _sm = sm;
        }

        public override void Enter()
        {
            _sm.PlayerLooking.Bobber.StopBobbing();
            _sm.PlayerLooking.weaponBobber.StopBobbing();
        }


        public override void Update()
        {
            _sm.PlayerMovement.ApplyDrag();

            _sm.PlayerLooking.Look();

            var os = _sm.PlayerLooking.objectSwayer;


            os.Sway(_sm.PlayerLooking.swayConfig);

            _sm.PlayerLooking.Lean();
        }


        public override void Exit()
        {
        }
    }
}