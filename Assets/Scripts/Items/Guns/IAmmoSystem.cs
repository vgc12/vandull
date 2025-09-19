using System;

namespace Items.Guns
{
    public interface IAmmoSystem
    {
   

        
        bool IsCurrentMagazineEmpty { get; }
        bool IsReloading { get; }
        bool CanReload { get; }
        void StartReload();
        void ConsumeAmmo();
        int CurrentAmmo { get; }
        int TotalAmmo { get; }
    }
}