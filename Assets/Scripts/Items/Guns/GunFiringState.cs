using StateMachine;

namespace Items.Guns
{
    internal class GunFiringState : BaseState
    {
        private readonly Gun _gun;
        public GunFiringState( Gun gun)
        {
            _gun = gun;
        }
    }
}