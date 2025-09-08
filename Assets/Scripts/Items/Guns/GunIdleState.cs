using StateMachine;

namespace Items.Guns
{
    internal class GunIdleState : BaseState
    {
        private readonly Gun _gun;
        public GunIdleState( Gun gun)
        {
            _gun = gun;
        }
        
    }
}