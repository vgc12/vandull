using System;

namespace Items.Guns
{
    public interface IAmmoSystem
    {
        event Action OnAmmoChanged;
        event Action OnReloadStarted;
        event Action OnReloadCompleted;
        
        bool IsCurrentMagazineEmpty { get; }
        bool IsReloading { get; }
        bool CanReload { get; }
        void StartReload();
        void ConsumeAmmo();
        int CurrentAmmo { get; }
        int TotalAmmo { get; }
    }
}