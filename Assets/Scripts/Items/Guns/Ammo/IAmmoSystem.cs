using System;

namespace Items.Guns.Ammo
{
    public interface IAmmoSystem : IGunSystem
    {
        bool CurrentMagazineEmpty { get; }
        bool IsReloading { get; }
        bool CanReload { get; }
        int CurrentAmmo { get; }
        int TotalAmmo { get; }
        void StartReload();
        void ConsumeAmmo();

        void DropMagazine();

        public event Action<ReloadEvent> OnReloadComplete;
        public event Action OnOutOfAmmo;
    }
}