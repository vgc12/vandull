using StateMachine;

namespace Items.Guns
{
    internal class GunAimingState : BaseState
    {
        private readonly Gun _gun;
        public GunAimingState( Gun gun)
        {
            _gun = gun;
        }
    }
}