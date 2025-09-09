using StateMachine;

namespace Items.Guns
{
    public class GunReloadingState : BaseState
    {
        private readonly Gun _gun;
        private float _reloadTimer;

        public GunReloadingState(Gun gun)
        {
            _gun = gun;
        }

        public override void Enter()
        {
            _gun.Reload();
        }

        public override void Exit()
        {
          
        }

     
    }
}