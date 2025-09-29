using General;
using Items.Guns;

namespace NPC
{
    public class ReloadStrategy : IActionStrategy
    {
        private readonly Gun _gun;
        public ReloadStrategy(Gun gun)
        {
            _gun = gun;
        }

        public void Start()
        {
            VandullLogger.LogError("ReloadStrategy started");
            _gun.StartReload();

        }

        public bool CanPerform => _gun.AmmoSystem.CurrentMagazineEmpty;
        public bool Complete => !_gun.AmmoSystem.CurrentMagazineEmpty;
    }
}